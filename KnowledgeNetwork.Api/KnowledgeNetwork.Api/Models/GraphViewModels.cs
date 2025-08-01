// using System.Text.Json;
//
// namespace KnowledgeNetwork.Api.Models;
//
// /// <summary>
// /// Request model for the composite graph view endpoint
// /// Supports different visualization contexts and selective loading
// /// </summary>
// public class GraphViewRequest
// {
//     public string Context { get; set; } = "2d"; // "2d", "3d", "presentation"
//     public string[]? NodeIds { get; set; } // Specific nodes (null = all visible)
//     public int? TraversalDepth { get; set; } // Depth from specified nodes
//     public string[] IncludeFields { get; set; } = new[] { "layout", "content" }; // What to include
//     public long? ClientVersion { get; set; } // For incremental updates
//     public GraphFilter? Filter { get; set; } // Additional filtering
// }
//
// /// <summary>
// /// Composite response that includes everything needed for visualization
// /// Single payload to minimize API calls and latency
// /// </summary>
// public class GraphViewResponse
// {
//     public GraphNode[] Nodes { get; set; } = Array.Empty<GraphNode>();
//     public GraphEdge[] Edges { get; set; } = Array.Empty<GraphEdge>();
//     public LayoutData? Layout2D { get; set; } // Precomputed 2D flowchart layout
//     public LayoutData? Layout3D { get; set; } // Precomputed 3D spatial layout
//     public long Version { get; set; } // Current graph version
//     public Dictionary<string, object> Metadata { get; set; } = new();
//     public GraphViewStats Stats { get; set; } = new();
// }
//
// /// <summary>
// /// Layout data structure for both 2D and 3D representations
// /// Stores precomputed positions and rendering hints
// /// </summary>
// public class LayoutData
// {
//     public required string Algorithm { get; set; } // "hierarchical_dag", "force_directed_3d"
//     public required string Context { get; set; } // "2d_flowchart", "3d_spatial"
//     public Dictionary<string, NodePosition> Positions { get; set; } = new();
//     public Dictionary<string, object> RenderingHints { get; set; } = new();
//     public LayoutMetadata Metadata { get; set; } = new();
//     public DateTime ComputedAt { get; set; } = DateTime.UtcNow;
// }
//
// /// <summary>
// /// Position data that supports both 2D and 3D coordinates
// /// Includes additional rendering context for each node
// /// </summary>
// public class NodePosition
// {
//     public double X { get; set; }
//     public double Y { get; set; }
//     public double? Z { get; set; } // Optional for 3D layouts
//     public int? Layer { get; set; } // For hierarchical layouts
//     public int? Order { get; set; } // Order within layer
//     public string? Cluster { get; set; } // Semantic clustering
//     public Dictionary<string, object> Properties { get; set; } = new();
// }
//
// /// <summary>
// /// Layout computation metadata
// /// Provides insights into the layout algorithm performance
// /// </summary>
// public class LayoutMetadata
// {
//     public int NodeCount { get; set; }
//     public int EdgeCount { get; set; }
//     public TimeSpan ComputationTime { get; set; }
//     public Dictionary<string, object> AlgorithmMetrics { get; set; } = new();
//     public string[] Warnings { get; set; } = Array.Empty<string>();
// }
//
// /// <summary>
// /// Filtering options for graph queries
// /// Supports type-based and property-based filtering
// /// </summary>
// public class GraphFilter
// {
//     public string[]? NodeTypes { get; set; } // Filter by node types
//     public string[]? EdgeTypes { get; set; } // Filter by edge types
//     public Dictionary<string, object>? Properties { get; set; } // Property filters
//     public DateRange? DateRange { get; set; } // Time-based filtering
//     public string? SearchQuery { get; set; } // Full-text search
// }
//
// /// <summary>
// /// Date range for temporal filtering
// /// </summary>
// public class DateRange
// {
//     public DateTime? From { get; set; }
//     public DateTime? To { get; set; }
// }
//
// /// <summary>
// /// Statistics about the returned graph view
// /// Useful for performance monitoring and client optimization
// /// </summary>
// public class GraphViewStats
// {
//     public int NodesReturned { get; set; }
//     public int EdgesReturned { get; set; }
//     public int TotalNodesInGraph { get; set; }
//     public int TotalEdgesInGraph { get; set; }
//     public TimeSpan QueryTime { get; set; }
//     public bool LayoutFromCache { get; set; }
//     public string[] AppliedFilters { get; set; } = Array.Empty<string>();
// }