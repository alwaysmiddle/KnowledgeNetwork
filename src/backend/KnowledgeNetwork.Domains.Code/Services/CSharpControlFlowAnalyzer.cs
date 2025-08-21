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
public class CSharpControlFlowAnalyzer
{
    /// <summary>
    /// Extract control flow graph from a method body
    /// </summary>
    /// <param name="compilation">Compilation context</param>
    /// <param name="methodDeclaration">Method syntax node</param>
    /// <returns>Control flow graph or null if extraction fails</returns>
    public async Task<CSharpControlFlowGraph?> ExtractControlFlowAsync(
        Compilation compilation,
        MethodDeclarationSyntax methodDeclaration)
    {
        await Task.CompletedTask; // To maintain async signature

        try
        {
            // Get semantic model for the syntax tree
            var semanticModel = compilation.GetSemanticModel(methodDeclaration.SyntaxTree);

            // Get the method symbol
            var methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration);
            if (methodSymbol == null)
            {
                // Console.WriteLine($"CFG extraction: Failed to get method symbol for {methodDeclaration.Identifier}");
                return null;
            }

            // Console.WriteLine($"CFG extraction: Processing method {methodDeclaration.Identifier}");
            // Get the method body operation
            var bodyNode = methodDeclaration.Body ?? (SyntaxNode?)methodDeclaration.ExpressionBody;
            if (bodyNode == null)
            {
                // Console.WriteLine($"CFG extraction: Method {methodDeclaration.Identifier} has no body");
                return null;
            }

            // Console.WriteLine($"CFG extraction: Getting operation for method {methodDeclaration.Identifier}");
            var operation = semanticModel.GetOperation(bodyNode);
            if (operation == null)
            {
                // Console.WriteLine($"CFG extraction: Failed to get operation for method {methodDeclaration.Identifier}");
                return null;
            }

            // Console.WriteLine($"CFG extraction: Got operation type {operation.GetType().Name} for method {methodDeclaration.Identifier}");
            // ControlFlowGraph.Create requires IBlockOperation specifically
            var blockOperation = operation switch
            {
                IBlockOperation directBlock => directBlock,
                IMethodBodyOperation { BlockBody: not null } methodBodyOp => methodBodyOp.BlockBody,
                _ => null
            };

            if (blockOperation == null)
            {
                // System.Diagnostics.Debug.WriteLine($"CFG extraction: No valid block operation found for method {methodDeclaration.Identifier}");
                return null;
            }

            // Create the control flow graph using Roslyn
            var cfg = ControlFlowGraph.Create(blockOperation);
            // System.Diagnostics.Debug.WriteLine($"CFG extraction: Successfully created CFG with {cfg.Blocks.Length} blocks for method {methodDeclaration.Identifier}");

            // Convert to our domain model
            return ConvertToDomainModel(cfg, methodDeclaration, methodSymbol);
        }
        catch (Exception ex)
        {
            // Log error but don't throw - return null to indicate failure
            // System.Diagnostics.Debug.WriteLine($"CFG extraction failed for method {methodDeclaration.Identifier}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Extract control flow graphs for all methods in a syntax tree
    /// </summary>
    /// <param name="compilation">Compilation context</param>
    /// <param name="syntaxTree">Syntax tree to analyze</param>
    /// <returns>List of control flow graphs</returns>
    public async Task<List<CSharpControlFlowGraph>> ExtractAllControlFlowsAsync(
        Compilation compilation,
        SyntaxTree syntaxTree)
    {
        var cfgs = new List<CSharpControlFlowGraph>();

        try
        {
            // Console.WriteLine("CFG extraction: Starting extraction for syntax tree");
            var root = await syntaxTree.GetRootAsync();

            // Find all method declarations
            var methods = root.DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(m => m.Body != null || m.ExpressionBody != null);

            // Console.WriteLine($"CFG extraction: Found {methods.Count()} methods to analyze");

            // Extract CFG for each method
            foreach (var method in methods)
            {
                var cfg = await ExtractControlFlowAsync(compilation, method);
                if (cfg != null)
                {
                    cfgs.Add(cfg);
                }
            }

            // Also handle constructors
            var constructors = root.DescendantNodes()
                .OfType<ConstructorDeclarationSyntax>()
                .Where(c => c.Body != null || c.ExpressionBody != null);

            foreach (var constructor in constructors)
            {
                var cfg = await ExtractControlFlowFromConstructorAsync(compilation, constructor);
                if (cfg != null)
                {
                    cfgs.Add(cfg);
                }
            }

            return cfgs;
        }
        catch (Exception ex)
        {
            // System.Diagnostics.Debug.WriteLine($"CFG extraction failed for syntax tree: {ex.Message}");
            return cfgs; // Return partial results
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Extract CFG from constructor declaration
    /// </summary>
    private async Task<CSharpControlFlowGraph?> ExtractControlFlowFromConstructorAsync(
        Compilation compilation,
        ConstructorDeclarationSyntax constructorDeclaration)
    {
        await Task.CompletedTask;

        try
        {
            var semanticModel = compilation.GetSemanticModel(constructorDeclaration.SyntaxTree);
            var constructorSymbol = semanticModel.GetDeclaredSymbol(constructorDeclaration);
            if (constructorSymbol == null)
            {
                // System.Diagnostics.Debug.WriteLine($"CFG extraction: Failed to get constructor symbol");
                return null;
            }

            var bodyNode = constructorDeclaration.Body ?? (SyntaxNode?)constructorDeclaration.ExpressionBody;
            if (bodyNode == null)
            {
                // System.Diagnostics.Debug.WriteLine($"CFG extraction: Constructor has no body");
                return null;
            }

            var operation = semanticModel.GetOperation(bodyNode);
            if (operation == null)
            {
                // System.Diagnostics.Debug.WriteLine($"CFG extraction: Failed to get operation for constructor");
                return null;
            }

            // ControlFlowGraph.Create requires IBlockOperation specifically
            var blockOperation = operation switch
            {
                IBlockOperation directBlock => directBlock,
                IMethodBodyOperation { BlockBody: not null } methodBodyOp => methodBodyOp.BlockBody,
                _ => null
            };

            if (blockOperation == null)
            {
                // System.Diagnostics.Debug.WriteLine($"CFG extraction: No valid block operation found for constructor");
                return null;
            }

            var cfg = ControlFlowGraph.Create(blockOperation);
            // System.Diagnostics.Debug.WriteLine($"CFG extraction: Successfully created CFG with {cfg.Blocks.Length} blocks for constructor");

            // Convert to our domain model
            var result = ConvertToDomainModel(cfg, constructorDeclaration, constructorSymbol);
            if (result != null)
            {
                result.MethodName =
                    $".ctor({string.Join(", ", constructorDeclaration.ParameterList.Parameters.Select(p => p.Type?.ToString() ?? ""))})";
            }

            return result;
        }
        catch (Exception ex)
        {
            // System.Diagnostics.Debug.WriteLine($"CFG extraction failed for constructor: {ex.Message}");
            return null;
        }
    }
    /// <summary>
    /// Convert Roslyn ControlFlowGraph to our domain model
    /// </summary>
    private CSharpControlFlowGraph ConvertToDomainModel(ControlFlowGraph cfg, SyntaxNode syntaxNode,
        ISymbol methodSymbol)
    {
        var domainCfg = new CSharpControlFlowGraph
        {
            MethodName = GetMethodName(methodSymbol),
            TypeName = methodSymbol.ContainingType?.ToDisplayString() ?? "",
            Location = CreateLocationInfo(syntaxNode)
        };

        // Convert basic blocks
        var blockMap = new Dictionary<BasicBlock, CSharpBasicBlock>();
        var ordinal = 0;

        foreach (var block in cfg.Blocks)
        {
            var domainBlock = ConvertBasicBlock(block, ordinal++);
            domainCfg.BasicBlocks.Add(domainBlock);
            blockMap[block] = domainBlock;
        }

        // Identify entry and exit blocks using Roslyn's structure
        if (cfg.Blocks.Length > 0)
        {
            // Use the first block as entry (Roslyn guarantees this)
            var entryBlock = blockMap[cfg.Blocks[0]];
            entryBlock.Kind = CSharpBasicBlockKind.Entry;
            domainCfg.EntryBlock = entryBlock;

            // Find exit blocks (blocks with no successors or special exit blocks)
            foreach (var kvp in blockMap)
            {
                var roslynBlock = kvp.Key;
                var domainBlock = kvp.Value;

                // Check if this is an exit block (no successors)
                if (roslynBlock.ConditionalSuccessor == null && roslynBlock.FallThroughSuccessor == null)
                {
                    domainBlock.Kind = CSharpBasicBlockKind.Exit;
                    domainCfg.ExitBlock = domainBlock; // Use the last one found
                }
            }
        }

        // Create edges based on actual Roslyn CFG structure
        foreach (var kvp in blockMap)
        {
            var roslynBlock = kvp.Key;
            var sourceBlock = kvp.Value;

            // Add fall-through successor
            if (roslynBlock.FallThroughSuccessor != null && roslynBlock.FallThroughSuccessor.Destination != null && blockMap.ContainsKey(roslynBlock.FallThroughSuccessor.Destination))
            {
                var targetBlock = blockMap[roslynBlock.FallThroughSuccessor.Destination];
                var edge = CreateControlFlowEdge(sourceBlock, targetBlock);
                edge.Kind = CSharpEdgeKind.Regular;
                edge.Label = "fallthrough";

                domainCfg.Edges.Add(edge);
                sourceBlock.Successors.Add(targetBlock.Id);
                targetBlock.Predecessors.Add(sourceBlock.Id);
            }

            // Add conditional successor
            if (roslynBlock.ConditionalSuccessor != null && roslynBlock.ConditionalSuccessor.Destination != null && blockMap.ContainsKey(roslynBlock.ConditionalSuccessor.Destination))
            {
                var targetBlock = blockMap[roslynBlock.ConditionalSuccessor.Destination];
                var edge = CreateControlFlowEdge(sourceBlock, targetBlock);
                edge.Kind = CSharpEdgeKind.ConditionalTrue;
                edge.Label = "condition";

                if (sourceBlock.BranchInfo != null)
                {
                    edge.Condition = new CSharpEdgeCondition
                    {
                        BooleanValue = true,
                        Description = sourceBlock.BranchInfo.Condition
                    };
                }

                domainCfg.Edges.Add(edge);
                sourceBlock.Successors.Add(targetBlock.Id);
                targetBlock.Predecessors.Add(sourceBlock.Id);
            }
        }

        // Calculate reachability
        CalculateReachability(domainCfg);

        // Calculate complexity metrics
        domainCfg.Metrics = CalculateComplexityMetrics(domainCfg);

        return domainCfg;
    }
    /// <summary>
    /// Convert Roslyn BasicBlock to our domain model
    /// </summary>
    private CSharpBasicBlock ConvertBasicBlock(BasicBlock block, int ordinal)
    {
        var domainBlock = new CSharpBasicBlock
        {
            Id = block.Ordinal,
            Ordinal = ordinal,
            Kind = CSharpBasicBlockKind.Block // Will be updated for entry/exit
        };

        // Convert operations
        foreach (var operation in block.Operations)
        {
            var operationInfo = CreateOperationInfo(operation);
            domainBlock.Operations.Add(operationInfo);
        }

        // Simplified branch handling
        var branchValue = block.BranchValue;
        if (branchValue != null)
        {
            domainBlock.BranchInfo = new CSharpBranchInfo
            {
                Condition = GetOperationSummary(branchValue),
                BranchType = DetermineBranchType(branchValue)
            };
        }

        return domainBlock;
    }

    /// <summary>
    /// Create operation info from IOperation
    /// </summary>
    private CSharpOperationInfo CreateOperationInfo(IOperation operation)
    {
        return new CSharpOperationInfo
        {
            OperationKind = operation.Kind.ToString(),
            Syntax = operation.Syntax?.ToString() ?? "",
            Summary = GetOperationSummary(operation),
            Location = operation.Syntax != null ? CreateLocationInfo(operation.Syntax) : null,
            MayThrow = CanOperationThrow(operation)
        };
    }

    /// <summary>
    /// Create control flow edge
    /// </summary>
    private CSharpControlFlowEdge CreateControlFlowEdge(CSharpBasicBlock source, CSharpBasicBlock target)
    {
        var edge = new CSharpControlFlowEdge
        {
            Source = source.Id,
            Target = target.Id
        };

        // Determine edge kind based on block relationship
        if (target.Id <= source.Id)
        {
            edge.Kind = CSharpEdgeKind.BackEdge;
            edge.Label = "loop";
        }
        else if (source.BranchInfo != null)
        {
            // This is a conditional branch
            edge.Kind = CSharpEdgeKind.ConditionalTrue; // Simplified - could be enhanced
            edge.Label = "condition";
            edge.Condition = new CSharpEdgeCondition
            {
                BooleanValue = true,
                Description = source.BranchInfo.Condition
            };
        }
        else
        {
            edge.Kind = CSharpEdgeKind.Regular;
            edge.Label = "";
        }

        return edge;
    }


    /// <summary>
    /// Get human-readable operation summary
    /// </summary>
    private string GetOperationSummary(IOperation? operation)
    {
        if (operation == null) return "";

        return operation.Kind switch
        {
            OperationKind.SimpleAssignment => $"{GetLeftSide(operation)} = {GetRightSide(operation)}",
            OperationKind.VariableDeclarator => $"var {GetVariableName(operation)}",
            OperationKind.Invocation => GetInvocationSummary(operation),
            OperationKind.Return => $"return {GetReturnValue(operation)}",
            OperationKind.Conditional => $"? {GetConditionalExpression(operation)}",
            OperationKind.BinaryOperator => GetBinaryOperatorSummary(operation),
            OperationKind.Loop => GetLoopSummary(operation),
            _ => operation.Syntax?.ToString()?.Trim() ?? operation.Kind.ToString()
        };
    }

    /// <summary>
    /// Calculate reachability from entry block
    /// </summary>
    private void CalculateReachability(CSharpControlFlowGraph cfg)
    {
        if (cfg.EntryBlock == null) return;

        var visited = new HashSet<int>();
        var queue = new Queue<int>();

        queue.Enqueue(cfg.EntryBlock.Id);
        visited.Add(cfg.EntryBlock.Id);

        while (queue.Count > 0)
        {
            var blockId = queue.Dequeue();
            var block = cfg.GetBlock(blockId);
            if (block != null)
            {
                block.IsReachable = true;

                foreach (var successorId in block.Successors)
                {
                    if (!visited.Contains(successorId))
                    {
                        visited.Add(successorId);
                        queue.Enqueue(successorId);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Calculate complexity metrics for the CFG
    /// </summary>
    private CSharpComplexityMetrics CalculateComplexityMetrics(CSharpControlFlowGraph cfg)
    {
        var metrics = new CSharpComplexityMetrics
        {
            BlockCount = cfg.BasicBlocks.Count,
            EdgeCount = cfg.Edges.Count
        };

        // Count decision points (conditional branches)
        metrics.DecisionPoints = cfg.Edges.Count(e =>
            e.Kind == CSharpEdgeKind.ConditionalTrue ||
            e.Kind == CSharpEdgeKind.ConditionalFalse);

        // Count loops (back edges)
        metrics.LoopCount = cfg.Edges.Count(e => e.Kind == CSharpEdgeKind.BackEdge);

        // Calculate cyclomatic complexity: Edges - Nodes + 2
        metrics.CyclomaticComplexity = Math.Max(1, cfg.Edges.Count - cfg.BasicBlocks.Count + 2);

        // Check for exception handling
        metrics.HasExceptionHandling = cfg.BasicBlocks.Any(b => b.Kind == CSharpBasicBlockKind.ExceptionHandler) ||
                                       cfg.Edges.Any(e => e.Kind == CSharpEdgeKind.Exception);

        return metrics;
    }

    #endregion

    #region Helper Methods for Operation Analysis

    private string GetMethodName(ISymbol methodSymbol) =>
        methodSymbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);

    private CSharpLocationInfo CreateLocationInfo(SyntaxNode node)
    {
        var location = node.GetLocation();
        var lineSpan = location.GetLineSpan();

        return new CSharpLocationInfo
        {
            StartLine = lineSpan.StartLinePosition.Line + 1,
            StartColumn = lineSpan.StartLinePosition.Character + 1,
            EndLine = lineSpan.EndLinePosition.Line + 1,
            EndColumn = lineSpan.EndLinePosition.Character + 1
        };
    }

    private CSharpBranchType DetermineBranchType(IOperation operation) => operation.Parent?.Kind switch
    {
        OperationKind.Conditional => CSharpBranchType.Conditional,
        OperationKind.Loop => CSharpBranchType.Loop,
        OperationKind.Switch => CSharpBranchType.Switch,
        _ => CSharpBranchType.Conditional
    };

    private bool CanOperationThrow(IOperation operation) => operation.Kind switch
    {
        OperationKind.Invocation => true,
        OperationKind.ArrayElementReference => true,
        OperationKind.PropertyReference => true,
        OperationKind.Throw => true,
        _ => false
    };

    private string GetLeftSide(IOperation operation) =>
        operation is ISimpleAssignmentOperation assignment ? assignment.Target?.Syntax?.ToString() ?? "" : "";

    private string GetRightSide(IOperation operation) =>
        operation is ISimpleAssignmentOperation assignment ? assignment.Value?.Syntax?.ToString() ?? "" : "";

    private string GetVariableName(IOperation operation) =>
        operation is IVariableDeclaratorOperation declarator ? declarator.Symbol?.Name ?? "" : "";

    private string GetInvocationSummary(IOperation operation) =>
        operation is IInvocationOperation invocation ?
            $"{invocation.TargetMethod?.Name ?? "unknown"}({new string(',', Math.Max(0, invocation.Arguments.Length - 1))})" :
            "method call";

    private string GetReturnValue(IOperation operation) =>
        operation is IReturnOperation returnOp ? returnOp.ReturnedValue?.Syntax?.ToString() ?? "" : "";

    private string GetConditionalExpression(IOperation operation) =>
        operation is IConditionalOperation conditional ? conditional.Condition?.Syntax?.ToString() ?? "" : "";

    private string GetBinaryOperatorSummary(IOperation operation) =>
        operation is IBinaryOperation binary ?
            $"{binary.LeftOperand?.Syntax?.ToString() ?? ""} {binary.OperatorKind} {binary.RightOperand?.Syntax?.ToString() ?? ""}" :
            "";

    private string GetLoopSummary(IOperation operation) =>
        operation is ILoopOperation loop ? loop.LoopKind switch
        {
            LoopKind.For => "for loop",
            LoopKind.ForEach => "foreach loop",
            LoopKind.While => "while loop",
            _ => "loop"
        } : "loop";

    #endregion
}