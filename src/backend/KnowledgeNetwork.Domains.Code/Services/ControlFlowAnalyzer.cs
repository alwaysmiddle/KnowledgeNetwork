using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;
using KnowledgeNetwork.Domains.Code.Models;
using KnowledgeNetwork.Domains.Code.Models.Common;
using KnowledgeNetwork.Domains.Code.Models.ControlFlow;
using KnowledgeNetwork.Domains.Code.Models.Enums;

namespace KnowledgeNetwork.Domains.Code.Services;

/// <summary>
/// Service for extracting control flow graphs from C# code using Roslyn
/// </summary>
public class ControlFlowAnalyzer
{
    /// <summary>
    /// Extract control flow graph from a method body
    /// </summary>
    /// <param name="compilation">Compilation context</param>
    /// <param name="methodDeclaration">Method syntax node</param>
    /// <returns>Control flow graph or null if extraction fails</returns>
    public async Task<Models.ControlFlowGraph?> ExtractControlFlowAsync(
        Compilation compilation, 
        MethodDeclarationSyntax methodDeclaration)
    {
        try
        {
            // Get semantic model
            var semanticModel = compilation.GetSemanticModel(methodDeclaration.SyntaxTree);
            
            // Get method symbol
            var methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration);
            if (methodSymbol == null) return null;

            // Get operation for the method body
            var methodOperation = semanticModel.GetOperation(methodDeclaration);
            if (methodOperation is not IMethodBodyOperation methodBodyOperation) return null;

            // Create Roslyn CFG
            var roslynCfg = Microsoft.CodeAnalysis.FlowAnalysis.ControlFlowGraph.Create(methodBodyOperation);
            if (roslynCfg == null) return null;

            // Convert to our CFG model
            var cfg = ConvertFromRoslynCfg(roslynCfg, methodDeclaration, methodSymbol);
            
            return cfg;
        }
        catch (Exception ex)
        {
            // Log error but don't throw - CFG extraction is optional
            System.Diagnostics.Debug.WriteLine($"CFG extraction failed: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Extract control flow graphs for all methods in a syntax tree
    /// </summary>
    /// <param name="compilation">Compilation context</param>
    /// <param name="syntaxTree">Syntax tree to analyze</param>
    /// <returns>List of control flow graphs</returns>
    public async Task<List<Models.ControlFlowGraph>> ExtractAllControlFlowsAsync(
        Compilation compilation,
        SyntaxTree syntaxTree)
    {
        var cfgs = new List<Models.ControlFlowGraph>();
        var root = await syntaxTree.GetRootAsync();
        
        // Find all method declarations
        var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
        
        foreach (var method in methods)
        {
            var cfg = await ExtractControlFlowAsync(compilation, method);
            if (cfg != null)
            {
                cfgs.Add(cfg);
            }
        }
        
        return cfgs;
    }

    #region Private Helper Methods

    /// <summary>
    /// Convert Roslyn CFG to our CFG model
    /// </summary>
    private Models.ControlFlowGraph ConvertFromRoslynCfg(
        Microsoft.CodeAnalysis.FlowAnalysis.ControlFlowGraph roslynCfg,
        MethodDeclarationSyntax methodSyntax,
        IMethodSymbol methodSymbol)
    {
        var cfg = new Models.ControlFlowGraph
        {
            MethodName = methodSymbol.Name,
            TypeName = methodSymbol.ContainingType.ToDisplayString(),
            Location = GetLocationInfo(methodSyntax)
        };

        // Convert basic blocks
        var blockMap = new Dictionary<Microsoft.CodeAnalysis.FlowAnalysis.BasicBlock, int>();
        for (int i = 0; i < roslynCfg.Blocks.Length; i++)
        {
            var roslynBlock = roslynCfg.Blocks[i];
            blockMap[roslynBlock] = i;
            
            var basicBlock = ConvertBasicBlock(roslynBlock, i);
            cfg.BasicBlocks.Add(basicBlock);
            
            // Set entry and exit blocks
            if (roslynBlock.Kind == Microsoft.CodeAnalysis.FlowAnalysis.BasicBlockKind.Entry)
            {
                cfg.EntryBlock = basicBlock;
            }
            else if (roslynBlock.Kind == Microsoft.CodeAnalysis.FlowAnalysis.BasicBlockKind.Exit)
            {
                cfg.ExitBlock = basicBlock;
            }
        }

        // Convert edges
        foreach (var roslynBlock in roslynCfg.Blocks)
        {
            var sourceId = blockMap[roslynBlock];
            
            // Conditional successor (true path)
            if (roslynBlock.ConditionalSuccessor != null)
            {
                var targetId = blockMap[roslynBlock.ConditionalSuccessor.Destination];
                cfg.Edges.Add(new ControlFlowEdge
                {
                    Source = sourceId,
                    Target = targetId,
                    Kind = EdgeKind.ConditionalTrue,
                    Label = "true",
                    Condition = new EdgeCondition 
                    { 
                        BooleanValue = true, 
                        Description = "Condition is true" 
                    }
                });
            }

            // Fall-through successor (false path or regular flow)
            if (roslynBlock.FallThroughSuccessor != null)
            {
                var targetId = blockMap[roslynBlock.FallThroughSuccessor.Destination];
                var edgeKind = roslynBlock.ConditionalSuccessor != null 
                    ? EdgeKind.ConditionalFalse 
                    : EdgeKind.Regular;
                var label = roslynBlock.ConditionalSuccessor != null ? "false" : "";
                
                cfg.Edges.Add(new ControlFlowEdge
                {
                    Source = sourceId,
                    Target = targetId,
                    Kind = edgeKind,
                    Label = label,
                    Condition = roslynBlock.ConditionalSuccessor != null 
                        ? new EdgeCondition 
                        { 
                            BooleanValue = false, 
                            Description = "Condition is false" 
                        }
                        : null
                });
            }
        }

        // Calculate metrics
        cfg.Metrics = CalculateMetrics(cfg, roslynCfg);

        return cfg;
    }

    /// <summary>
    /// Convert a Roslyn basic block to our model
    /// </summary>
    private Models.BasicBlock ConvertBasicBlock(Microsoft.CodeAnalysis.FlowAnalysis.BasicBlock roslynBlock, int id)
    {
        var block = new Models.BasicBlock
        {
            Id = id,
            Ordinal = roslynBlock.Ordinal,
            Kind = ConvertBlockKind(roslynBlock.Kind),
            IsReachable = roslynBlock.IsReachable
        };

        // Convert operations
        foreach (var operation in roslynBlock.Operations)
        {
            var operationInfo = ConvertOperation(operation);
            block.Operations.Add(operationInfo);
        }

        // Extract branch information if this is a conditional block
        if (roslynBlock.BranchValue != null)
        {
            block.BranchInfo = ExtractBranchInfo(roslynBlock);
        }

        return block;
    }

    /// <summary>
    /// Convert Roslyn basic block kind to our enum
    /// </summary>
    private KnowledgeNetwork.Domains.Code.Models.Enums.BasicBlockKind ConvertBlockKind(Microsoft.CodeAnalysis.FlowAnalysis.BasicBlockKind roslynKind)
    {
        return roslynKind switch
        {
            Microsoft.CodeAnalysis.FlowAnalysis.BasicBlockKind.Entry => KnowledgeNetwork.Domains.Code.Models.Enums.BasicBlockKind.Entry,
            Microsoft.CodeAnalysis.FlowAnalysis.BasicBlockKind.Exit => KnowledgeNetwork.Domains.Code.Models.Enums.BasicBlockKind.Exit,
            Microsoft.CodeAnalysis.FlowAnalysis.BasicBlockKind.Block => KnowledgeNetwork.Domains.Code.Models.Enums.BasicBlockKind.Block,
            _ => KnowledgeNetwork.Domains.Code.Models.Enums.BasicBlockKind.Block
        };
    }

    /// <summary>
    /// Convert an operation to our operation info model
    /// </summary>
    private OperationInfo ConvertOperation(IOperation operation)
    {
        var info = new OperationInfo
        {
            OperationKind = operation.Kind.ToString(),
            Syntax = operation.Syntax?.ToString() ?? "",
            Summary = GenerateOperationSummary(operation),
            Location = GetLocationInfo(operation.Syntax),
            MayThrow = OperationMayThrow(operation)
        };

        return info;
    }

    /// <summary>
    /// Generate a human-readable summary for an operation
    /// </summary>
    private string GenerateOperationSummary(IOperation operation)
    {
        return operation.Kind switch
        {
            OperationKind.SimpleAssignment => ExtractAssignmentSummary(operation),
            OperationKind.Invocation => ExtractInvocationSummary(operation),
            OperationKind.VariableDeclaration => ExtractVariableDeclarationSummary(operation),
            OperationKind.Return => "return",
            OperationKind.Conditional => ExtractConditionalSummary(operation),
            OperationKind.Loop => ExtractLoopSummary(operation),
            _ => operation.Kind.ToString()
        };
    }

    /// <summary>
    /// Extract assignment operation summary
    /// </summary>
    private string ExtractAssignmentSummary(IOperation operation)
    {
        if (operation is ISimpleAssignmentOperation assignment)
        {
            var target = assignment.Target?.Syntax?.ToString() ?? "?";
            var value = assignment.Value?.Syntax?.ToString() ?? "?";
            return $"{target} = {value}";
        }
        return "assignment";
    }

    /// <summary>
    /// Extract method invocation summary
    /// </summary>
    private string ExtractInvocationSummary(IOperation operation)
    {
        if (operation is IInvocationOperation invocation)
        {
            var methodName = invocation.TargetMethod.Name;
            var argCount = invocation.Arguments.Length;
            return $"{methodName}({string.Join(", ", Enumerable.Repeat("_", argCount))})";
        }
        return "method call";
    }

    /// <summary>
    /// Extract variable declaration summary
    /// </summary>
    private string ExtractVariableDeclarationSummary(IOperation operation)
    {
        if (operation is IVariableDeclarationOperation declaration)
        {
            var variables = declaration.Declarators
                .Select(d => d.Symbol.Name)
                .ToList();
            return $"var {string.Join(", ", variables)}";
        }
        return "variable declaration";
    }

    /// <summary>
    /// Extract conditional statement summary
    /// </summary>
    private string ExtractConditionalSummary(IOperation operation)
    {
        if (operation is IConditionalOperation conditional)
        {
            var condition = conditional.Condition?.Syntax?.ToString() ?? "?";
            return $"if ({condition})";
        }
        return "conditional";
    }

    /// <summary>
    /// Extract loop statement summary
    /// </summary>
    private string ExtractLoopSummary(IOperation operation)
    {
        return operation.Syntax switch
        {
            WhileStatementSyntax => "while loop",
            ForStatementSyntax => "for loop",
            ForEachStatementSyntax => "foreach loop",
            DoStatementSyntax => "do-while loop",
            _ => "loop"
        };
    }

    /// <summary>
    /// Check if an operation may throw an exception
    /// </summary>
    private bool OperationMayThrow(IOperation operation)
    {
        return operation.Kind switch
        {
            OperationKind.Invocation => true,
            OperationKind.PropertyReference => true,
            OperationKind.ArrayElementReference => true,
            OperationKind.Throw => true,
            _ => false
        };
    }

    /// <summary>
    /// Extract branch information from a basic block
    /// </summary>
    private BranchInfo ExtractBranchInfo(Microsoft.CodeAnalysis.FlowAnalysis.BasicBlock roslynBlock)
    {
        var branchInfo = new BranchInfo();

        if (roslynBlock.BranchValue != null)
        {
            branchInfo.Condition = roslynBlock.BranchValue.Syntax?.ToString() ?? "";
            branchInfo.BranchType = KnowledgeNetwork.Domains.Code.Models.Enums.BranchType.Conditional;
        }

        return branchInfo;
    }

    /// <summary>
    /// Get location information from syntax node
    /// </summary>
    private LocationInfo? GetLocationInfo(SyntaxNode? syntax)
    {
        if (syntax == null) return null;

        var location = syntax.GetLocation();
        if (!location.IsInSource) return null;

        var lineSpan = location.GetLineSpan();
        return new LocationInfo
        {
            StartLine = lineSpan.StartLinePosition.Line + 1,
            EndLine = lineSpan.EndLinePosition.Line + 1,
            StartColumn = lineSpan.StartLinePosition.Character,
            EndColumn = lineSpan.EndLinePosition.Character,
            FilePath = lineSpan.Path
        };
    }

    /// <summary>
    /// Calculate complexity metrics for the CFG
    /// </summary>
    private ComplexityMetrics CalculateMetrics(
        Models.ControlFlowGraph cfg, 
        Microsoft.CodeAnalysis.FlowAnalysis.ControlFlowGraph roslynCfg)
    {
        var metrics = new ComplexityMetrics
        {
            BlockCount = cfg.BasicBlocks.Count,
            EdgeCount = cfg.Edges.Count
        };

        // Calculate cyclomatic complexity: M = E - N + 2P
        // Where E = edges, N = nodes, P = connected components (usually 1)
        metrics.CyclomaticComplexity = Math.Max(1, cfg.Edges.Count - cfg.BasicBlocks.Count + 2);

        // Count decision points (conditional branches)
        metrics.DecisionPoints = cfg.Edges.Count(e => 
            e.Kind == EdgeKind.ConditionalTrue || e.Kind == EdgeKind.ConditionalFalse);

        // Count loops (back edges)
        metrics.LoopCount = cfg.Edges.Count(e => e.Kind == EdgeKind.BackEdge);

        // Check for exception handling
        metrics.HasExceptionHandling = cfg.BasicBlocks.Any(b => b.Kind == KnowledgeNetwork.Domains.Code.Models.Enums.BasicBlockKind.ExceptionHandler);

        return metrics;
    }

    #endregion
}