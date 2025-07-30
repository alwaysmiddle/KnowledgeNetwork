namespace KnowledgeNetwork.Api.Models;

/// <summary>
/// Utility class for mapping between legacy Node models and new unified GraphNode models
/// This enables backward compatibility during the migration process
/// </summary>
public static class ModelMappers
{
    /// <summary>
    /// Maps a legacy Node to a new GraphNode
    /// </summary>
    /// <param name="legacyNode">The legacy node to convert</param>
    /// <returns>A new GraphNode with mapped data</returns>
    public static GraphNode MapLegacyToGraphNode(Node legacyNode)
    {
        return new GraphNode
        {
            Id = legacyNode.Id.ToString(), // Convert int to string
            Label = legacyNode.Title, // Title → Label
            Content = legacyNode.Content, // Direct mapping
            Position = new Position2D
            {
                X = legacyNode.XPosition,
                Y = legacyNode.YPosition
            },
            Types = new HashSet<string> { legacyNode.NodeType }, // String → HashSet
            Properties = new Dictionary<string, object>(),
            CreatedAt = legacyNode.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Maps a GraphNode back to a legacy Node (for backward compatibility)
    /// Warning: This conversion may lose data (e.g., multiple types, properties)
    /// </summary>
    /// <param name="graphNode">The GraphNode to convert</param>
    /// <returns>A legacy Node with mapped data</returns>
    public static Node MapGraphNodeToLegacy(GraphNode graphNode)
    {
        // Note: This is risky conversion from string to int
        // Should only be used during migration period
        if (!int.TryParse(graphNode.Id, out int legacyId))
        {
            throw new InvalidOperationException($"Cannot convert GraphNode ID '{graphNode.Id}' to integer for legacy Node");
        }

        return new Node
        {
            Id = legacyId,
            Title = graphNode.Label, // Label → Title
            Content = graphNode.Content, // Direct mapping
            NodeType = graphNode.Types.FirstOrDefault() ?? "concept", // HashSet → String (loses data!)
            XPosition = graphNode.Position.X,
            YPosition = graphNode.Position.Y,
            CreatedAt = graphNode.CreatedAt
        };
    }

    /// <summary>
    /// Maps a CreateNodeRequest to a new GraphNode
    /// </summary>
    /// <param name="request">The creation request</param>
    /// <param name="generateId">Whether to generate a new UUID (default: true)</param>
    /// <returns>A new GraphNode with mapped data</returns>
    public static GraphNode MapCreateRequestToGraphNode(CreateNodeRequest request, bool generateId = true)
    {
        return new GraphNode
        {
            Id = generateId ? Guid.NewGuid().ToString() : string.Empty,
            Label = request.Title,
            Content = request.Content,
            Position = new Position2D
            {
                X = request.XPosition,
                Y = request.YPosition
            },
            Types = new HashSet<string> { request.NodeType },
            Properties = new Dictionary<string, object>(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Maps a GraphNode back to a legacy Node response format
    /// This ensures the API response maintains the expected legacy structure
    /// </summary>
    /// <param name="graphNode">The GraphNode to convert</param>
    /// <param name="legacyId">Optional: provide a legacy integer ID</param>
    /// <returns>A legacy Node formatted for API response</returns>
    public static Node MapGraphNodeToLegacyResponse(GraphNode graphNode, int? legacyId = null)
    {
        return new Node
        {
            Id = legacyId ?? (int.TryParse(graphNode.Id, out int id) ? id : 0),
            Title = graphNode.Label,
            Content = graphNode.Content,
            NodeType = graphNode.Types.FirstOrDefault() ?? "concept",
            XPosition = graphNode.Position.X,
            YPosition = graphNode.Position.Y,
            CreatedAt = graphNode.CreatedAt
        };
    }
}