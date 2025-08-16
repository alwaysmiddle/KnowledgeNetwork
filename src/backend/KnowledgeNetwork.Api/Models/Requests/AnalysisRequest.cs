using System.ComponentModel.DataAnnotations;

namespace KnowledgeNetwork.Api.Models.Requests;

/// <summary>
/// Request model for code analysis
/// </summary>
public class AnalysisRequest
{
    /// <summary>
    /// Source code to analyze
    /// </summary>
    [Required]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Optional language identifier (defaults to auto-detection)
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Optional filename for context
    /// </summary>
    public string? FileName { get; set; }
}