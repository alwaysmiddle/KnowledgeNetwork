using System;
using System.IO;

namespace KnowledgeNetwork.Api.Models.Analysis
{
    /// <summary>
    /// Base interface for all language-specific analysis results.
    /// Provides common properties while allowing each language to maintain its rich compiler data.
    /// </summary>
    public interface ILanguageAnalysisResult : IDisposable
    {
        /// <summary>
        /// The programming language identifier (e.g., "csharp", "typescript", "python")
        /// </summary>
        string Language { get; }
        
        /// <summary>
        /// The source file that was analyzed
        /// </summary>
        FileInfo SourceFile { get; }
        
        /// <summary>
        /// When the analysis was performed
        /// </summary>
        DateTime AnalyzedAt { get; }
        
        /// <summary>
        /// The project root directory for context
        /// </summary>
        DirectoryInfo ProjectRoot { get; }
        
        /// <summary>
        /// Gets the relative path from project root to source file
        /// </summary>
        string RelativePath { get; }
        
        /// <summary>
        /// Indicates if this is likely a test file based on naming conventions
        /// </summary>
        bool IsTestFile { get; }
        
        /// <summary>
        /// Indicates if this is generated code
        /// </summary>
        bool IsGeneratedCode { get; }
    }
}