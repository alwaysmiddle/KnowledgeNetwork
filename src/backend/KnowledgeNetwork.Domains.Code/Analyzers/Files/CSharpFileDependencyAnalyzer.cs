using KnowledgeNetwork.Domains.Code.Analyzers.Files.Abstractions;
using KnowledgeNetwork.Domains.Code.Models.Files;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Files;

/// <summary>
/// Analyzes file-level dependencies within C# projects including using statements, namespace dependencies, and assembly references
/// </summary>
public class CSharpFileDependencyAnalyzer(ILogger<CSharpFileDependencyAnalyzer> logger, IFileNodeFactory fileNodeFactory,
    IUsingDependencyAnalyzer usingDependencyAnalyzer, INamespaceDependencyAnalyzer namespaceDependencyAnalyzer,
    ITypeReferenceDependencyAnalyzer typeReferenceDependencyAnalyzer, IAssemblyDependencyAnalyzer assemblyDependencyAnalyzer) : ICSharpFileDependencyAnalyzer
{
    /// <summary>
    /// Analyzes file dependencies within a compilation (project-level analysis)
    /// </summary>
    public async Task<FileDependencyGraph?> AnalyzeProjectAsync(Compilation compilation, string projectName = "", string projectPath = "")
    {
        try
        {
            logger.LogInformation("Starting file dependency analysis for project: {ProjectName}", projectName);

            var graph = new FileDependencyGraph
            {
                ScopeName = projectName,
                ScopeType = FileDependencyScope.Project
            };

            // Get all syntax trees (files) in the compilation
            var syntaxTrees = compilation.SyntaxTrees.ToList();
            logger.LogDebug("Found {Count} files in project", syntaxTrees.Count);

            // Create file nodes using the factory
            foreach (var syntaxTree in syntaxTrees)
            {
                var fileNode = await fileNodeFactory.CreateFileNodeAsync(compilation, syntaxTree, projectName);
                if (fileNode != null)
                {
                    graph.Files.Add(fileNode);
                }
            }

            // Analyze dependencies using specialized analyzers
            await usingDependencyAnalyzer.AnalyzeAsync(compilation, graph);
            await namespaceDependencyAnalyzer.AnalyzeAsync(compilation, graph);
            await typeReferenceDependencyAnalyzer.AnalyzeAsync(compilation, graph);
            await assemblyDependencyAnalyzer.AnalyzeAsync(compilation, graph);

            logger.LogInformation("Completed file dependency analysis. Found {FileCount} files, {UsingCount} using dependencies, {NamespaceCount} namespace dependencies, {TypeRefCount} type reference dependencies, {AssemblyCount} assembly dependencies",
                graph.Files.Count,
                graph.UsingDependencies.Count,
                graph.NamespaceDependencies.Count,
                graph.TypeReferenceDependencies.Count,
                graph.AssemblyDependencies.Count);

            return graph;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error analyzing file dependencies for project: {ProjectName}", projectName);
            return null;
        }
    }
}