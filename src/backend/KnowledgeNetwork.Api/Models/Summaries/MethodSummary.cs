namespace KnowledgeNetwork.Api.Models.Summaries;

/// <summary>
/// Summary information about a method
/// </summary>
public class MethodSummary
{
    /// <summary>
    /// Name of the method
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Return type of the method
    /// </summary>
    public string ReturnType { get; set; } = string.Empty;

    /// <summary>
    /// Access modifiers (public, private, static, etc.)
    /// </summary>
    public string Modifiers { get; set; } = string.Empty;

    /// <summary>
    /// Method parameters with types
    /// </summary>
    public List<string> Parameters { get; set; } = new();

    /// <summary>
    /// Line number where the method is declared
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// Name of the class containing this method
    /// </summary>
    public string ClassName { get; set; } = string.Empty;
}