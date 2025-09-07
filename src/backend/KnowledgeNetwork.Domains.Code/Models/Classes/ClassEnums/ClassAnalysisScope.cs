namespace KnowledgeNetwork.Domains.Code.Models.Classes.ClassEnums;

/// <summary>
/// Scope types for class analysis
/// </summary>
public enum ClassAnalysisScope
{
    /// <summary>
    /// Single file analysis
    /// </summary>
    File,

    /// <summary>
    /// Namespace analysis
    /// </summary>
    Namespace,

    /// <summary>
    /// Project-wide analysis
    /// </summary>
    Project,

    /// <summary>
    /// Assembly analysis
    /// </summary>
    Assembly
}