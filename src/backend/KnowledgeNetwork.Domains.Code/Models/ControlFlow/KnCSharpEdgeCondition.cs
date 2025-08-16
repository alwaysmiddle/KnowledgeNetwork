namespace KnowledgeNetwork.Domains.Code.Models.ControlFlow;

/// <summary>
/// Condition information for conditional edges
/// </summary>
public class KnCSharpEdgeCondition
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