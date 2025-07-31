using Microsoft.AspNetCore.Mvc;
using KnowledgeNetwork.Api.Models;
using KnowledgeNetwork.Api.Services;

namespace KnowledgeNetwork.Api.Controllers;
[ApiController]
[Route("api/graphics-engine")]
public class GraphicsEngineController : ControllerBase
{
    private readonly DatabaseService _databaseService;

    public GraphicsEngineController(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

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
            return StatusCode(500, new { error = "Failed to retrieve nodes", message = ex.Message });
        }
    }

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
            return StatusCode(500, new { error = "Failed to retrieve node", message = ex.Message });
        }
    }

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
            return CreatedAtAction(nameof(GetNode), new { id = node.Id }, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to create node", message = ex.Message });
        }
    }

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
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to update node", message = ex.Message });
        }
    }

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
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to delete node", message = ex.Message });
        }
    }
}