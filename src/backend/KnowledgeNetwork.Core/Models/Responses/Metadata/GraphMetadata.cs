using System.Text.Json.Serialization;

namespace KnowledgeNetwork.Core.Models.Responses.Metadata;

/// <summary>
/// Metadata about the graph response
/// </summary>
public class GraphMetadata
{
    [JsonPropertyName("totalNodes")]
    public int TotalNodes { get; set; }

    [JsonPropertyName("languages")]
    public List<string> Languages { get; set; } = new();

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("O");

    [JsonPropertyName("viewCount")]
    public int ViewCount { get; set; }

    [JsonPropertyName("duration")]
    public TimeSpan? Duration { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0.0";
}