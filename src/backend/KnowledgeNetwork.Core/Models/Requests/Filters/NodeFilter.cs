using System.Text.Json.Serialization;

namespace KnowledgeNetwork.Core.Models.Requests.Filters;

/// <summary>
/// Filter for nodes in graph queries
/// </summary>
public class NodeFilter
{
    [JsonPropertyName("field")]
    public string Field { get; set; } = string.Empty;

    [JsonPropertyName("operator")]
    public string Operator { get; set; } = "equals";

    [JsonPropertyName("value")]
    public object? Value { get; set; }
}