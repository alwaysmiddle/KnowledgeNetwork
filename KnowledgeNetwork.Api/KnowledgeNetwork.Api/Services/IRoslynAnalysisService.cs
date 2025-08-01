using KnowledgeNetwork.Api.Models;

namespace KnowledgeNetwork.Api.Services;

/// <summary>
/// Service interface for analyzing C# code using Roslyn compiler APIs
/// </summary>
public interface IRoslynAnalysisService
{
    /// <summary>
    /// Analyzes C# code and extracts structural information including classes, methods, and their relationships
    /// </summary>
    /// <param name="request">The code analysis request containing source code and analysis options</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>Analysis response containing extracted code elements and their relationships</returns>
    Task<CodeAnalysisResponse> AnalyzeCodeAsync(CodeAnalysisRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Validates that the provided source code is syntactically correct C# code
    /// </summary>
    /// <param name="sourceCode">The C# source code to validate</param>
    /// <returns>True if the code is syntactically valid, false otherwise</returns>
    bool ValidateSourceCode(string sourceCode);
}