namespace KnowledgeNetwork.Domains.Code.Models.Common;

/// <summary>
/// Source location information
/// </summary>
public class LocationInfo
{
    /// <summary>
    /// Starting line number (1-based)
    /// </summary>
    public int StartLine { get; set; }

    /// <summary>
    /// Ending line number (1-based)
    /// </summary>
    public int EndLine { get; set; }

    /// <summary>
    /// Starting column (0-based)
    /// </summary>
    public int StartColumn { get; set; }

    /// <summary>
    /// Ending column (0-based)
    /// </summary>
    public int EndColumn { get; set; }

    /// <summary>
    /// File path (if available)
    /// </summary>
    public string? FilePath { get; set; }
}