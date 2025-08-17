using KnowledgeNetwork.Core.Models.Core;
using KnowledgeNetwork.Core.Models.Constants;
using KnowledgeNetwork.Domains.Code.Models;
using KnowledgeNetwork.Domains.Code.Models.Enums;
using KnowledgeNetwork.Domains.Code.Models.ControlFlow;

namespace KnowledgeNetwork.Domains.Code.Converters;

/// <summary>
/// Converts Control Flow Graph models to unified KnowledgeNode format
/// </summary>
public class CfgToKnowledgeNodeConverter
{
    /// <summary>
    /// Convert a complete CFG to a KnowledgeNode representing the method and all its components
    /// </summary>
    /// <param name="cfg">Control Flow Graph to convert</param>
    /// <returns>Method node containing all basic blocks and relationships</returns>
    public KnowledgeNode ConvertCfgToKnowledgeNode(CSharpControlFlowGraph cfg)
    {
        var methodId = $"method-{cfg.TypeName}-{cfg.MethodName}";
        
        // Create all basic block nodes first
        var blockNodes = new List<KnowledgeNode>();
        foreach (var block in cfg.BasicBlocks)
        {
            var blockNode = ConvertBasicBlockToNode(block, methodId);
            blockNodes.Add(blockNode);
        }

        // Add relationships for control flow edges
        foreach (var edge in cfg.Edges)
        {
            var sourceNode = blockNodes.FirstOrDefault(n => n.Id.EndsWith($"-{edge.Source}"));
            var targetNode = blockNodes.FirstOrDefault(n => n.Id.EndsWith($"-{edge.Target}"));

            if (sourceNode != null && targetNode != null)
            {
                var relationship = ConvertEdgeToRelationship(edge, targetNode.Id);
                sourceNode.Relationships.Add(relationship);

                // Add reverse relationship
                var reverseRelationship = ConvertEdgeToReverseRelationship(edge, sourceNode.Id);
                targetNode.Relationships.Add(reverseRelationship);
            }
        }

        // Create the method node that contains all blocks
        var methodNode = new KnowledgeNode
        {
            Id = methodId,
            Type = new NodeType
            {
                Primary = PrimaryNodeType.Method,
                Secondary = "csharp-method",
                Custom = GetMethodCustomType(cfg)
            },
            Label = cfg.MethodName,
            SourceLanguage = "csharp",
            Contains = blockNodes.Select((node, index) => new NodeReference
            {
                NodeId = node.Id,
                Role = GetBlockRole(cfg.BasicBlocks.FirstOrDefault(b => node.Id.EndsWith($"-{b.Id}"))),
                Order = index
            }).ToList(),
            Properties = new Dictionary<string, object?>
            {
                ["typeName"] = cfg.TypeName,
                ["methodName"] = cfg.MethodName,
                ["entryBlockId"] = cfg.EntryBlock?.Id,
                ["exitBlockId"] = cfg.ExitBlock?.Id,
                ["totalBlocks"] = cfg.BasicBlocks.Count,
                ["totalEdges"] = cfg.Edges.Count,
                ["sourceLocation"] = cfg.Location != null ? new
                {
                    startLine = cfg.Location.StartLine,
                    endLine = cfg.Location.EndLine,
                    startColumn = cfg.Location.StartColumn,
                    endColumn = cfg.Location.EndColumn,
                    filePath = cfg.Location.FilePath
                } : null
            },
            Metrics = new NodeMetrics
            {
                Complexity = cfg.Metrics.CyclomaticComplexity,
                NodeCount = cfg.BasicBlocks.Count,
                EdgeCount = cfg.Edges.Count,
                CustomMetrics = new Dictionary<string, object?>
                {
                    ["decisionPoints"] = cfg.Metrics.DecisionPoints,
                    ["loopCount"] = cfg.Metrics.LoopCount,
                    ["unreachableBlocks"] = cfg.BasicBlocks.Count(b => !b.IsReachable)
                }
            },
            Visualization = new VisualizationHints
            {
                PreferredLayout = "cfg-timeline",
                Collapsed = false,
                Color = GetMethodVisualizationColor(cfg.Metrics.CyclomaticComplexity)
            },
            IsView = false,
            IsPersisted = true
        };

        return methodNode;
    }

    /// <summary>
    /// Get all nodes from a CFG (method + all basic blocks + operations)
    /// </summary>
    /// <param name="cfg">Control Flow Graph to convert</param>
    /// <param name="includeOperations">Whether to include operation nodes</param>
    /// <returns>List of all nodes in the CFG</returns>
    public List<KnowledgeNode> ConvertCfgToAllNodes(CSharpControlFlowGraph cfg, bool includeOperations = true)
    {
        var nodes = new List<KnowledgeNode>();
        var methodId = $"method-{cfg.TypeName}-{cfg.MethodName}";
        
        // Create all basic block nodes
        var blockNodes = new List<KnowledgeNode>();
        foreach (var block in cfg.BasicBlocks)
        {
            var blockNode = ConvertBasicBlockToNode(block, methodId, includeOperations);
            blockNodes.Add(blockNode);

            // Add operation nodes if requested
            if (includeOperations)
            {
                for (int i = 0; i < block.Operations.Count; i++)
                {
                    var operationNode = ConvertOperationToNode(block.Operations[i], blockNode.Id, i);
                    nodes.Add(operationNode);
                }
            }
        }

        nodes.AddRange(blockNodes);

        // Add control flow relationships between blocks
        foreach (var edge in cfg.Edges)
        {
            var sourceNode = blockNodes.FirstOrDefault(n => n.Id.EndsWith($"-{edge.Source}"));
            var targetNode = blockNodes.FirstOrDefault(n => n.Id.EndsWith($"-{edge.Target}"));

            if (sourceNode != null && targetNode != null)
            {
                var relationship = ConvertEdgeToRelationship(edge, targetNode.Id);
                sourceNode.Relationships.Add(relationship);

                var reverseRelationship = ConvertEdgeToReverseRelationship(edge, sourceNode.Id);
                targetNode.Relationships.Add(reverseRelationship);
            }
        }

        // Create the method node directly (avoiding duplicate block creation)
        var methodNode = new KnowledgeNode
        {
            Id = methodId,
            Type = new NodeType
            {
                Primary = PrimaryNodeType.Method,
                Secondary = "csharp-method",
                Custom = GetMethodCustomType(cfg)
            },
            Label = cfg.MethodName,
            SourceLanguage = "csharp",
            Contains = blockNodes.Select((node, index) => new NodeReference
            {
                NodeId = node.Id,
                Role = GetBlockRole(cfg.BasicBlocks.FirstOrDefault(b => node.Id.EndsWith($"-{b.Id}"))),
                Order = index
            }).ToList(),
            Properties = new Dictionary<string, object?>
            {
                ["typeName"] = cfg.TypeName,
                ["methodName"] = cfg.MethodName,
                ["entryBlockId"] = cfg.EntryBlock?.Id,
                ["exitBlockId"] = cfg.ExitBlock?.Id,
                ["totalBlocks"] = cfg.BasicBlocks.Count,
                ["totalEdges"] = cfg.Edges.Count,
                ["sourceLocation"] = cfg.Location != null ? new
                {
                    startLine = cfg.Location.StartLine,
                    endLine = cfg.Location.EndLine,
                    startColumn = cfg.Location.StartColumn,
                    endColumn = cfg.Location.EndColumn,
                    filePath = cfg.Location.FilePath
                } : null
            },
            Metrics = new NodeMetrics
            {
                Complexity = cfg.Metrics.CyclomaticComplexity,
                NodeCount = cfg.BasicBlocks.Count,
                EdgeCount = cfg.Edges.Count,
                CustomMetrics = new Dictionary<string, object?>
                {
                    ["decisionPoints"] = cfg.Metrics.DecisionPoints,
                    ["loopCount"] = cfg.Metrics.LoopCount,
                    ["unreachableBlocks"] = cfg.BasicBlocks.Count(b => !b.IsReachable)
                }
            },
            Visualization = new VisualizationHints
            {
                PreferredLayout = "cfg-timeline",
                Collapsed = false,
                Color = GetMethodVisualizationColor(cfg.Metrics.CyclomaticComplexity)
            },
            IsView = false,
            IsPersisted = true
        };

        nodes.Insert(0, methodNode); // Add method node first
        
        return nodes;
    }

    private KnowledgeNode ConvertBasicBlockToNode(CSharpBasicBlock block, string methodId, bool includeOperations = true)
    {
        var blockId = $"block-{methodId}-{block.Id}";
        
        return new KnowledgeNode
        {
            Id = blockId,
            Type = new NodeType
            {
                Primary = PrimaryNodeType.BasicBlock,
                Secondary = GetBasicBlockSecondaryType(block.Kind),
                Custom = block.Kind.ToString().ToLowerInvariant()
            },
            Label = $"Block {block.Ordinal}",
            SourceLanguage = "csharp",
            Contains = includeOperations ? block.Operations.Select((op, index) => new NodeReference
            {
                NodeId = $"op-{blockId}-{index}",
                Role = "operation",
                Order = index
            }).ToList() : new List<NodeReference>(),
            Properties = new Dictionary<string, object?>
            {
                ["ordinal"] = block.Ordinal,
                ["kind"] = block.Kind.ToString(),
                ["isReachable"] = block.IsReachable,
                ["operationCount"] = block.Operations.Count,
                ["predecessors"] = block.Predecessors,
                ["successors"] = block.Successors,
                ["branchInfo"] = block.BranchInfo != null ? new
                {
                    branchType = block.BranchInfo.BranchType.ToString(),
                    condition = block.BranchInfo.Condition,
                    trueTarget = block.BranchInfo.TrueTarget,
                    falseTarget = block.BranchInfo.FalseTarget
                } : null,
                ["sourceLocation"] = block.Location != null ? new
                {
                    startLine = block.Location.StartLine,
                    endLine = block.Location.EndLine,
                    startColumn = block.Location.StartColumn,
                    endColumn = block.Location.EndColumn
                } : null
            },
            Metrics = new NodeMetrics
            {
                NodeCount = block.Operations.Count,
                CustomMetrics = new Dictionary<string, object?>
                {
                    ["predecessorCount"] = block.Predecessors.Count,
                    ["successorCount"] = block.Successors.Count
                }
            },
            Visualization = new VisualizationHints
            {
                PreferredLayout = "cfg-timeline",
                Collapsed = !includeOperations,
                Color = GetBasicBlockVisualizationColor(block.Kind),
                Icon = GetBasicBlockIcon(block.Kind)
            },
            IsView = false,
            IsPersisted = true
        };
    }

    private KnowledgeNode ConvertOperationToNode(CSharpOperationInfo operation, string blockId, int index)
    {
        var operationId = $"op-{blockId}-{index}";
        
        return new KnowledgeNode
        {
            Id = operationId,
            Type = new NodeType
            {
                Primary = "operation",
                Secondary = operation.OperationKind.ToLowerInvariant(),
                Custom = operation.OperationKind
            },
            Label = operation.Summary,
            SourceLanguage = "csharp",
            Properties = new Dictionary<string, object?>
            {
                ["operationKind"] = operation.OperationKind,
                ["syntax"] = operation.Syntax,
                ["summary"] = operation.Summary,
                ["mayThrow"] = operation.MayThrow,
                ["index"] = index,
                ["sourceLocation"] = operation.Location != null ? new
                {
                    startLine = operation.Location.StartLine,
                    endLine = operation.Location.EndLine,
                    startColumn = operation.Location.StartColumn,
                    endColumn = operation.Location.EndColumn
                } : null
            },
            Visualization = new VisualizationHints
            {
                PreferredLayout = "cfg-timeline",
                Collapsed = true,
                Color = GetOperationVisualizationColor(operation.OperationKind),
                Icon = GetOperationIcon(operation.OperationKind)
            },
            IsView = false,
            IsPersisted = true
        };
    }

    private RelationshipPair ConvertEdgeToRelationship(CSharpControlFlowEdge edge, string targetNodeId)
    {
        var relationshipType = GetRelationshipTypeForEdge(edge.Kind);
        
        return new RelationshipPair
        {
            Type = relationshipType,
            Direction = "outgoing",
            TargetNodeId = targetNodeId,
            Metadata = new Dictionary<string, object?>
            {
                ["edgeKind"] = edge.Kind.ToString(),
                ["condition"] = edge.Condition?.ToString(),
                ["isConditional"] = edge.Kind == CSharpEdgeKind.ConditionalTrue || edge.Kind == CSharpEdgeKind.ConditionalFalse,
                ["isBackEdge"] = edge.Kind == CSharpEdgeKind.BackEdge
            }
        };
    }

    private RelationshipPair ConvertEdgeToReverseRelationship(CSharpControlFlowEdge edge, string sourceNodeId)
    {
        var relationshipType = GetRelationshipTypeForEdge(edge.Kind);
        
        return new RelationshipPair
        {
            Type = new RelationshipType
            {
                Forward = relationshipType.Reverse,
                Reverse = relationshipType.Forward,
                Category = relationshipType.Category
            },
            Direction = "incoming",
            TargetNodeId = sourceNodeId,
            Metadata = new Dictionary<string, object?>
            {
                ["edgeKind"] = edge.Kind.ToString(),
                ["condition"] = edge.Condition?.ToString(),
                ["isConditional"] = edge.Kind == CSharpEdgeKind.ConditionalTrue || edge.Kind == CSharpEdgeKind.ConditionalFalse,
                ["isBackEdge"] = edge.Kind == CSharpEdgeKind.BackEdge
            }
        };
    }

    private RelationshipType GetRelationshipTypeForEdge(CSharpEdgeKind edgeKind)
    {
        return edgeKind switch
        {
            CSharpEdgeKind.Regular => RelationshipTypes.FlowsTo,
            CSharpEdgeKind.ConditionalTrue => RelationshipTypes.BranchesTo,
            CSharpEdgeKind.ConditionalFalse => RelationshipTypes.BranchesTo,
            CSharpEdgeKind.BackEdge => RelationshipTypes.LoopsTo,
            _ => RelationshipTypes.FlowsTo
        };
    }

    private string GetMethodCustomType(CSharpControlFlowGraph cfg)
    {
        if (cfg.Metrics.LoopCount > 0 && cfg.Metrics.DecisionPoints > 2)
            return "complex-method";
        if (cfg.Metrics.LoopCount > 0)
            return "loop-method";
        if (cfg.Metrics.DecisionPoints > 0)
            return "conditional-method";
        return "simple-method";
    }

    private string GetBlockRole(CSharpBasicBlock? block)
    {
        if (block == null) return "block";
        
        return block.Kind switch
        {
            CSharpBasicBlockKind.Entry => "entry",
            CSharpBasicBlockKind.Exit => "exit",
            CSharpBasicBlockKind.Block => "regular",
            _ => "block"
        };
    }

    private string GetBasicBlockSecondaryType(CSharpBasicBlockKind kind)
    {
        return kind switch
        {
            CSharpBasicBlockKind.Entry => "entry-block",
            CSharpBasicBlockKind.Exit => "exit-block",
            CSharpBasicBlockKind.Block => "regular-block",
            _ => "basic-block"
        };
    }

    private string GetMethodVisualizationColor(int complexity)
    {
        return complexity switch
        {
            <= 5 => "#4CAF50", // Green - simple
            <= 10 => "#FF9800", // Orange - moderate
            _ => "#F44336" // Red - complex
        };
    }

    private string GetBasicBlockVisualizationColor(CSharpBasicBlockKind kind)
    {
        return kind switch
        {
            CSharpBasicBlockKind.Entry => "#2196F3", // Blue
            CSharpBasicBlockKind.Exit => "#9C27B0", // Purple
            CSharpBasicBlockKind.Block => "#607D8B", // Blue Grey
            _ => "#9E9E9E" // Grey
        };
    }

    private string GetBasicBlockIcon(CSharpBasicBlockKind kind)
    {
        return kind switch
        {
            CSharpBasicBlockKind.Entry => "play_arrow",
            CSharpBasicBlockKind.Exit => "stop",
            CSharpBasicBlockKind.Block => "crop_square",
            _ => "help"
        };
    }

    private string GetOperationVisualizationColor(string kind)
    {
        return kind switch
        {
            "Assignment" => "#8BC34A", // Light Green
            "Invocation" => "#FF5722", // Deep Orange
            "Conditional" => "#E91E63", // Pink
            "Return" => "#673AB7", // Deep Purple
            _ => "#795548" // Brown
        };
    }

    private string GetOperationIcon(string operationKind)
    {
        return operationKind.ToLowerInvariant() switch
        {
            "assignment" => "assignment",
            "invocation" => "call_made",
            "conditional" => "help_outline",
            "return" => "keyboard_return",
            "variable" => "storage",
            _ => "code"
        };
    }
}