using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using KnowledgeNetwork.Api.Models.Requests;
using KnowledgeNetwork.Api.Models.Responses;
using KnowledgeNetwork.Api.Models.Summaries;
using KnowledgeNetwork.Api.Models.Metadata;
using KnowledgeNetwork.Domains.Code.Services;

namespace KnowledgeNetwork.Api.Controllers;

/// <summary>
/// Controller for code analysis operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CodeAnalysisController(
    CSharpAnalysisService cSharpAnalysisService,
    ILogger<CodeAnalysisController> logger)
    : ControllerBase
{
    /// <summary>
    /// Analyze C# source code and extract structural information
    /// </summary>
    /// <param name="request">Analysis request containing source code</param>
    /// <returns>Analysis results with extracted code structure</returns>
    [HttpPost("analyze")]
    [ProducesResponseType(typeof(AnalysisResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AnalysisResponse>> AnalyzeCode([FromBody] AnalysisRequest request)
    {
        try
        {
            logger.LogInformation("Starting code analysis for {Language}", request.Language ?? "auto-detect");
            var stopwatch = Stopwatch.StartNew();

            // For now, we only support C# analysis
            if (!string.IsNullOrEmpty(request.Language) && 
                !request.Language.Equals("csharp", StringComparison.OrdinalIgnoreCase) &&
                !request.Language.Equals("c#", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest($"Language '{request.Language}' is not currently supported. Only C# is supported.");
            }

            // Perform the analysis
            var analysisResult = await cSharpAnalysisService.AnalyzeAsync(request.Code);
            stopwatch.Stop();

            // Convert to API response format
            var response = new AnalysisResponse
            {
                Success = analysisResult.Success,
                LanguageId = analysisResult.LanguageId,
                Errors = analysisResult.Errors,
                Classes = analysisResult.Classes.Select(c => new ClassSummary
                {
                    Name = c.Name,
                    Namespace = c.Namespace,
                    Modifiers = c.Modifiers,
                    LineNumber = c.LineNumber
                }).ToList(),
                Methods = analysisResult.Methods.Select(m => new MethodSummary
                {
                    Name = m.Name,
                    ReturnType = m.ReturnType,
                    Modifiers = m.Modifiers,
                    Parameters = m.Parameters,
                    LineNumber = m.LineNumber,
                    ClassName = m.ClassName
                }).ToList(),
                Properties = analysisResult.Properties.Select(p => new PropertySummary
                {
                    Name = p.Name,
                    Type = p.Type,
                    Modifiers = p.Modifiers,
                    HasGetter = p.HasGetter,
                    HasSetter = p.HasSetter,
                    LineNumber = p.LineNumber,
                    ClassName = p.ClassName
                }).ToList(),
                UsingStatements = analysisResult.UsingStatements,
                Metadata = new AnalysisMetadata
                {
                    AnalyzedAt = DateTime.UtcNow,
                    Duration = stopwatch.Elapsed,
                    Version = "1.0.0"
                }
            };

            logger.LogInformation("Code analysis completed in {Duration}ms. Found {ClassCount} classes, {MethodCount} methods, {PropertyCount} properties",
                stopwatch.ElapsedMilliseconds, response.Classes.Count, response.Methods.Count, response.Properties.Count);

            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred during code analysis");
            return StatusCode(500, "An error occurred during code analysis. Please check the logs for details.");
        }
    }

    /// <summary>
    /// Health check endpoint for the code analysis service
    /// </summary>
    /// <returns>Health status of the service</returns>
    [HttpGet("health")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult> HealthCheck()
    {
        try
        {
            var isHealthy = await cSharpAnalysisService.IsHealthyAsync();
            
            if (isHealthy)
            {
                return Ok(new
                {
                    status = "healthy",
                    service = "CodeAnalysis",
                    timestamp = DateTime.UtcNow,
                    version = "1.0.0",
                    supportedLanguages = new[] { "csharp" }
                });
            }
            else
            {
                return StatusCode(503, new
                {
                    status = "unhealthy",
                    service = "CodeAnalysis",
                    timestamp = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Health check failed");
            return StatusCode(503, new
            {
                status = "unhealthy",
                service = "CodeAnalysis",
                timestamp = DateTime.UtcNow,
                error = "Health check failed"
            });
        }
    }

    /// <summary>
    /// Get information about supported languages and capabilities
    /// </summary>
    /// <returns>Service capabilities information</returns>
    [HttpGet("capabilities")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public ActionResult GetCapabilities()
    {
        return Ok(new
        {
            supportedLanguages = new[]
            {
                new
                {
                    id = "csharp",
                    name = "C#",
                    fileExtensions = new[] { ".cs" },
                    features = new[]
                    {
                        "syntax-analysis",
                        "class-extraction",
                        "method-extraction",
                        "property-extraction",
                        "using-statements",
                        "error-detection"
                    }
                }
            },
            version = "1.0.0",
            apiVersion = "v1"
        });
    }
}