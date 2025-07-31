using Microsoft.AspNetCore.Mvc;
using KnowledgeNetwork.Api.Models;
using KnowledgeNetwork.Api.Services;
using System.Diagnostics;

namespace KnowledgeNetwork.Api.Controllers;
/// <summary>
/// Graph API Controller - Universal node operations with composite view endpoints
/// Supports both traditional CRUD operations and ultra-fast composite queries
/// </summary>
[ApiController]
[Route("api/graph")]
public class GraphController : ControllerBase
{
    private readonly DatabaseService _databaseService;
    private readonly ILogger<GraphController> _logger;

    public GraphController(DatabaseService databaseService, ILogger<GraphController> logger)
    {
        _databaseService = databaseService;
        _logger = logger;
    }

    /// <summary>
    /// Ultra-fast composite endpoint - Returns everything needed for visualization in one call
    /// Supports different contexts (2D flowchart, 3D spatial) with selective loading
    /// </summary>
    [HttpGet("view")]
    public async Task<ActionResult<GraphViewResponse>> GetGraphView(
    [FromQuery] string context = "2d",
    [FromQuery] string[]? nodes = null,
    [FromQuery] int? depth = null,
    [FromQuery] string[]? include = null,
    [FromQuery] long? version = null)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Getting graph view: context={Context}, nodeCount={NodeCount}, depth={Depth}",
            context, nodes?.Length ?? 0, depth);

            var request = new GraphViewRequest
            {
                Context = context,
                NodeIds = nodes,
                TraversalDepth = depth,
                IncludeFields = include ?? new[] { "layout", "content" },
                ClientVersion = version
            };

            // For now, we'll return the basic structure
            // TODO: Implement full composite query with layouts
            var allNodes = await _databaseService.GetAllGraphNodesAsync();
            var filteredNodes = FilterNodesByRequest(allNodes, request);

            stopwatch.Stop();

            var response = new GraphViewResponse
            {
                Nodes = filteredNodes.ToArray(),
                Edges = Array.Empty<GraphEdge>(), // TODO: Implement edge retrieval
                Layout2D = null, // TODO: Implement layout computation
                Layout3D = null, // TODO: Implement layout computation
                Version = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Stats = new GraphViewStats
                {
                    NodesReturned = filteredNodes.Count,
                    EdgesReturned = 0,
                    TotalNodesInGraph = allNodes.Count,
                    TotalEdgesInGraph = 0,
                    QueryTime = stopwatch.Elapsed,
                    LayoutFromCache = false,
                    AppliedFilters = request.NodeIds?.Length > 0 ? new[] { "node_ids" } : Array.Empty<string>()
                }
            };

            _logger.LogInformation("Graph view completed in {ElapsedMs}ms, returned {NodeCount} nodes",
            stopwatch.ElapsedMilliseconds, response.NodesReturned);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get graph view");
            return StatusCode(500, new { error = "Failed to retrieve graph view", message = ex.Message });
        }
    }

    /// <summary>
    /// Get all nodes in the graph
    /// </summary>
    [HttpGet("nodes")]
    public async Task<ActionResult<List<GraphNodeResponse>>> GetNodes()
    {
        try
        {
            var nodes = await _databaseService.GetAllGraphNodesAsync();
            var response = nodes.Select(GraphNodeResponse.FromGraphNode).ToList();
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve nodes");
            return StatusCode(500, new { error = "Failed to retrieve nodes", message = ex.Message });
        }
    }

    /// <summary>
    /// Get a specific node by ID
    /// </summary>
    [HttpGet("nodes/{id}")]
    public async Task<ActionResult<GraphNodeResponse>> GetNode(string id)
    {
        try
        {
            var node = await _databaseService.GetGraphNodeByIdAsync(id);
            if (node == null)
            {
                return NotFound(new { error = $"Node with ID '{id}' not found" });
            }
            return Ok(GraphNodeResponse.FromGraphNode(node));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve node {NodeId}", id);
            return StatusCode(500, new { error = "Failed to retrieve node", message = ex.Message });
        }
    }

    /// <summary>
    /// Create a new node
    /// </summary>
    [HttpPost("nodes")]
    public async Task<ActionResult<GraphNodeResponse>> CreateNode(CreateGraphNodeRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Label))
            {
                return BadRequest(new { error = "Label is required" });
            }

            var node = await _databaseService.CreateGraphNodeAsync(request);
            var response = GraphNodeResponse.FromGraphNode(node);
            _logger.LogInformation("Created node {NodeId} with label '{Label}'", node.Id, node.Label);
            return CreatedAtAction(nameof(GetNode), new { id = node.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create node");
            return StatusCode(500, new { error = "Failed to create node", message = ex.Message });
        }
    }

    /// <summary>
    /// Update an existing node
    /// </summary>
    [HttpPut("nodes/{id}")]
    public async Task<ActionResult<GraphNodeResponse>> UpdateNode(string id, UpdateGraphNodeRequest request)
    {
        try
        {
            var updatedNode = await _databaseService.UpdateGraphNodeAsync(id, request);
            if (updatedNode == null)
            {
                return NotFound(new { error = $"Node with ID '{id}' not found" });
            }
            var response = GraphNodeResponse.FromGraphNode(updatedNode);
            _logger.LogInformation("Updated node {NodeId}", id);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update node {NodeId}", id);
            return StatusCode(500, new { error = "Failed to update node", message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a node
    /// </summary>
    [HttpDelete("nodes/{id}")]
    public async Task<ActionResult> DeleteNode(string id)
    {
        try
        {
            var deleted = await _databaseService.DeleteGraphNodeAsync(id);
            if (!deleted)
            {
                return NotFound(new { error = $"Node with ID '{id}' not found" });
            }
            _logger.LogInformation("Deleted node {NodeId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete node {NodeId}", id);
            return StatusCode(500, new { error = "Failed to delete node", message = ex.Message });
        }
    }

    /// <summary>
    /// Helper method to filter nodes based on request parameters
    /// TODO: Move to service layer for better separation of concerns
    /// </summary>
    private List<GraphNode> FilterNodesByRequest(List<GraphNode> allNodes, GraphViewRequest request)
    {
        var filtered = allNodes.AsEnumerable();

        // Filter by specific node IDs if provided
        if (request.NodeIds?.Length > 0)
        {
            var nodeIdSet = request.NodeIds.ToHashSet();
            filtered = filtered.Where(n => nodeIdSet.Contains(n.Id));
        }

        // TODO: Implement traversal depth filtering
        // TODO: Implement type-based filtering
        // TODO: Implement property-based filtering

        return filtered.ToList();
    }
}