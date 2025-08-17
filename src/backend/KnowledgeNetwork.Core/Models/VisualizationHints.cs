using System.Text.Json.Serialization;

namespace KnowledgeNetwork.Core.Models;

/// <summary>
/// Hints for rendering the node in visualizations
/// </summary>
public class VisualizationHints
{
    [JsonPropertyName("preferredLayout")]
    public string PreferredLayout { get; set; } = "force-directed";

    [JsonPropertyName("collapsed")]
    public bool Collapsed { get; set; }

    [JsonPropertyName("color")]
    public string? Color { get; set; }

    [JsonPropertyName("icon")]
    public string? Icon { get; set; }

    [JsonPropertyName("position")]
    public Position? Position { get; set; }

    [JsonPropertyName("size")]
    public Size? Size { get; set; }
}

/// <summary>
/// Position coordinates for visualization
/// </summary>
public class Position
{
    [JsonPropertyName("x")]
    public double X { get; set; }

    [JsonPropertyName("y")]
    public double Y { get; set; }

    [JsonPropertyName("z")]
    public double? Z { get; set; }
}

/// <summary>
/// Size dimensions for visualization
/// </summary>
public class Size
{
    [JsonPropertyName("width")]
    public double Width { get; set; }

    [JsonPropertyName("height")]
    public double Height { get; set; }
}