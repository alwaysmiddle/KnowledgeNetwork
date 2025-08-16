using KnowledgeNetwork.Domains.Code.Models.Enums;

namespace KnowledgeNetwork.Domains.Code.Models.ControlFlow;

/// <summary>
/// Information about conditional branches
/// </summary>
public class KnCSharpBranchInfo
{
    /// <summary>
    /// Condition expression for the branch
    /// </summary>
    public string Condition { get; set; } = string.Empty;

    /// <summary>
    /// Block to execute if condition is true
    /// </summary>
    public int? TrueTarget { get; set; }

    /// <summary>
    /// Block to execute if condition is false
    /// </summary>
    public int? FalseTarget { get; set; }

    /// <summary>
    /// Type of branch (if, while, for, switch, etc.)
    /// </summary>
    public KnCSharpBranchType BranchType { get; set; }
}