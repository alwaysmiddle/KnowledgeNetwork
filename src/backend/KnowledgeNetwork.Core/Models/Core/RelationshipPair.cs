using System.Text.Json.Serialization;

namespace KnowledgeNetwork.Core.Models.Core;

/// <summary>
/// Bidirectional relationship between nodes
/// </summary>
public class RelationshipPair
{
    [JsonPropertyName("type")]
    public RelationshipType Type { get; set; } = new();

    [JsonPropertyName("direction")]
    public string Direction { get; set; } = "outgoing";

    [JsonPropertyName("targetNodeId")]
    public string TargetNodeId { get; set; } = string.Empty;

    [JsonPropertyName("metadata")]
    public Dictionary<string, object?>? Metadata { get; set; }
}

/// <summary>
/// Bidirectional relationship type definition
/// </summary>
public class RelationshipType
{
    [JsonPropertyName("forward")]
    public string Forward { get; set; } = string.Empty;

    [JsonPropertyName("reverse")]
    public string Reverse { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;
}