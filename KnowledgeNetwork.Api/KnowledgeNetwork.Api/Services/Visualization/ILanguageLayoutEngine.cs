using KnowledgeNetwork.Api.Models.Analysis;
using KnowledgeNetwork.Api.Models.Visualization;

namespace KnowledgeNetwork.Api.Services.Visualization
{
    /// <summary>
    /// Interface for language-specific layout engines that transform analysis results into graph layouts
    /// </summary>
    /// <typeparam name="TResult">The language-specific analysis result type</typeparam>
    public interface ILanguageLayoutEngine<TResult> where TResult : ILanguageAnalysisResult
    {
        /// <summary>
        /// The language this engine handles
        /// </summary>
        string Language { get; }
        
        /// <summary>
        /// Generates a graph layout from language-specific analysis results
        /// </summary>
        GraphLayout GenerateLayout(TResult result);
    }
}