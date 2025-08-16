using KnowledgeNetwork.Api.Models.Summaries;
using KnowledgeNetwork.Api.Models.Metadata;

namespace KnowledgeNetwork.Api.Models.Responses;

/// <summary>
/// Response model for code analysis
/// </summary>
public class AnalysisResponse
{
    /// <summary>
    /// Whether the analysis was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Language that was analyzed
    /// </summary>
    public string LanguageId { get; set; } = string.Empty;

    /// <summary>
    /// Any errors that occurred during analysis
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Extracted classes from the code
    /// </summary>
    public List<ClassSummary> Classes { get; set; } = new();

    /// <summary>
    /// Extracted methods from the code
    /// </summary>
    public List<MethodSummary> Methods { get; set; } = new();

    /// <summary>
    /// Extracted properties from the code
    /// </summary>
    public List<PropertySummary> Properties { get; set; } = new();

    /// <summary>
    /// Using statements/imports found in the code
    /// </summary>
    public List<string> UsingStatements { get; set; } = new();

    /// <summary>
    /// Analysis metadata
    /// </summary>
    public AnalysisMetadata Metadata { get; set; } = new();
}