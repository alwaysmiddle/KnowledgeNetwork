using System.Text.Json.Serialization;

namespace KnowledgeNetwork.Core.Models.Responses;

/// <summary>
/// Response containing unified graph data
/// </summary>
public class UnifiedGraphResponse
{
    [JsonPropertyName("nodes")]
    public List<KnowledgeNode> Nodes { get; set; } = new();

    [JsonPropertyName("metadata")]
    public GraphMetadata Metadata { get; set; } = new();
}

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

/// <summary>
/// Response for expanded node requests
/// </summary>
public class ExpandedNode
{
    [JsonPropertyName("node")]
    public KnowledgeNode Node { get; set; } = new();

    [JsonPropertyName("children")]
    public List<KnowledgeNode> Children { get; set; } = new();

    [JsonPropertyName("relationships")]
    public List<RelationshipPair>? Relationships { get; set; }
}