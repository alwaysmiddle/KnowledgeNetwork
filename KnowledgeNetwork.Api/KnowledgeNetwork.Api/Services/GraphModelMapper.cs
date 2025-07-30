using KnowledgeNetwork.Api.Models;

namespace KnowledgeNetwork.Api.Services
{
    public static class GraphModelMapper
    {
        /// <summary>
        /// Maps a legacy Node to the new unified GraphNode model
        /// </summary>
        public static GraphNode MapLegacyToGraphNode(Node legacyNode)
        {
            return new GraphNode
            {
                Id = legacyNode.Id.ToString(),
                Label = legacyNode.Title,
                Content = legacyNode.Content,
                Position = new Position2D 
                { 
                    X = legacyNode.XPosition, 
                    Y = legacyNode.YPosition 
                },
                Types = new HashSet<string> { legacyNode.NodeType },
                Properties = new Dictionary<string, object>(),
                CreatedAt = legacyNode.CreatedAt,
                UpdatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Maps a unified GraphNode back to legacy Node model (for backward compatibility)
        /// WARNING: This conversion may lose data due to model differences
        /// </summary>
        public static Node MapGraphNodeToLegacy(GraphNode graphNode)
        {
            // Attempt to parse string ID back to int - this is risky!
            if (!int.TryParse(graphNode.Id, out int legacyId))
            {
                throw new InvalidOperationException($"Cannot convert GraphNode ID '{graphNode.Id}' to legacy integer ID");
            }

            return new Node
            {
                Id = legacyId,
                Title = graphNode.Label,
                Content = graphNode.Content,
                NodeType = graphNode.Types.FirstOrDefault() ?? "concept",
                XPosition = graphNode.Position.X,
                YPosition = graphNode.Position.Y,
                CreatedAt = graphNode.CreatedAt
            };
        }

        /// <summary>
        /// Maps a CreateNodeRequest to the new unified GraphNode model
        /// </summary>
        public static GraphNode MapCreateRequestToGraphNode(CreateNodeRequest request)
        {
            return new GraphNode
            {
                Id = Guid.NewGuid().ToString(), // Generate new UUID
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
        /// Creates a simple Graph container with a single node (for basic compatibility)
        /// </summary>
        public static Graph CreateSingleNodeGraph(GraphNode node, string? graphName = null)
        {
            return new Graph
            {
                Id = Guid.NewGuid().ToString(),
                GraphType = "knowledge-node",
                Name = graphName ?? $"Graph for {node.Label}",
                Nodes = new List<GraphNode> { node },
                Edges = new List<GraphEdge>(),
                Metadata = new Dictionary<string, object>
                {
                    ["migrated_from_legacy"] = true,
                    ["original_node_id"] = node.Id
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Maps a collection of legacy nodes to a unified Graph
        /// </summary>
        public static Graph MapLegacyNodesToGraph(IEnumerable<Node> legacyNodes, string? graphName = null)
        {
            var graphNodes = legacyNodes.Select(MapLegacyToGraphNode).ToList();
            
            return new Graph
            {
                Id = Guid.NewGuid().ToString(),
                GraphType = "knowledge-graph",
                Name = graphName ?? "Migrated Knowledge Graph",
                Nodes = graphNodes,
                Edges = new List<GraphEdge>(), // Legacy system doesn't have explicit edges
                Metadata = new Dictionary<string, object>
                {
                    ["migrated_from_legacy"] = true,
                    ["migration_date"] = DateTime.UtcNow,
                    ["total_nodes"] = graphNodes.Count
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}