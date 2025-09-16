using KnowledgeNetwork.Domains.Code.Models.Files;
using Microsoft.CodeAnalysis;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Files.Abstractions;

/// <summary>
/// Analyzes file-level dependencies within C# projects including using statements, namespace dependencies, and assembly references
/// </summary>
public interface ICSharpFileDependencyAnalyzer
{
    /// <summary>
    /// Analyzes file dependencies within a compilation (project-level analysis)
    /// </summary>
    /// <param name="compilation">The compilation to analyze</param>
    /// <param name="projectName">The name of the project being analyzed</param>
    /// <param name="projectPath">The path to the project being analyzed</param>
    /// <returns>A graph of file dependencies, or null if analysis failed</returns>
    Task<FileDependencyGraph?> AnalyzeProjectAsync(Compilation compilation, string projectName = "", string projectPath = "");
}