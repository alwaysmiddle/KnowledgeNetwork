using System.Text.Json.Serialization;

namespace KnowledgeNetwork.Core.Models.Requests.Analysis;

/// <summary>
/// Request for CFG analysis
/// </summary>
public class CfgAnalysisRequest
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("depth")]
    public int? Depth { get; set; }

    [JsonPropertyName("includeOperations")]
    public bool? IncludeOperations { get; set; } = true;

    [JsonPropertyName("language")]
    public string? Language { get; set; } = "csharp";
}