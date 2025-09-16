using KnowledgeNetwork.Domains.Code.Models.Files;
using Microsoft.CodeAnalysis;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Files.Abstractions;

/// <summary>
/// Analyzes type reference dependencies between files
/// </summary>
public interface ITypeReferenceDependencyAnalyzer
{
    /// <summary>
    /// Analyzes type reference dependencies between files in the graph
    /// </summary>
    /// <param name="compilation">The compilation containing all files</param>
    /// <param name="graph">The file dependency graph to populate with type reference dependencies</param>
    Task AnalyzeAsync(Compilation compilation, FileDependencyGraph graph);
}