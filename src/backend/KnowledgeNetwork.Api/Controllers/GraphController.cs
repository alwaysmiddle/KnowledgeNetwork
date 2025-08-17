using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using KnowledgeNetwork.Core.Models.Core;
using KnowledgeNetwork.Core.Models.Constants;
using KnowledgeNetwork.Core.Models.Requests.Graph;
using KnowledgeNetwork.Core.Models.Requests.Filters;
using KnowledgeNetwork.Core.Models.Responses.Graph;
using KnowledgeNetwork.Core.Models.Responses.Metadata;
using KnowledgeNetwork.Domains.Code.Services;

namespace KnowledgeNetwork.Api.Controllers;

/// <summary>
/// Controller for unified graph operations (expand, query, create views)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class GraphController(
    CSharpAnalysisService cSharpAnalysisService,
    ILogger<GraphController> logger)
    : ControllerBase
{
    // In-memory storage for nodes (in a real implementation, this would be a database)
    private static readonly Dictionary<string, List<KnowledgeNode>> _nodeStorage = new();

    /// <summary>
    /// Expand a specific node to load its children and relationships
    /// </summary>
    /// <param name="request">Expand node request with node ID and depth</param>
    /// <returns>Expanded node with children and relationships</returns>
    [HttpPost("expand")]
    [ProducesResponseType(typeof(ExpandedNode), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ExpandedNode>> ExpandNode([FromBody] ExpandNodeRequest request)
    {
        try
        {
            logger.LogInformation("Expanding node {NodeId} with depth {Depth}", request.NodeId, request.Depth);
            var stopwatch = Stopwatch.StartNew();

            // Validate request
            if (string.IsNullOrWhiteSpace(request.NodeId))
            {
                return BadRequest("NodeId is required for expansion.");
            }

            // Find the node across all stored contexts
            KnowledgeNode? targetNode = null;
            List<KnowledgeNode>? allNodes = null;

            foreach (var context in _nodeStorage.Values)
            {
                targetNode = FindNodeById(context, request.NodeId);
                if (targetNode != null)
                {
                    allNodes = context;
                    break;
                }
            }

            if (targetNode == null)
            {
                return NotFound($"Node with ID '{request.NodeId}' not found.");
            }

            // Get children based on the node's contains relationship
            var children = new List<KnowledgeNode>();
            if (allNodes != null)
            {
                foreach (var nodeRef in targetNode.Contains)
                {
                    var childNode = FindNodeById(allNodes, nodeRef.NodeId);
                    if (childNode != null)
                    {
                        children.Add(childNode);
                    }
                }
            }

            // Include relationships if requested
            List<RelationshipPair>? relationships = null;
            if (request.IncludeRelationships == true)
            {
                relationships = targetNode.Relationships;
            }

            var expandedNode = new ExpandedNode
            {
                Node = targetNode,
                Children = children,
                Relationships = relationships
            };

            stopwatch.Stop();
            logger.LogInformation("Node expansion completed in {Duration}ms. Found {ChildCount} children",
                stopwatch.ElapsedMilliseconds, children.Count);

            return Ok(expandedNode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred during node expansion");
            return StatusCode(500, "An error occurred during node expansion. Please check the logs for details.");
        }
    }

    /// <summary>
    /// Query the graph with filters to find specific nodes
    /// </summary>
    /// <param name="request">Graph query request with filters</param>
    /// <returns>Filtered graph response</returns>
    [HttpPost("query")]
    [ProducesResponseType(typeof(UnifiedGraphResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UnifiedGraphResponse>> QueryGraph([FromBody] UnifiedGraphRequest request)
    {
        try
        {
            logger.LogInformation("Querying graph with {FilterCount} filters", request.Filters?.Count ?? 0);
            var stopwatch = Stopwatch.StartNew();

            // Get nodes for the specified analysis
            var allNodes = GetNodesForAnalysis(request.AnalysisId);
            if (allNodes == null || !allNodes.Any())
            {
                return Ok(new UnifiedGraphResponse
                {
                    Nodes = new List<KnowledgeNode>(),
                    Metadata = new GraphMetadata
                    {
                        TotalNodes = 0,
                        Languages = new List<string>(),
                        Timestamp = DateTime.UtcNow.ToString("O")
                    }
                });
            }

            // Apply filters
            var filteredNodes = allNodes.AsEnumerable();
            
            if (request.Filters != null)
            {
                foreach (var filter in request.Filters)
                {
                    filteredNodes = ApplyFilter(filteredNodes, filter);
                }
            }

            // Apply depth limitation
            var resultNodes = filteredNodes.ToList();
            if (request.Depth.HasValue)
            {
                resultNodes = LimitDepth(resultNodes, request.Depth.Value);
            }

            var response = new UnifiedGraphResponse
            {
                Nodes = resultNodes,
                Metadata = new GraphMetadata
                {
                    TotalNodes = resultNodes.Count,
                    Languages = new List<string> { "csharp" },
                    Timestamp = DateTime.UtcNow.ToString("O"),
                    Duration = stopwatch.Elapsed,
                    Version = "1.0.0"
                }
            };

            stopwatch.Stop();
            logger.LogInformation("Graph query completed in {Duration}ms. Returned {NodeCount} nodes",
                stopwatch.ElapsedMilliseconds, resultNodes.Count);

            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred during graph query");
            return StatusCode(500, "An error occurred during graph query. Please check the logs for details.");
        }
    }

    /// <summary>
    /// Create a view node that aggregates or filters other nodes
    /// </summary>
    /// <param name="request">View creation request</param>
    /// <returns>Created view node</returns>
    [HttpPost("create-view")]
    [ProducesResponseType(typeof(KnowledgeNode), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<KnowledgeNode>> CreateView([FromBody] ViewCreationRequest request)
    {
        try
        {
            logger.LogInformation("Creating view '{ViewName}' with {NodeCount} nodes", 
                request.Name, request.NodeIds?.Count ?? 0);
            var stopwatch = Stopwatch.StartNew();

            // Validate request
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("View name is required.");
            }

            // Create the view node
            var viewNode = new KnowledgeNode
            {
                Id = $"view-{Guid.NewGuid()}",
                Type = new NodeType
                {
                    Primary = PrimaryNodeType.View,
                    Secondary = request.AggregationType ?? "custom-view"
                },
                Label = request.Name,
                IsView = true,
                IsPersisted = false,
                Contains = new List<NodeReference>(),
                Properties = new Dictionary<string, object?>
                {
                    ["viewType"] = request.AggregationType ?? "custom",
                    ["createdBy"] = "api",
                    ["timestamp"] = DateTime.UtcNow.ToString("O"),
                    ["filter"] = request.Filter
                },
                Metrics = new NodeMetrics(),
                Visualization = new VisualizationHints
                {
                    PreferredLayout = "force-directed",
                    Collapsed = false,
                    Color = "#9C27B0" // Purple for views
                }
            };

            // Add node references if specified
            if (request.NodeIds != null)
            {
                for (int i = 0; i < request.NodeIds.Count; i++)
                {
                    viewNode.Contains.Add(new NodeReference
                    {
                        NodeId = request.NodeIds[i],
                        Role = "aggregated",
                        Order = i
                    });
                }

                viewNode.Metrics.NodeCount = request.NodeIds.Count;
            }

            // Store the view (in a real implementation, this would be persisted)
            // For now, we'll add it to a special "views" context
            const string viewContext = "views";
            if (!_nodeStorage.ContainsKey(viewContext))
            {
                _nodeStorage[viewContext] = new List<KnowledgeNode>();
            }
            _nodeStorage[viewContext].Add(viewNode);

            stopwatch.Stop();
            logger.LogInformation("View creation completed in {Duration}ms. Created view '{ViewId}'",
                stopwatch.ElapsedMilliseconds, viewNode.Id);

            return Ok(viewNode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred during view creation");
            return StatusCode(500, "An error occurred during view creation. Please check the logs for details.");
        }
    }

    /// <summary>
    /// Store nodes from analysis for later operations (temporary endpoint for testing)
    /// </summary>
    /// <param name="analysisId">Analysis identifier</param>
    /// <param name="nodes">Nodes to store</param>
    /// <returns>Storage confirmation</returns>
    [HttpPost("store/{analysisId}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> StoreNodes(string analysisId, [FromBody] List<KnowledgeNode> nodes)
    {
        try
        {
            _nodeStorage[analysisId] = nodes;
            
            logger.LogInformation("Stored {NodeCount} nodes for analysis '{AnalysisId}'", 
                nodes.Count, analysisId);

            return Ok(new
            {
                analysisId = analysisId,
                nodeCount = nodes.Count,
                timestamp = DateTime.UtcNow.ToString("O")
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred during node storage");
            return StatusCode(500, "An error occurred during node storage.");
        }
    }

    #region Private Helper Methods

    private KnowledgeNode? FindNodeById(List<KnowledgeNode> nodes, string nodeId)
    {
        return nodes.FirstOrDefault(n => n.Id.Equals(nodeId, StringComparison.OrdinalIgnoreCase));
    }

    private List<KnowledgeNode>? GetNodesForAnalysis(string analysisId)
    {
        if (string.IsNullOrWhiteSpace(analysisId))
        {
            // Return all nodes from all contexts
            return _nodeStorage.Values.SelectMany(nodes => nodes).ToList();
        }

        return _nodeStorage.TryGetValue(analysisId, out var nodes) ? nodes : null;
    }

    private IEnumerable<KnowledgeNode> ApplyFilter(IEnumerable<KnowledgeNode> nodes, NodeFilter filter)
    {
        return filter.Field.ToLowerInvariant() switch
        {
            "type.primary" => FilterByStringProperty(nodes, n => n.Type.Primary, filter),
            "type.secondary" => FilterByStringProperty(nodes, n => n.Type.Secondary, filter),
            "label" => FilterByStringProperty(nodes, n => n.Label, filter),
            "isview" => FilterByBoolProperty(nodes, n => n.IsView, filter),
            "sourcelanguage" => FilterByStringProperty(nodes, n => n.SourceLanguage, filter),
            "metrics.complexity" => FilterByIntProperty(nodes, n => n.Metrics.Complexity, filter),
            "metrics.linesofcode" => FilterByIntProperty(nodes, n => n.Metrics.LinesOfCode, filter),
            _ => nodes // Unknown filter field, return unchanged
        };
    }

    private IEnumerable<KnowledgeNode> FilterByStringProperty(
        IEnumerable<KnowledgeNode> nodes, 
        Func<KnowledgeNode, string?> propertySelector, 
        NodeFilter filter)
    {
        var filterValue = filter.Value?.ToString() ?? "";
        
        return filter.Operator.ToLowerInvariant() switch
        {
            "equals" => nodes.Where(n => string.Equals(propertySelector(n), filterValue, StringComparison.OrdinalIgnoreCase)),
            "contains" => nodes.Where(n => propertySelector(n)?.Contains(filterValue, StringComparison.OrdinalIgnoreCase) == true),
            "startswith" => nodes.Where(n => propertySelector(n)?.StartsWith(filterValue, StringComparison.OrdinalIgnoreCase) == true),
            "endswith" => nodes.Where(n => propertySelector(n)?.EndsWith(filterValue, StringComparison.OrdinalIgnoreCase) == true),
            _ => nodes
        };
    }

    private IEnumerable<KnowledgeNode> FilterByBoolProperty(
        IEnumerable<KnowledgeNode> nodes,
        Func<KnowledgeNode, bool> propertySelector,
        NodeFilter filter)
    {
        if (bool.TryParse(filter.Value?.ToString(), out var boolValue))
        {
            return nodes.Where(n => propertySelector(n) == boolValue);
        }
        return nodes;
    }

    private IEnumerable<KnowledgeNode> FilterByIntProperty(
        IEnumerable<KnowledgeNode> nodes,
        Func<KnowledgeNode, int?> propertySelector,
        NodeFilter filter)
    {
        if (int.TryParse(filter.Value?.ToString(), out var intValue))
        {
            return filter.Operator.ToLowerInvariant() switch
            {
                "equals" => nodes.Where(n => propertySelector(n) == intValue),
                "greater" => nodes.Where(n => propertySelector(n) > intValue),
                "less" => nodes.Where(n => propertySelector(n) < intValue),
                "greaterequal" => nodes.Where(n => propertySelector(n) >= intValue),
                "lessequal" => nodes.Where(n => propertySelector(n) <= intValue),
                _ => nodes
            };
        }
        return nodes;
    }

    private List<KnowledgeNode> LimitDepth(List<KnowledgeNode> nodes, int maxDepth)
    {
        // For simplicity, return nodes at top level. In a real implementation,
        // this would traverse the graph hierarchy
        return nodes.Where(n => !n.Type.Primary.Contains("operation")).ToList();
    }

    #endregion
}