using System.Text.Json.Serialization;
using KnowledgeNetwork.Core.Models.Core;
using KnowledgeNetwork.Core.Models.Responses.Metadata;

namespace KnowledgeNetwork.Core.Models.Responses.Graph;

/// <summary>
/// Response containing unified graph data
/// </summary>
public class UnifiedGraphResponse
{
    [JsonPropertyName("nodes")]
    public List<KnowledgeNode> Nodes { get; set; } = new();

    [JsonPropertyName("edges")]
    public List<KnowledgeEdge> Edges { get; set; } = new();

    [JsonPropertyName("metadata")]
    public GraphMetadata Metadata { get; set; } = new();
}