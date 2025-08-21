namespace KnowledgeNetwork.Api.Models.Summaries;

/// <summary>
/// Summary information about a class
/// </summary>
public class ClassSummary
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
    /// Access modifiers (public, private, etc.)
    /// </summary>
    public string Modifiers { get; set; } = string.Empty;

    /// <summary>
    /// Line number where the class is declared
    /// </summary>
    public int LineNumber { get; set; }
}