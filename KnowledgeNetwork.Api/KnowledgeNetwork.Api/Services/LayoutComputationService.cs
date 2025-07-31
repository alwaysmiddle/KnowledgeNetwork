using System.Diagnostics;
using System.Text.Json;
using KnowledgeNetwork.Api.Models;
using Npgsql;

namespace KnowledgeNetwork.Api.Services;

/// <summary>
/// Service for computing and caching graph layouts
/// Supports both 2D hierarchical flowcharts and 3D spatial layouts
/// </summary>
public class LayoutComputationService
{
    private readonly string _connectionString;
    private readonly ILogger<LayoutComputationService> _logger;

    public LayoutComputationService(IConfiguration configuration, ILogger<LayoutComputationService> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new ArgumentNullException("Connection string not found");
        _logger = logger;
    }

    /// <summary>
    /// Get or compute layout for a specific context
    /// Returns cached layout if available and current, otherwise computes new layout
    /// </summary>
    public async Task<LayoutData?> GetOrComputeLayoutAsync(string context, GraphNode[] nodes, GraphEdge[] edges)
    {
        try
        {
            var currentVersion = await GetCurrentGraphVersionAsync();
            var cachedLayout = await GetCachedLayoutAsync(context, currentVersion);

            if (cachedLayout != null)
            {
                _logger.LogInformation("Using cached layout for context {Context}, version {Version}",
                context, currentVersion);
                return cachedLayout;
            }

            _logger.LogInformation("Computing new layout for context {Context}, {NodeCount} nodes, {EdgeCount} edges",
            context, nodes.Length, edges.Length);

            // Compute layout based on context
            var layout = context.ToLower() switch
            {
                "2d" or "2d_flowchart" => await ComputeHierarchicalLayoutAsync(nodes, edges),
                "3d" or "3d_spatial" => await ComputeSpatialLayoutAsync(nodes, edges),
                _ => throw new ArgumentException($"Unknown layout context: {context}")
            };

            // Cache the computed layout
            await CacheLayoutAsync(context, currentVersion, layout, nodes.Length, edges.Length);

            return layout;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get or compute layout for context {Context}", context);
            return null;
        }
    }

    /// <summary>
    /// Compute 2D hierarchical flowchart layout
    /// Uses layered approach with relationship-aware positioning
    /// </summary>
    private async Task<LayoutData> ComputeHierarchicalLayoutAsync(GraphNode[] nodes, GraphEdge[] edges)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Simple grid layout for now - will be enhanced with proper hierarchical algorithm
            var positions = new Dictionary<string, NodePosition>();
            var nodesPerRow = (int)Math.Ceiling(Math.Sqrt(nodes.Length));
            var nodeSpacing = 150.0;

            for (int i = 0; i < nodes.Length; i++)
            {
                var row = i / nodesPerRow;
                var col = i % nodesPerRow;

                positions[nodes[i].Id] = new NodePosition
                {
                    X = col * nodeSpacing,
                    Y = row * nodeSpacing,
                    Layer = row,
                    Order = col,
                    Properties = new Dictionary<string, object>
                    {
                        ["hierarchical_level"] = row,
                        ["horizontal_order"] = col
                    }
                };
            }

            // Generate flow paths for edges (simplified)
            var renderingHints = new Dictionary<string, object>
            {
                ["algorithm_used"] = "simple_grid",
                ["node_spacing"] = nodeSpacing,
                ["total_layers"] = (int)Math.Ceiling((double)nodes.Length / nodesPerRow),
                ["layout_bounds"] = new { width = nodesPerRow * nodeSpacing, height = Math.Ceiling((double)nodes.Length / nodesPerRow) * nodeSpacing }
            };

            stopwatch.Stop();

            return new LayoutData
            {
                Algorithm = "hierarchical_dag_simple",
                Context = "2d_flowchart",
                Positions = positions,
                RenderingHints = renderingHints,
                Metadata = new LayoutMetadata
                {
                    NodeCount = nodes.Length,
                    EdgeCount = edges.Length,
                    ComputationTime = stopwatch.Elapsed,
                    AlgorithmMetrics = new Dictionary<string, object>
                    {
                        ["nodes_per_row"] = nodesPerRow,
                        ["total_rows"] = Math.Ceiling((double)nodes.Length / nodesPerRow)
                    }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compute hierarchical layout");
            throw;
        }
    }

    /// <summary>
    /// Compute 3D spatial layout using force-directed algorithm
    /// Creates clustered spatial representation
    /// </summary>
    private async Task<LayoutData> ComputeSpatialLayoutAsync(GraphNode[] nodes, GraphEdge[] edges)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Simple 3D spherical distribution for now - will be enhanced with force-directed algorithm
            var positions = new Dictionary<string, NodePosition>();
            var center = new { X = 0.0, Y = 0.0, Z = 0.0 };
            var radius = Math.Max(100.0, nodes.Length * 10.0);

            for (int i = 0; i < nodes.Length; i++)
            {
                // Distribute nodes on a sphere surface
                var phi = Math.Acos(1 - 2.0 * i / nodes.Length); // Latitude
                var theta = Math.PI * (1 + Math.Sqrt(5)) * i; // Longitude (golden ratio)

                var x = radius * Math.Sin(phi) * Math.Cos(theta);
                var y = radius * Math.Sin(phi) * Math.Sin(theta);
                var z = radius * Math.Cos(phi);

                // Determine cluster based on node type
                var cluster = DetermineCluster(nodes[i]);

                positions[nodes[i].Id] = new NodePosition
                {
                    X = x,
                    Y = y,
                    Z = z,
                    Cluster = cluster,
                    Properties = new Dictionary<string, object>
                    {
                        ["sphere_phi"] = phi,
                        ["sphere_theta"] = theta,
                        ["cluster_id"] = cluster
                    }
                };
            }

            var renderingHints = new Dictionary<string, object>
            {
                ["algorithm_used"] = "spherical_distribution",
                ["sphere_radius"] = radius,
                ["camera_position"] = new { x = 0, y = 0, z = radius * 2 },
                ["camera_target"] = center,
                ["clusters"] = GetClusterInfo(positions)
            };

            stopwatch.Stop();

            return new LayoutData
            {
                Algorithm = "force_directed_3d_simple",
                Context = "3d_spatial",
                Positions = positions,
                RenderingHints = renderingHints,
                Metadata = new LayoutMetadata
                {
                    NodeCount = nodes.Length,
                    EdgeCount = edges.Length,
                    ComputationTime = stopwatch.Elapsed,
                    AlgorithmMetrics = new Dictionary<string, object>
                    {
                        ["sphere_radius"] = radius,
                        ["unique_clusters"] = positions.Values.Select(p => p.Cluster).Distinct().Count()
                    }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compute spatial layout");
            throw;
        }
    }

    /// <summary>
    /// Determine cluster for a node based on its types and properties
    /// </summary>
    private string DetermineCluster(GraphNode node)
    {
        // Simple clustering logic - will be enhanced
        if (node.Types.Contains("document"))
            return "documents";
        if (node.Types.Contains("code"))
            return "code";
        if (node.Types.Contains("concept"))
            return "concepts";

        return "general";
    }

    /// <summary>
    /// Get cluster information for rendering hints
    /// </summary>
    private Dictionary<string, object> GetClusterInfo(Dictionary<string, NodePosition> positions)
    {
        var clusters = new Dictionary<string, object>();
        var clusterGroups = positions.Values.GroupBy(p => p.Cluster ?? "general");

        foreach (var group in clusterGroups)
        {
            var clusterPositions = group.ToList();
            if (clusterPositions.Count == 0) continue;

            var avgX = clusterPositions.Average(p => p.X);
            var avgY = clusterPositions.Average(p => p.Y);
            var avgZ = clusterPositions.Average(p => p.Z ?? 0);

            clusters[group.Key] = new
            {
                center = new { x = avgX, y = avgY, z = avgZ },
                count = clusterPositions.Count,
                radius = CalculateClusterRadius(clusterPositions)
            };
        }

        return clusters;
    }

    /// <summary>
    /// Calculate the radius of a cluster for rendering
    /// </summary>
    private double CalculateClusterRadius(List<NodePosition> positions)
    {
        if (positions.Count <= 1) return 50.0;

        var centerX = positions.Average(p => p.X);
        var centerY = positions.Average(p => p.Y);
        var centerZ = positions.Average(p => p.Z ?? 0);

        var maxDistance = positions.Max(p =>
        Math.Sqrt(Math.Pow(p.X - centerX, 2) + Math.Pow(p.Y - centerY, 2) + Math.Pow((p.Z ?? 0) - centerZ, 2)));

        return Math.Max(maxDistance, 50.0);
    }

    /// <summary>
    /// Get the current graph version for cache validation
    /// </summary>
    private async Task<long> GetCurrentGraphVersionAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = "SELECT COALESCE(MAX(version), 0) FROM graph_nodes";
        await using var command = new NpgsqlCommand(sql, connection);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt64(result ?? 0L);
    }

    /// <summary>
    /// Get cached layout if available and current
    /// </summary>
    private async Task<LayoutData?> GetCachedLayoutAsync(string context, long version)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        const string sql = @"
SELECT layout_data, computed_at, computation_time_ms
FROM graph_layouts 
WHERE graph_version = @version AND context = @context AND is_current = TRUE
LIMIT 1";

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@version", version);
        command.Parameters.AddWithValue("@context", context);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        var layoutDataJson = reader.GetString(0);
        var computedAt = reader.GetDateTime(1);
        var computationTimeMs = reader.GetInt32(2);

        var layoutData = JsonSerializer.Deserialize<LayoutData>(layoutDataJson);
        if (layoutData != null)
        {
            layoutData.ComputedAt = computedAt;
            layoutData.Metadata.ComputationTime = TimeSpan.FromMilliseconds(computationTimeMs);
        }

        return layoutData;
    }

    /// <summary>
    /// Cache computed layout for future use
    /// </summary>
    private async Task CacheLayoutAsync(string context, long version, LayoutData layout, int nodeCount, int edgeCount)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = await connection.BeginTransactionAsync();
        try
        {
            // Mark existing layouts for this context as not current
            const string invalidateSql = "UPDATE graph_layouts SET is_current = FALSE WHERE context = @context AND is_current = TRUE";
            await using var invalidateCommand = new NpgsqlCommand(invalidateSql, connection, transaction);
            invalidateCommand.Parameters.AddWithValue("@context", context);
            await invalidateCommand.ExecuteNonQueryAsync();

            // Insert new layout
            const string insertSql = @"
INSERT INTO graph_layouts (graph_version, context, algorithm, layout_data, is_current, node_count, edge_count, computation_time_ms)
VALUES (@version, @context, @algorithm, @layoutData::jsonb, @isCurrent, @nodeCount, @edgeCount, @computationTimeMs)";

            await using var insertCommand = new NpgsqlCommand(insertSql, connection, transaction);
            insertCommand.Parameters.AddWithValue("@version", version);
            insertCommand.Parameters.AddWithValue("@context", context);
            insertCommand.Parameters.AddWithValue("@algorithm", layout.Algorithm);
            insertCommand.Parameters.AddWithValue("@layoutData", JsonSerializer.Serialize(layout));
            insertCommand.Parameters.AddWithValue("@isCurrent", true);
            insertCommand.Parameters.AddWithValue("@nodeCount", nodeCount);
            insertCommand.Parameters.AddWithValue("@edgeCount", edgeCount);
            insertCommand.Parameters.AddWithValue("@computationTimeMs", (int)layout.Metadata.ComputationTime.TotalMilliseconds);

            await insertCommand.ExecuteNonQueryAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Cached layout for context {Context}, version {Version}, took {ComputationTime}ms",
            context, version, layout.Metadata.ComputationTime.TotalMilliseconds);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}