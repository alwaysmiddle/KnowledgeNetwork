using KnowledgeNetwork.Domains.Code.Models.Common;
using KnowledgeNetwork.Domains.Code.Models.ControlFlow;
using KnowledgeNetwork.Domains.Code.Models.Enums;

namespace KnowledgeNetwork.Domains.Code.Models;

/// <summary>
/// Represents a basic block in a control flow graph.
/// A basic block is a sequence of operations with single entry and exit points.
/// </summary>
public class KnCSharpBasicBlock
{
    /// <summary>
    /// Unique identifier for this basic block
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Ordinal position in the control flow graph
    /// </summary>
    public int Ordinal { get; set; }

    /// <summary>
    /// Type of basic block (Entry, Exit, Regular, etc.)
    /// </summary>
    public KnCSharpBasicBlockKind Kind { get; set; }

    /// <summary>
    /// Operations contained in this basic block
    /// </summary>
    public List<KnCSharpOperationInfo> Operations { get; set; } = new();

    /// <summary>
    /// IDs of basic blocks that can execute before this one
    /// </summary>
    public List<int> Predecessors { get; set; } = new();

    /// <summary>
    /// IDs of basic blocks that can execute after this one
    /// </summary>
    public List<int> Successors { get; set; } = new();

    /// <summary>
    /// Whether this block is reachable from the entry block
    /// </summary>
    public bool IsReachable { get; set; }

    /// <summary>
    /// Conditional branch information if this block ends with a condition
    /// </summary>
    public KnCSharpBranchInfo? BranchInfo { get; set; }

    /// <summary>
    /// Source location information for this block
    /// </summary>
    public KnCSharpLocationInfo? Location { get; set; }

    /// <summary>
    /// Additional metadata for visualization
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}