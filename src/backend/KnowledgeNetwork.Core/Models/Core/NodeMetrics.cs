using System.Text.Json.Serialization;

namespace KnowledgeNetwork.Core.Models.Core;

/// <summary>
/// Quantitative measurements for nodes
/// </summary>
public class NodeMetrics
{
    [JsonPropertyName("linesOfCode")]
    public int? LinesOfCode { get; set; }

    [JsonPropertyName("complexity")]
    public int? Complexity { get; set; }

    [JsonPropertyName("nodeCount")]
    public int? NodeCount { get; set; }

    [JsonPropertyName("edgeCount")]
    public int? EdgeCount { get; set; }

    [JsonPropertyName("depth")]
    public int? Depth { get; set; }

    [JsonPropertyName("customMetrics")]
    public Dictionary<string, object?>? CustomMetrics { get; set; }
}