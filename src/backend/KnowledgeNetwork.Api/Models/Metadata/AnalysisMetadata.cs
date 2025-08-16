namespace KnowledgeNetwork.Api.Models.Metadata;

/// <summary>
/// Metadata about the analysis operation
/// </summary>
public class AnalysisMetadata
{
    /// <summary>
    /// Timestamp when the analysis was performed
    /// </summary>
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Duration of the analysis operation
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Version of the analysis engine
    /// </summary>
    public string Version { get; set; } = "1.0.0";
}