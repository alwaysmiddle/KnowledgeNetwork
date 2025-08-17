using System.Text.Json.Serialization;

namespace KnowledgeNetwork.Core.Models.Requests;

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

/// <summary>
/// Request to expand a specific node
/// </summary>
public class ExpandNodeRequest
{
    [JsonPropertyName("nodeId")]
    public string NodeId { get; set; } = string.Empty;

    [JsonPropertyName("depth")]
    public int Depth { get; set; } = 1;

    [JsonPropertyName("includeRelationships")]
    public bool? IncludeRelationships { get; set; }
}

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