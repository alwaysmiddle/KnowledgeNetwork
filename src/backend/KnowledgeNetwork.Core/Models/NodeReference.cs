using System.Text.Json.Serialization;

namespace KnowledgeNetwork.Core.Models;

/// <summary>
/// Reference to another node within the contains relationship
/// </summary>
public class NodeReference
{
    [JsonPropertyName("nodeId")]
    public string NodeId { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("order")]
    public int? Order { get; set; }
}