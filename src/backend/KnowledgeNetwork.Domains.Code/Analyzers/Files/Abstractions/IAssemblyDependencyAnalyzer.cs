using KnowledgeNetwork.Domains.Code.Models.Files;
using Microsoft.CodeAnalysis;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Files.Abstractions;

/// <summary>
/// Analyzes external assembly dependencies for files
/// </summary>
public interface IAssemblyDependencyAnalyzer
{
    /// <summary>
    /// Analyzes external assembly dependencies for all files in the graph
    /// </summary>
    /// <param name="compilation">The compilation containing all files</param>
    /// <param name="graph">The file dependency graph to populate with assembly dependencies</param>
    Task AnalyzeAsync(Compilation compilation, FileDependencyGraph graph);
}