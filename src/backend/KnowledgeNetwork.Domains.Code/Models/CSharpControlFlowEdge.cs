using KnowledgeNetwork.Domains.Code.Models.ControlFlow;
using KnowledgeNetwork.Domains.Code.Models.Enums;

namespace KnowledgeNetwork.Domains.Code.Models;

/// <summary>
/// Represents an edge between two basic blocks in a control flow graph
/// </summary>
public class CSharpControlFlowEdge
{
    /// <summary>
    /// ID of the source basic block
    /// </summary>
    public int Source { get; set; }

    /// <summary>
    /// ID of the target basic block
    /// </summary>
    public int Target { get; set; }

    /// <summary>
    /// Type of control flow edge
    /// </summary>
    public CSharpEdgeKind Kind { get; set; }

    /// <summary>
    /// Condition for conditional edges (true/false/exception)
    /// </summary>
    public CSharpEdgeCondition? Condition { get; set; }

    /// <summary>
    /// Human-readable label for this edge
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Additional metadata for visualization
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}