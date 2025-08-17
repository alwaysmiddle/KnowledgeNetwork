using System.Text.Json.Serialization;
using KnowledgeNetwork.Core.Models.Requests.Filters;

namespace KnowledgeNetwork.Core.Models.Requests.Graph;

/// <summary>
/// Request to create a view node
/// </summary>
public class ViewCreationRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("nodeIds")]
    public List<string>? NodeIds { get; set; }

    [JsonPropertyName("filter")]
    public NodeFilter? Filter { get; set; }

    [JsonPropertyName("aggregationType")]
    public string? AggregationType { get; set; }
}