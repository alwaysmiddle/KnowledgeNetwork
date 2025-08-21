using System.Text.Json.Serialization;

namespace KnowledgeNetwork.Core.Models.Requests.Graph;

/// <summary>
/// Request to expand a specific node
/// </summary>
public class ExpandNodeRequest
{
    [JsonPropertyName("nodeId")]
    public string NodeId { get; set; } = string.Empty;

    [JsonPropertyName("depth")]
    public int Depth { get; set; } = 1;

    [JsonPropertyName("includeEdges")]
    public bool? IncludeEdges { get; set; }
}