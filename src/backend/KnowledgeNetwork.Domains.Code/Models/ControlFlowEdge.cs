namespace KnowledgeNetwork.Domains.Code.Models;

/// <summary>
/// Represents an edge between two basic blocks in a control flow graph
/// </summary>
public class ControlFlowEdge
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
    public EdgeKind Kind { get; set; }

    /// <summary>
    /// Condition for conditional edges (true/false/exception)
    /// </summary>
    public EdgeCondition? Condition { get; set; }

    /// <summary>
    /// Human-readable label for this edge
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Additional metadata for visualization
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Types of control flow edges
/// </summary>
public enum EdgeKind
{
    /// <summary>
    /// Normal sequential flow
    /// </summary>
    Regular,

    /// <summary>
    /// Conditional branch (true path)
    /// </summary>
    ConditionalTrue,

    /// <summary>
    /// Conditional branch (false path)
    /// </summary>
    ConditionalFalse,

    /// <summary>
    /// Back edge indicating a loop
    /// </summary>
    BackEdge,

    /// <summary>
    /// Exception flow edge
    /// </summary>
    Exception,

    /// <summary>
    /// Return statement edge to exit
    /// </summary>
    Return,

    /// <summary>
    /// Switch case edge
    /// </summary>
    SwitchCase,

    /// <summary>
    /// Default case in switch
    /// </summary>
    SwitchDefault
}

/// <summary>
/// Condition information for conditional edges
/// </summary>
public class EdgeCondition
{
    /// <summary>
    /// Boolean result for this edge (true/false for conditionals)
    /// </summary>
    public bool? BooleanValue { get; set; }

    /// <summary>
    /// Case value for switch statements
    /// </summary>
    public string? CaseValue { get; set; }

    /// <summary>
    /// Exception type for exception edges
    /// </summary>
    public string? ExceptionType { get; set; }

    /// <summary>
    /// Human-readable description of the condition
    /// </summary>
    public string Description { get; set; } = string.Empty;
}