namespace KnowledgeNetwork.Domains.Code.Models.Common;

/// <summary>
/// Complexity metrics for a control flow graph
/// </summary>
public class ComplexityMetrics
{
    /// <summary>
    /// Cyclomatic complexity (number of decision points + 1)
    /// </summary>
    public int CyclomaticComplexity { get; set; }

    /// <summary>
    /// Number of basic blocks
    /// </summary>
    public int BlockCount { get; set; }

    /// <summary>
    /// Number of edges
    /// </summary>
    public int EdgeCount { get; set; }

    /// <summary>
    /// Number of decision points (conditional branches)
    /// </summary>
    public int DecisionPoints { get; set; }

    /// <summary>
    /// Number of loops detected
    /// </summary>
    public int LoopCount { get; set; }

    /// <summary>
    /// Maximum depth of nested conditions
    /// </summary>
    public int MaxNestingDepth { get; set; }

    /// <summary>
    /// Whether the method contains exception handling
    /// </summary>
    public bool HasExceptionHandling { get; set; }
}