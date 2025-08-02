using System.ComponentModel.DataAnnotations;
using KnowledgeNetwork.Api.Models.Visualization;

namespace KnowledgeNetwork.Api.Models.Analysis
{
    /// <summary>
    /// Request model for analyzing a file using the new architecture
    /// </summary>
    public class FileAnalysisRequest
    {
        /// <summary>
        /// The absolute path to the file to analyze
        /// </summary>
        [Required]
        public required string FilePath { get; init; }
        
        /// <summary>
        /// The project root directory (optional, defaults to file's directory)
        /// </summary>
        public string? ProjectRoot { get; init; }
        
        /// <summary>
        /// Whether to include visualization data in the response
        /// </summary>
        public bool IncludeVisualization { get; init; } = true;
        
        /// <summary>
        /// Whether to dispose the analysis result immediately (saves memory)
        /// </summary>
        public bool DisposeAfterAnalysis { get; init; } = true;
    }
    
    /// <summary>
    /// Response model for file analysis
    /// </summary>
    public class FileAnalysisResponse
    {
        /// <summary>
        /// Whether the analysis was successful
        /// </summary>
        public required bool Success { get; init; }
        
        /// <summary>
        /// Error message if analysis failed
        /// </summary>
        public string? ErrorMessage { get; init; }
        
        /// <summary>
        /// The language that was detected and analyzed
        /// </summary>
        public required string Language { get; init; }
        
        /// <summary>
        /// Basic analysis summary
        /// </summary>
        public required AnalysisSummary Summary { get; init; }
        
        /// <summary>
        /// Graph layout for visualization (if requested)
        /// </summary>
        public GraphLayout? Visualization { get; init; }
        
        /// <summary>
        /// Detailed analysis data (lightweight DTOs)
        /// </summary>
        public AnalysisData? Details { get; init; }
    }
    
    /// <summary>
    /// Summary of the analysis results
    /// </summary>
    public record AnalysisSummary
    {
        public required int TypeCount { get; init; }
        public required int MethodCount { get; init; }
        public required int PropertyCount { get; init; }
        public required int FieldCount { get; init; }
        public required int RelationshipCount { get; init; }
        public required string FilePath { get; init; }
        public required DateTime AnalyzedAt { get; init; }
    }
    
    /// <summary>
    /// Detailed analysis data
    /// </summary>
    public record AnalysisData
    {
        public required IReadOnlyList<TypeInfo> Types { get; init; }
        public required IReadOnlyList<MethodInfo> Methods { get; init; }
        public required IReadOnlyList<PropertyInfo> Properties { get; init; }
        public required IReadOnlyList<FieldInfo> Fields { get; init; }
        public required IReadOnlyList<SymbolRelationship> Relationships { get; init; }
    }
}