namespace KnowledgeNetwork.Api.Models;

/// <summary>
/// DTO for creating a new GraphNode
/// </summary>
public class CreateGraphNodeRequest
{
    public required string Label { get; set; }
    public string? Content { get; set; }
    public required Position2D Position { get; set; }
    public HashSet<string> Types { get; set; } = new() { "concept" };
    public Dictionary<string, object> Properties { get; set; } = new();
}

/// <summary>
/// DTO for updating an existing GraphNode
/// </summary>
public class UpdateGraphNodeRequest
{
    public string? Label { get; set; }
    public string? Content { get; set; }
    public Position2D? Position { get; set; }
    public HashSet<string>? Types { get; set; }
    public Dictionary<string, object>? Properties { get; set; }
}

/// <summary>
/// DTO for GraphNode responses (includes computed fields)
/// </summary>
public class GraphNodeResponse
{
    public required string Id { get; set; }
    public required string Label { get; set; }
    public string? Content { get; set; }
    public required Position2D Position { get; set; }
    public HashSet<string> Types { get; set; } = new();
    public Dictionary<string, object> Properties { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Maps a GraphNode to a response DTO
    /// </summary>
    public static GraphNodeResponse FromGraphNode(GraphNode node)
    {
        return new GraphNodeResponse
        {
            Id = node.Id,
            Label = node.Label,
            Content = node.Content,
            Position = node.Position,
            Types = node.Types,
            Properties = node.Properties,
            CreatedAt = node.CreatedAt,
            UpdatedAt = node.UpdatedAt
        };
    }
}