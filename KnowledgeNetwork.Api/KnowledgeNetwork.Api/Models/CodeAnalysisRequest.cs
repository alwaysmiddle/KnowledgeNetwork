using System.ComponentModel.DataAnnotations;

namespace KnowledgeNetwork.Api.Models;

/// <summary>
/// Request model for code analysis API
/// </summary>
public class CodeAnalysisRequest
{
    /// <summary>
    /// The C# code to analyze
    /// </summary>
    [Required]
    [StringLength(1000000, ErrorMessage = "Code cannot exceed 1,000,000 characters")]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Optional filename for the code (helps with analysis context)
    /// </summary>
    [StringLength(255, ErrorMessage = "Filename cannot exceed 255 characters")]
    public string? FileName { get; set; }

    /// <summary>
    /// Optional namespace context for analysis
    /// </summary>
    [StringLength(500, ErrorMessage = "Namespace cannot exceed 500 characters")]
    public string? NamespaceContext { get; set; }

    /// <summary>
    /// Include detailed analysis (default: true)
    /// </summary>
    public bool IncludeDetails { get; set; } = true;

    /// <summary>
    /// Include syntax tree information (default: false)
    /// </summary>
    public bool IncludeSyntaxTree { get; set; } = false;
}