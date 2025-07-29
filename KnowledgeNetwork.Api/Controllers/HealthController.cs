using Microsoft.AspNetCore.Mvc;

namespace KnowledgeNetwork.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { status = "OK", timestamp = DateTime.UtcNow });
    }
}