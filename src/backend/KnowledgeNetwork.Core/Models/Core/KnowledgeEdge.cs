using System.Text.Json.Serialization;

namespace KnowledgeNetwork.Core.Models.Core;

/// <summary>
/// First-class edge entity representing relationships between nodes
/// </summary>
public class KnowledgeEdge
{
    /// <summary>
    /// Unique identifier for this edge
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// ID of the source node
    /// </summary>
    [JsonPropertyName("sourceNodeId")]
    public string SourceNodeId { get; set; } = string.Empty;

    /// <summary>
    /// ID of the target node
    /// </summary>
    [JsonPropertyName("targetNodeId")]
    public string TargetNodeId { get; set; } = string.Empty;

    /// <summary>
    /// Type of relationship (calls, inherits, contains, etc.)
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Category of relationship (structure, execution, control-flow, etc.)
    /// </summary>
    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Additional properties specific to this edge
    /// </summary>
    [JsonPropertyName("properties")]
    public Dictionary<string, object?> Properties { get; set; } = new();

    /// <summary>
    /// Quantitative measurements for this edge
    /// </summary>
    [JsonPropertyName("metrics")]
    public EdgeMetrics Metrics { get; set; } = new();

    /// <summary>
    /// Strength of the relationship (0.0 to 1.0)
    /// </summary>
    [JsonPropertyName("strength")]
    public double Strength { get; set; } = 0.5;

    /// <summary>
    /// Confidence in the relationship (0.0 to 1.0)
    /// </summary>
    [JsonPropertyName("confidence")]
    public double Confidence { get; set; } = 1.0;

    /// <summary>
    /// Time-based metadata for this edge
    /// </summary>
    [JsonPropertyName("temporal")]
    public TemporalData Temporal { get; set; } = new();

    /// <summary>
    /// Visualization hints for rendering this edge
    /// </summary>
    [JsonPropertyName("visualizationHints")]
    public EdgeVisualizationHints VisualizationHints { get; set; } = new();
}