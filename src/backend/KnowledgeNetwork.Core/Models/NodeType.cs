using System.Text.Json.Serialization;
using KnowledgeNetwork.Core.Models.Constants;

namespace KnowledgeNetwork.Core.Models;

/// <summary>
/// Hierarchical type system supporting universal concepts, language-specific details, and custom classifications
/// </summary>
public class NodeType
{
    [JsonPropertyName("primary")]
    public string Primary { get; set; } = PrimaryNodeType.Unknown;

    [JsonPropertyName("secondary")]
    public string? Secondary { get; set; }

    [JsonPropertyName("custom")]
    public string? Custom { get; set; }
}