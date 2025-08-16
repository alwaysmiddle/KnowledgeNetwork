namespace KnowledgeNetwork.Domains.Code.Models.Analysis;

/// <summary>
/// Information about a class declaration
/// </summary>
public class ClassInfo
{
    /// <summary>
    /// Name of the class
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Namespace containing the class
    /// </summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// Access modifiers and other modifiers
    /// </summary>
    public string Modifiers { get; set; } = string.Empty;

    /// <summary>
    /// Line number where the class is declared
    /// </summary>
    public int LineNumber { get; set; }
}