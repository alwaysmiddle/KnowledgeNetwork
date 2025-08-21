using System.Text.Json.Serialization;

namespace KnowledgeNetwork.Core.Models.Core;

/// <summary>
/// Quantitative measurements for edges
/// </summary>
public class EdgeMetrics
{
    /// <summary>
    /// How often this edge occurs (e.g., method call frequency)
    /// </summary>
    [JsonPropertyName("frequency")]
    public int? Frequency { get; set; }

    /// <summary>
    /// Weight/importance of this edge (0.0 to 1.0)
    /// </summary>
    [JsonPropertyName("weight")]
    public double? Weight { get; set; }

    /// <summary>
    /// Performance impact in milliseconds
    /// </summary>
    [JsonPropertyName("performance")]
    public double? Performance { get; set; }

    /// <summary>
    /// Quality score of this relationship (0.0 to 1.0)
    /// </summary>
    [JsonPropertyName("quality")]
    public double? Quality { get; set; }

    /// <summary>
    /// Additional custom metrics
    /// </summary>
    [JsonPropertyName("customMetrics")]
    public Dictionary<string, object?>? CustomMetrics { get; set; }
}