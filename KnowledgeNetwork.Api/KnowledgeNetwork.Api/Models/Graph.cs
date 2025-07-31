namespace KnowledgeNetwork.Api.Models
{
    public class GraphNode
    {
        public required string Id { get; set; }
        public required string Label { get; set; }
        public string? Content { get; set; }
        public required Position2D Position { get; set; }
        public HashSet<string> Types { get; set; } = new();
        public Dictionary<string, object> Properties { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public long Version { get; set; } = 1;
    }
    public class GraphEdge
    {
        public required string Id { get; set; }
        public required string SourceNodeId { get; set; }
        public required string TargetNodeId { get; set; }
        public HashSet<string> Types { get; set; } = new();
        public string? Label { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Graph
    {
        public required string Id { get; set; }
        public required string GraphType { get; set; }
        public string? Name { get; set; }
        public List<GraphNode> Nodes { get; set; } = new();
        public List<GraphEdge> Edges { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}