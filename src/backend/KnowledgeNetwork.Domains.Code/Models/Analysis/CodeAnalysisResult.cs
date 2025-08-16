using Microsoft.CodeAnalysis;

namespace KnowledgeNetwork.Domains.Code.Models.Analysis;

/// <summary>
/// Result of C# code analysis
/// </summary>
public class KnCodeAnalysisResult
{
    /// <summary>
    /// Whether the analysis was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Language identifier for the analysis
    /// </summary>
    public string LanguageId { get; set; } = string.Empty;

    /// <summary>
    /// List of errors encountered during analysis
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// The parsed syntax tree (Roslyn object)
    /// </summary>
    public SyntaxTree? SyntaxTree { get; set; }

    /// <summary>
    /// Extracted class information
    /// </summary>
    public List<KnClassInfo> Classes { get; set; } = new();

    /// <summary>
    /// Extracted method information
    /// </summary>
    public List<KnMethodInfo> Methods { get; set; } = new();

    /// <summary>
    /// Extracted property information
    /// </summary>
    public List<KnPropertyInfo> Properties { get; set; } = new();

    /// <summary>
    /// Using statements found in the code
    /// </summary>
    public List<string> UsingStatements { get; set; } = new();
}