namespace KnowledgeNetwork.Api.Models;

/// <summary>
/// Legacy Node model - maintained for backward compatibility during migration
/// This model will be removed once migration to GraphNode is complete
/// </summary>
public class Node
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string NodeType { get; set; } = "concept";
    public double XPosition { get; set; }
    public double YPosition { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Legacy CreateNodeRequest - maintained for backward compatibility during migration
/// </summary>
public class CreateNodeRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string NodeType { get; set; } = "concept";
    public double XPosition { get; set; }
    public double YPosition { get; set; }
}