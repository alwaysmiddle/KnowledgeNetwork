using System.Text.Json.Serialization;

namespace KnowledgeNetwork.Core.Models.Core;

/// <summary>
/// Hints for rendering edges in visualizations
/// </summary>
public class EdgeVisualizationHints
{
    /// <summary>
    /// Line style for the edge
    /// </summary>
    [JsonPropertyName("lineStyle")]
    public string LineStyle { get; set; } = "solid"; // solid, dashed, dotted

    /// <summary>
    /// Color of the edge
    /// </summary>
    [JsonPropertyName("color")]
    public string? Color { get; set; }

    /// <summary>
    /// Thickness of the edge line
    /// </summary>
    [JsonPropertyName("thickness")]
    public double Thickness { get; set; } = 1.0;

    /// <summary>
    /// Arrow style at the target end
    /// </summary>
    [JsonPropertyName("arrowStyle")]
    public string ArrowStyle { get; set; } = "triangle"; // triangle, circle, diamond, none

    /// <summary>
    /// Whether the edge should be curved
    /// </summary>
    [JsonPropertyName("curved")]
    public bool Curved { get; set; } = false;

    /// <summary>
    /// Label to display on the edge
    /// </summary>
    [JsonPropertyName("label")]
    public string? Label { get; set; }

    /// <summary>
    /// Position of the label on the edge
    /// </summary>
    [JsonPropertyName("labelPosition")]
    public string LabelPosition { get; set; } = "center"; // center, start, end, hidden

    /// <summary>
    /// Animation style for the edge
    /// </summary>
    [JsonPropertyName("animation")]
    public string? Animation { get; set; } // flow, pulse, none
}