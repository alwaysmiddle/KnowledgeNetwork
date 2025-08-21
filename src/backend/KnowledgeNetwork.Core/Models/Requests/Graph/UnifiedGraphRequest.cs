using System.Text.Json.Serialization;
using KnowledgeNetwork.Core.Models.Requests.Filters;

namespace KnowledgeNetwork.Core.Models.Requests.Graph;

/// <summary>
/// Request for unified graph operations
/// </summary>
public class UnifiedGraphRequest
{
    [JsonPropertyName("analysisId")]
    public string AnalysisId { get; set; } = string.Empty;

    [JsonPropertyName("context")]
    public string? Context { get; set; }

    [JsonPropertyName("filters")]
    public List<NodeFilter>? Filters { get; set; }

    [JsonPropertyName("depth")]
    public int? Depth { get; set; }
}