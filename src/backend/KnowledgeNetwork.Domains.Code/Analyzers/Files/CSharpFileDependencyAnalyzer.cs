using KnowledgeNetwork.Domains.Code.Analyzers.Files.Abstractions;
using KnowledgeNetwork.Domains.Code.Models.Files;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Files;

/// <summary>
/// Analyzes file-level dependencies within C# projects including using statements, namespace dependencies, and assembly references
/// </summary>
public class CSharpFileDependencyAnalyzer(
    ILogger<CSharpFileDependencyAnalyzer> logger,
    IFileNodeFactory fileNodeFactory,
    IUsingDependencyAnalyzer usingDependencyAnalyzer,
    INamespaceDependencyAnalyzer namespaceDependencyAnalyzer,
    ITypeReferenceDependencyAnalyzer typeReferenceDependencyAnalyzer,
    IAssemblyDependencyAnalyzer assemblyDependencyAnalyzer) : ICSharpFileDependencyAnalyzer
{
    private readonly ILogger<CSharpFileDependencyAnalyzer> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFileNodeFactory _fileNodeFactory = fileNodeFactory ?? throw new ArgumentNullException(nameof(fileNodeFactory));
    private readonly IUsingDependencyAnalyzer _usingDependencyAnalyzer = usingDependencyAnalyzer ?? throw new ArgumentNullException(nameof(usingDependencyAnalyzer));
    private readonly INamespaceDependencyAnalyzer _namespaceDependencyAnalyzer = namespaceDependencyAnalyzer ?? throw new ArgumentNullException(nameof(namespaceDependencyAnalyzer));
    private readonly ITypeReferenceDependencyAnalyzer _typeReferenceDependencyAnalyzer = typeReferenceDependencyAnalyzer ?? throw new ArgumentNullException(nameof(typeReferenceDependencyAnalyzer));
    private readonly IAssemblyDependencyAnalyzer _assemblyDependencyAnalyzer = assemblyDependencyAnalyzer ?? throw new ArgumentNullException(nameof(assemblyDependencyAnalyzer));

    /// <summary>
    /// Analyzes file dependencies within a compilation (project-level analysis)
    /// </summary>
    public async Task<FileDependencyGraph?> AnalyzeProjectAsync(Compilation compilation, string projectName = "",
        string projectPath = "")
    {
        try
        {
            _logger.LogInformation("Starting file dependency analysis for project: {ProjectName}", projectName);

            var graph = new FileDependencyGraph
            {
                ScopeName = projectName,
                ScopeType = FileDependencyScope.Project
            };

            // Get all syntax trees (files) in the compilation
            var syntaxTrees = compilation.SyntaxTrees.ToList();
            _logger.LogDebug("Found {Count} files in project", syntaxTrees.Count);

            // Create file nodes using the factory
            foreach (var syntaxTree in syntaxTrees)
            {
                var fileNode = await _fileNodeFactory.CreateFileNodeAsync(compilation, syntaxTree, projectName);
                if (fileNode != null)
                {
                    graph.Files.Add(fileNode);
                }
            }

            // Analyze dependencies using specialized analyzers
            await _usingDependencyAnalyzer.AnalyzeAsync(compilation, graph);
            await _namespaceDependencyAnalyzer.AnalyzeAsync(compilation, graph);
            await _typeReferenceDependencyAnalyzer.AnalyzeAsync(compilation, graph);
            await _assemblyDependencyAnalyzer.AnalyzeAsync(compilation, graph);

            _logger.LogInformation("Completed file dependency analysis. Found {FileCount} files, {UsingCount} using dependencies, {NamespaceCount} namespace dependencies, {TypeRefCount} type reference dependencies, {AssemblyCount} assembly dependencies",
                graph.Files.Count,
                graph.UsingDependencies.Count,
                graph.NamespaceDependencies.Count,
                graph.TypeReferenceDependencies.Count,
                graph.AssemblyDependencies.Count);

            return graph;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing file dependencies for project: {ProjectName}", projectName);
            return null;
        }
    }
}