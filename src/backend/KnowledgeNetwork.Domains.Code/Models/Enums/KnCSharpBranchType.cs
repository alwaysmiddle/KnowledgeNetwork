namespace KnowledgeNetwork.Domains.Code.Models.Enums;

/// <summary>
/// Types of control flow branches
/// </summary>
public enum KnCSharpBranchType
{
    /// <summary>
    /// Simple if statement
    /// </summary>
    Conditional,

    /// <summary>
    /// Loop condition (while, for)
    /// </summary>
    Loop,

    /// <summary>
    /// Switch statement
    /// </summary>
    Switch,

    /// <summary>
    /// Exception handling
    /// </summary>
    Exception,

    /// <summary>
    /// Unconditional jump
    /// </summary>
    Unconditional
}