using Microsoft.AspNetCore.Mvc;
using KnowledgeNetwork.Api.Models;
using KnowledgeNetwork.Api.Services;

namespace KnowledgeNetwork.Api.Controllers;

/// <summary>
/// API controller for analyzing C# code using Roslyn compiler APIs
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CodeAnalysisController : ControllerBase
{
    private readonly IRoslynAnalysisService _analysisService;
    private readonly ILogger<CodeAnalysisController> _logger;

    /// <summary>
    /// Initializes a new instance of the CodeAnalysisController
    /// </summary>
    /// <param name="analysisService">The Roslyn analysis service</param>
    /// <param name="logger">The logger instance</param>
    public CodeAnalysisController(IRoslynAnalysisService analysisService, ILogger<CodeAnalysisController> logger)
    {
        _analysisService = analysisService ?? throw new ArgumentNullException(nameof(analysisService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Analyzes C# code and returns structural information
    /// </summary>
    /// <param name="request">The code analysis request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Analysis results including code elements and relationships</returns>
    /// <response code="200">Analysis completed successfully</response>
    /// <response code="400">Invalid request or malformed code</response>
    /// <response code="500">Internal server error during analysis</response>
    [HttpPost("analyze")]
    [ProducesResponseType(typeof(CodeAnalysisResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CodeAnalysisResponse>> AnalyzeAsync(
        [FromBody] CodeAnalysisRequest request, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate model state
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for code analysis request");
                return BadRequest(ModelState);
            }

            // Validate code syntax before processing
            if (!string.IsNullOrWhiteSpace(request.Code) && !_analysisService.ValidateSourceCode(request.Code))
            {
                _logger.LogWarning("Invalid C# syntax provided in analysis request");
                ModelState.AddModelError(nameof(request.Code), "The provided code contains syntax errors");
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Starting code analysis for {FileNameOrLength} characters", 
                request.FileName ?? $"{request.Code.Length}");

            // Perform the analysis
            var response = await _analysisService.AnalyzeCodeAsync(request, cancellationToken);

            _logger.LogInformation("Code analysis completed successfully. Found {ClassCount} classes, {MethodCount} methods", 
                response.Summary.ClassCount, response.Summary.MethodCount);

            return Ok(response);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Code analysis was cancelled");
            return StatusCode(StatusCodes.Status499ClientClosedRequest);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument provided for code analysis");
            ModelState.AddModelError("Request", ex.Message);
            return BadRequest(ModelState);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during code analysis");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                "An error occurred while analyzing the code. Please try again.");
        }
    }

    /// <summary>
    /// Health check endpoint for the code analysis service
    /// </summary>
    /// <returns>Service health status</returns>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<object> GetHealth()
    {
        return Ok(new { 
            Status = "Healthy", 
            Service = "Code Analysis API",
            Timestamp = DateTime.UtcNow 
        });
    }
}