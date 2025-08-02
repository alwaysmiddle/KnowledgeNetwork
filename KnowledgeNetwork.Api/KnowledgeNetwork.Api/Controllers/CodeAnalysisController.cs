using Microsoft.AspNetCore.Mvc;
using KnowledgeNetwork.Api.Models;
using KnowledgeNetwork.Api.Models.Analysis;
using KnowledgeNetwork.Api.Models.Visualization;
using KnowledgeNetwork.Api.Services;
using KnowledgeNetwork.Api.Services.Visualization;

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
    private readonly CSharpAnalyzer _csharpAnalyzer;
    private readonly IVisualizationService _visualizationService;

    /// <summary>
    /// Initializes a new instance of the CodeAnalysisController
    /// </summary>
    /// <param name="analysisService">The Roslyn analysis service</param>
    /// <param name="logger">The logger instance</param>
    public CodeAnalysisController(IRoslynAnalysisService analysisService, ILogger<CodeAnalysisController> logger)
    {
        _analysisService = analysisService ?? throw new ArgumentNullException(nameof(analysisService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _csharpAnalyzer = new CSharpAnalyzer();
        _visualizationService = new VisualizationService();
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
    
    /// <summary>
    /// Analyzes a file using the new architecture with language-specific analyzers and visualization
    /// </summary>
    /// <param name="request">The file analysis request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Analysis results with optional visualization data</returns>
    /// <response code="200">Analysis completed successfully</response>
    /// <response code="400">Invalid request or file not found</response>
    /// <response code="500">Internal server error during analysis</response>
    [HttpPost("analyze-file")]
    [ProducesResponseType(typeof(FileAnalysisResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FileAnalysisResponse>> AnalyzeFileAsync(
        [FromBody] FileAnalysisRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate model state
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for file analysis request");
                return BadRequest(ModelState);
            }
            
            // Validate file exists
            var fileInfo = new FileInfo(request.FilePath);
            if (!fileInfo.Exists)
            {
                _logger.LogWarning("File not found: {FilePath}", request.FilePath);
                ModelState.AddModelError(nameof(request.FilePath), "File not found");
                return BadRequest(ModelState);
            }
            
            // Determine project root
            var projectRoot = !string.IsNullOrEmpty(request.ProjectRoot) 
                ? new DirectoryInfo(request.ProjectRoot)
                : fileInfo.Directory!;
                
            _logger.LogInformation("Starting file analysis for {FilePath}", request.FilePath);
            
            // Check if we can analyze this file
            if (!_csharpAnalyzer.CanAnalyze(fileInfo))
            {
                return Ok(new FileAnalysisResponse
                {
                    Success = false,
                    ErrorMessage = $"File type not supported: {fileInfo.Extension}",
                    Language = "unknown",
                    Summary = new AnalysisSummary
                    {
                        TypeCount = 0,
                        MethodCount = 0,
                        PropertyCount = 0,
                        FieldCount = 0,
                        RelationshipCount = 0,
                        FilePath = request.FilePath,
                        AnalyzedAt = DateTime.UtcNow
                    }
                });
            }
            
            // Perform the analysis
            using var analysisResult = await _csharpAnalyzer.AnalyzeAsync(fileInfo, projectRoot, cancellationToken);
            
            // Create summary
            var summary = new AnalysisSummary
            {
                TypeCount = analysisResult.Types.Count,
                MethodCount = analysisResult.Methods.Count,
                PropertyCount = analysisResult.Properties.Count,
                FieldCount = analysisResult.Fields.Count,
                RelationshipCount = analysisResult.Relationships.Count,
                FilePath = request.FilePath,
                AnalyzedAt = analysisResult.AnalyzedAt
            };
            
            // Generate visualization if requested
            GraphLayout? visualization = null;
            if (request.IncludeVisualization)
            {
                visualization = _visualizationService.GenerateLayout(analysisResult);
            }
            
            // Create response
            var response = new FileAnalysisResponse
            {
                Success = true,
                Language = analysisResult.Language,
                Summary = summary,
                Visualization = visualization,
                Details = new AnalysisData
                {
                    Types = analysisResult.Types,
                    Methods = analysisResult.Methods,
                    Properties = analysisResult.Properties,
                    Fields = analysisResult.Fields,
                    Relationships = analysisResult.Relationships
                }
            };
            
            _logger.LogInformation(
                "File analysis completed successfully. Found {TypeCount} types, {MethodCount} methods",
                summary.TypeCount, summary.MethodCount);
            
            return Ok(response);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("File analysis was cancelled");
            return StatusCode(StatusCodes.Status499ClientClosedRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during file analysis");
            return Ok(new FileAnalysisResponse
            {
                Success = false,
                ErrorMessage = ex.Message,
                Language = "unknown",
                Summary = new AnalysisSummary
                {
                    TypeCount = 0,
                    MethodCount = 0,
                    PropertyCount = 0,
                    FieldCount = 0,
                    RelationshipCount = 0,
                    FilePath = request.FilePath,
                    AnalyzedAt = DateTime.UtcNow
                }
            });
        }
    }
}