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
    public async Task<ActionResult<List<Node>>> GetNodes()
    {
        try
        {
            var nodes = await _databaseService.GetAllNodesAsync();
            return Ok(nodes);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to retrieve nodes", message = ex.Message });
        }
    }

    [HttpGet("nodes/{id}")]
    public async Task<ActionResult<Node>> GetNode(int id)
    {
        try
        {
            // TODO: Implement GetNodeByIdAsync in DatabaseService
            return NotFound(new { error = "GetNode by ID not yet implemented" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to retrieve node", message = ex.Message });
        }
    }

    [HttpPost("nodes")]
    public async Task<ActionResult<Node>> CreateNode(CreateNodeRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest(new { error = "Title is required" });
            }

            var node = await _databaseService.CreateNodeAsync(request);
            return CreatedAtAction(nameof(GetNode), new { id = node.Id }, node);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to create node", message = ex.Message });
        }
    }
}