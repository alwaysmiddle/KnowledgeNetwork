using KnowledgeNetwork.Domains.Code.Analyzers.Blocks.Abstractions;
using KnowledgeNetwork.Domains.Code.Models.Blocks;
using KnowledgeNetwork.Domains.Code.Models;
using KnowledgeNetwork.Domains.Code.Models.Common;
using KnowledgeNetwork.Domains.Code.Models.ControlFlow;
using KnowledgeNetwork.Domains.Code.Models.Enums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.Extensions.Logging;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Blocks;

/// <summary>
/// Implementation of IDomainModelConverter that handles conversion from Roslyn's
/// ControlFlowGraph to our domain-specific MethodBlockGraph model. This service
/// encapsulates all the complexity of domain model mapping and metadata calculation.
/// </summary>
public class DomainModelConverter(ILogger<DomainModelConverter> logger) : IDomainModelConverter
{
    private readonly ILogger<DomainModelConverter> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Convert a Roslyn ControlFlowGraph to our domain model MethodBlockGraph
    /// </summary>
    public async Task<MethodBlockGraph> ConvertToDomainModelAsync(
        ControlFlowGraph cfg, 
        SyntaxNode syntaxNode, 
        ISymbol methodSymbol)
    {
        await Task.CompletedTask; // Maintain async signature

        try
        {
            _logger.LogDebug("Converting CFG to domain model for {MemberName}", methodSymbol.Name);

            var domainCfg = new MethodBlockGraph
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
            CreateControlFlowEdges(blockMap, domainCfg);

            // Calculate reachability
            CalculateReachability(domainCfg);

            // Calculate complexity metrics
            domainCfg.Metrics = CalculateComplexityMetrics(domainCfg);

            _logger.LogDebug("Successfully converted CFG for {MemberName}: {BlockCount} blocks, {EdgeCount} edges",
                methodSymbol.Name, domainCfg.BasicBlocks.Count, domainCfg.Edges.Count);

            return domainCfg;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert CFG to domain model for {MemberName}", methodSymbol.Name);
            throw;
        }
    }

    /// <summary>
    /// Convert a Roslyn ControlFlowGraph for a constructor with special naming
    /// </summary>
    public async Task<MethodBlockGraph> ConvertConstructorToDomainModelAsync(
        ControlFlowGraph cfg,
        ConstructorDeclarationSyntax constructorDeclaration,
        ISymbol constructorSymbol)
    {
        var result = await ConvertToDomainModelAsync(cfg, constructorDeclaration, constructorSymbol);
        
        // Override method name for constructors
        result.MethodName = $".ctor({string.Join(", ", 
            constructorDeclaration.ParameterList.Parameters.Select(p => p.Type?.ToString() ?? ""))})";
        
        return result;
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
    /// Create control flow edges based on Roslyn CFG structure
    /// </summary>
    private void CreateControlFlowEdges(Dictionary<BasicBlock, CSharpBasicBlock> blockMap, MethodBlockGraph domainCfg)
    {
        foreach (var kvp in blockMap)
        {
            var roslynBlock = kvp.Key;
            var sourceBlock = kvp.Value;

            // Add fall-through successor
            if (roslynBlock.FallThroughSuccessor != null && roslynBlock.FallThroughSuccessor.Destination != null &&
                blockMap.ContainsKey(roslynBlock.FallThroughSuccessor.Destination))
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
            if (roslynBlock.ConditionalSuccessor != null && roslynBlock.ConditionalSuccessor.Destination != null &&
                blockMap.ContainsKey(roslynBlock.ConditionalSuccessor.Destination))
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
    /// Calculate reachability from entry block
    /// </summary>
    private void CalculateReachability(MethodBlockGraph cfg)
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
    private CSharpComplexityMetrics CalculateComplexityMetrics(MethodBlockGraph cfg)
    {
        var metrics = new CSharpComplexityMetrics
        {
            BlockCount = cfg.BasicBlocks.Count,
            EdgeCount = cfg.Edges.Count,
            // Count decision points (conditional branches)
            DecisionPoints = cfg.Edges.Count(e =>
                e.Kind == CSharpEdgeKind.ConditionalTrue ||
                e.Kind == CSharpEdgeKind.ConditionalFalse),
            // Count loops (back edges)
            LoopCount = cfg.Edges.Count(e => e.Kind == CSharpEdgeKind.BackEdge),
            // Calculate cyclomatic complexity: Edges - Nodes + 2
            CyclomaticComplexity = Math.Max(1, cfg.Edges.Count - cfg.BasicBlocks.Count + 2),
            // Check for exception handling
            HasExceptionHandling = cfg.BasicBlocks.Any(b => b.Kind == CSharpBasicBlockKind.ExceptionHandler) ||
                                   cfg.Edges.Any(e => e.Kind == CSharpEdgeKind.Exception)
        };

        return metrics;
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
        operation is IInvocationOperation invocation
            ? $"{invocation.TargetMethod?.Name ?? "unknown"}({new string(',', Math.Max(0, invocation.Arguments.Length - 1))})"
            : "method call";

    private string GetReturnValue(IOperation operation) =>
        operation is IReturnOperation returnOp ? returnOp.ReturnedValue?.Syntax?.ToString() ?? "" : "";

    private string GetConditionalExpression(IOperation operation) =>
        operation is IConditionalOperation conditional ? conditional.Condition?.Syntax?.ToString() ?? "" : "";

    private string GetBinaryOperatorSummary(IOperation operation) =>
        operation is IBinaryOperation binary
            ? $"{binary.LeftOperand?.Syntax?.ToString() ?? ""} {binary.OperatorKind} {binary.RightOperand?.Syntax?.ToString() ?? ""}"
            : "";

    private string GetLoopSummary(IOperation operation) =>
        operation is ILoopOperation loop
            ? loop.LoopKind switch
            {
                LoopKind.For => "for loop",
                LoopKind.ForEach => "foreach loop",
                LoopKind.While => "while loop",
                _ => "loop"
            }
            : "loop";
}