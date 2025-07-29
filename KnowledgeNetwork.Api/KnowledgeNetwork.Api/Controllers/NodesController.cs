using Microsoft.AspNetCore.Mvc;
using KnowledgeNetwork.Api.Models;
using KnowledgeNetwork.Api.Services;

namespace KnowledgeNetwork.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NodesController : ControllerBase
{
    private readonly DatabaseService _databaseService;

    public NodesController(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [HttpGet]
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

    [HttpPost]
    public async Task<ActionResult<Node>> CreateNode(CreateNodeRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest(new { error = "Title is required" });
            }

            var node = await _databaseService.CreateNodeAsync(request);
            return CreatedAtAction(nameof(GetNodes), new { id = node.Id }, node);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to create node", message = ex.Message });
        }
    }
}