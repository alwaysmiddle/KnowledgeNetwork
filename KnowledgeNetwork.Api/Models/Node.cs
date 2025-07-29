namespace KnowledgeNetwork.Api.Models;

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

public class CreateNodeRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string NodeType { get; set; } = "concept";
    public double XPosition { get; set; }
    public double YPosition { get; set; }
}