using System.Text.Json.Serialization;

namespace KnowledgeNetwork.Core.Models.Core;

/// <summary>
/// Core node that represents everything in the knowledge graph
/// </summary>
public class KnowledgeNode
{
    // Identity
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public NodeType Type { get; set; } = new();

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    // Recursive structure - nodes contain references to other nodes
    [JsonPropertyName("contains")]
    public List<NodeReference> Contains { get; set; } = new();

    // Edge references (first-class edges)
    [JsonPropertyName("incomingEdgeIds")]
    public List<string> IncomingEdgeIds { get; set; } = new();

    [JsonPropertyName("outgoingEdgeIds")]
    public List<string> OutgoingEdgeIds { get; set; } = new();

    // Flexible metadata
    [JsonPropertyName("properties")]
    public Dictionary<string, object?> Properties { get; set; } = new();

    [JsonPropertyName("metrics")]
    public NodeMetrics Metrics { get; set; } = new();

    // Visualization hints
    [JsonPropertyName("visualization")]
    public VisualizationHints Visualization { get; set; } = new();

    // Node characteristics
    [JsonPropertyName("isView")]
    public bool IsView { get; set; }

    [JsonPropertyName("isPersisted")]
    public bool IsPersisted { get; set; }

    [JsonPropertyName("sourceLanguage")]
    public string? SourceLanguage { get; set; }
}