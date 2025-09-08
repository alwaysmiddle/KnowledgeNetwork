using KnowledgeNetwork.Domains.Code.Models.Files;
using Microsoft.CodeAnalysis;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Files.Abstractions;

/// <summary>
/// Analyzes namespace dependencies between files
/// </summary>
public interface INamespaceDependencyAnalyzer
{
    /// <summary>
    /// Analyzes namespace dependencies between files in the graph
    /// </summary>
    /// <param name="compilation">The compilation containing all files</param>
    /// <param name="graph">The file dependency graph to populate with namespace dependencies</param>
    Task AnalyzeAsync(Compilation compilation, FileDependencyGraph graph);
}