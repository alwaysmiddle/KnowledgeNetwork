using System.IO;
using System.Threading;
using System.Threading.Tasks;
using KnowledgeNetwork.Api.Models.Analysis;

namespace KnowledgeNetwork.Api.Services
{
    /// <summary>
    /// Base interface for all language-specific analyzers
    /// </summary>
    /// <typeparam name="TResult">The language-specific analysis result type</typeparam>
    public interface ILanguageAnalyzer<TResult> where TResult : ILanguageAnalysisResult
    {
        /// <summary>
        /// The language identifier this analyzer handles
        /// </summary>
        string Language { get; }
        
        /// <summary>
        /// File extensions this analyzer can process
        /// </summary>
        string[] SupportedExtensions { get; }
        
        /// <summary>
        /// Analyzes a source file and returns language-specific analysis results
        /// </summary>
        Task<TResult> AnalyzeAsync(
            FileInfo sourceFile, 
            DirectoryInfo projectRoot,
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Checks if this analyzer can handle the given file
        /// </summary>
        bool CanAnalyze(FileInfo file);
    }
}