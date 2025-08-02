using System;
using KnowledgeNetwork.Api.Models.Analysis;
using KnowledgeNetwork.Api.Models.Visualization;

namespace KnowledgeNetwork.Api.Services.Visualization
{
    /// <summary>
    /// Service that orchestrates language-specific layout engines to generate graph visualizations
    /// </summary>
    public class VisualizationService : IVisualizationService
    {
        private readonly CSharpLayoutEngine _csharpEngine;
        // Future: Add more language engines here
        // private readonly TypeScriptLayoutEngine _typescriptEngine;
        // private readonly PythonLayoutEngine _pythonEngine;
        
        public VisualizationService()
        {
            _csharpEngine = new CSharpLayoutEngine();
        }
        
        /// <summary>
        /// Generates a graph layout from language analysis results using pattern matching
        /// </summary>
        public GraphLayout GenerateLayout(ILanguageAnalysisResult result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));
            
            // Use pattern matching to dispatch to the appropriate layout engine
            return result switch
            {
                CSharpAnalysisResult csResult => _csharpEngine.GenerateLayout(csResult),
                // Future language support:
                // TypeScriptAnalysisResult tsResult => _typescriptEngine.GenerateLayout(tsResult),
                // PythonAnalysisResult pyResult => _pythonEngine.GenerateLayout(pyResult),
                _ => throw new NotSupportedException($"No layout engine available for language: {result.Language}")
            };
        }
        
        /// <summary>
        /// Checks if a layout engine is available for the specified language
        /// </summary>
        public bool SupportsLanguage(string language)
        {
            return language?.ToLowerInvariant() switch
            {
                "csharp" or "c#" => true,
                // "typescript" or "ts" => true,
                // "python" or "py" => true,
                _ => false
            };
        }
    }
    
    /// <summary>
    /// Interface for the visualization service
    /// </summary>
    public interface IVisualizationService
    {
        /// <summary>
        /// Generates a graph layout from language analysis results
        /// </summary>
        GraphLayout GenerateLayout(ILanguageAnalysisResult result);
        
        /// <summary>
        /// Checks if a layout engine is available for the specified language
        /// </summary>
        bool SupportsLanguage(string language);
    }
}