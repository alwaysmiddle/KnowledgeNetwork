using System.Collections.Generic;

namespace KnowledgeNetwork.Api.Models.Visualization
{
    /// <summary>
    /// Represents a graph layout that can be rendered by the frontend visualization
    /// </summary>
    public record GraphLayout
    {
        /// <summary>
        /// The nodes in the graph
        /// </summary>
        public required IReadOnlyList<GraphNode> Nodes { get; init; }
        
        /// <summary>
        /// The edges connecting nodes
        /// </summary>
        public required IReadOnlyList<GraphEdge> Edges { get; init; }
        
        /// <summary>
        /// Layout configuration hints for the frontend
        /// </summary>
        public required LayoutConfiguration Configuration { get; init; }
        
        /// <summary>
        /// Metadata about the graph
        /// </summary>
        public required GraphMetadata Metadata { get; init; }
    }
    
    /// <summary>
    /// Represents a node in the knowledge graph
    /// </summary>
    public record GraphNode
    {
        /// <summary>
        /// Unique identifier for the node
        /// </summary>
        public required string Id { get; init; }
        
        /// <summary>
        /// Display label for the node
        /// </summary>
        public required string Label { get; init; }
        
        /// <summary>
        /// The type of code element this node represents
        /// </summary>
        public required string NodeType { get; init; }
        
        /// <summary>
        /// The programming language of this node
        /// </summary>
        public required string Language { get; init; }
        
        /// <summary>
        /// Visual properties for rendering
        /// </summary>
        public required NodeVisualProperties Visual { get; init; }
        
        /// <summary>
        /// Additional data specific to the node type
        /// </summary>
        public required Dictionary<string, object> Data { get; init; }
        
        /// <summary>
        /// Position hint for layout algorithms
        /// </summary>
        public NodePosition? Position { get; init; }
        
        /// <summary>
        /// Parent node ID for hierarchical layouts
        /// </summary>
        public string? ParentId { get; init; }
    }
    
    /// <summary>
    /// Represents an edge between nodes
    /// </summary>
    public record GraphEdge
    {
        /// <summary>
        /// Unique identifier for the edge
        /// </summary>
        public required string Id { get; init; }
        
        /// <summary>
        /// Source node ID
        /// </summary>
        public required string Source { get; init; }
        
        /// <summary>
        /// Target node ID
        /// </summary>
        public required string Target { get; init; }
        
        /// <summary>
        /// The type of relationship
        /// </summary>
        public required string EdgeType { get; init; }
        
        /// <summary>
        /// Optional label for the edge
        /// </summary>
        public string? Label { get; init; }
        
        /// <summary>
        /// Visual properties for rendering
        /// </summary>
        public required EdgeVisualProperties Visual { get; init; }
    }
    
    /// <summary>
    /// Visual properties for nodes
    /// </summary>
    public record NodeVisualProperties
    {
        public required string Color { get; init; }
        public required string Shape { get; init; }
        public required string Size { get; init; }
        public string? Icon { get; init; }
        public bool IsExpandable { get; init; }
        public bool IsExpanded { get; init; }
    }
    
    /// <summary>
    /// Visual properties for edges
    /// </summary>
    public record EdgeVisualProperties
    {
        public required string Color { get; init; }
        public required string Style { get; init; } // "solid", "dashed", "dotted"
        public required string ArrowStyle { get; init; } // "none", "arrow", "circle"
        public required float Width { get; init; }
        public bool IsAnimated { get; init; }
    }
    
    /// <summary>
    /// Position information for nodes
    /// </summary>
    public record NodePosition
    {
        public required float X { get; init; }
        public required float Y { get; init; }
    }
    
    /// <summary>
    /// Configuration for layout algorithms
    /// </summary>
    public record LayoutConfiguration
    {
        /// <summary>
        /// The layout algorithm to use
        /// </summary>
        public required string LayoutType { get; init; } // "hierarchical", "force", "circular", "grid"
        
        /// <summary>
        /// Direction for hierarchical layouts
        /// </summary>
        public string? Direction { get; init; } // "TB", "BT", "LR", "RL"
        
        /// <summary>
        /// Spacing between nodes
        /// </summary>
        public required float NodeSpacing { get; init; }
        
        /// <summary>
        /// Spacing between ranks/levels
        /// </summary>
        public required float RankSpacing { get; init; }
        
        /// <summary>
        /// Whether to enable clustering
        /// </summary>
        public bool EnableClustering { get; init; }
    }
    
    /// <summary>
    /// Metadata about the graph
    /// </summary>
    public record GraphMetadata
    {
        /// <summary>
        /// Total number of nodes
        /// </summary>
        public required int NodeCount { get; init; }
        
        /// <summary>
        /// Total number of edges
        /// </summary>
        public required int EdgeCount { get; init; }
        
        /// <summary>
        /// Languages represented in the graph
        /// </summary>
        public required IReadOnlyList<string> Languages { get; init; }
        
        /// <summary>
        /// Timestamp when the graph was generated
        /// </summary>
        public required DateTime GeneratedAt { get; init; }
        
        /// <summary>
        /// Source files included in the analysis
        /// </summary>
        public required IReadOnlyList<string> SourceFiles { get; init; }
    }
}