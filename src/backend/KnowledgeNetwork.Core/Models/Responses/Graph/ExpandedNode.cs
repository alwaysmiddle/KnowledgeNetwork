using System.Text.Json.Serialization;
using KnowledgeNetwork.Core.Models.Core;

namespace KnowledgeNetwork.Core.Models.Responses.Graph;

/// <summary>
/// Response for expanded node requests
/// </summary>
public class ExpandedNode
{
    [JsonPropertyName("node")]
    public KnowledgeNode Node { get; set; } = new();

    [JsonPropertyName("children")]
    public List<KnowledgeNode> Children { get; set; } = new();

    [JsonPropertyName("edges")]
    public List<KnowledgeEdge> Edges { get; set; } = new();
}