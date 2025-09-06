using KnowledgeNetwork.Domains.Code.Analyzers.Classes.Abstractions;
using KnowledgeNetwork.Domains.Code.Analyzers.Classes.RelationshipAnalyzers;
using KnowledgeNetwork.Domains.Code.Analyzers.Classes.Utilities;
using KnowledgeNetwork.Domains.Code.Models.Classes;
using KnowledgeNetwork.Domains.Code.Models.Classes.ClassEnums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Classes;

/// <summary>
/// Analyzes class-level relationships within C# code including inheritance, interfaces, composition, and dependencies
/// </summary>
public class CSharpClassRelationshipAnalyzer(
    ILogger<CSharpClassRelationshipAnalyzer> logger,
    IFileNameResolver fileNameResolver,
    IClassNodeFactory classNodeFactory,
    ISyntaxUtilities syntaxUtilities,
    IInheritanceAnalyzer inheritanceAnalyzer,
    IInterfaceImplementationAnalyzer interfaceImplementationAnalyzer,
    ICompositionAnalyzer compositionAnalyzer,
    IDependencyAnalyzer dependencyAnalyzer,
    INestedClassAnalyzer nestedClassAnalyzer) : ICSharpClassRelationshipAnalyzer
{
    private readonly ILogger<CSharpClassRelationshipAnalyzer> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFileNameResolver _fileNameResolver = fileNameResolver ?? throw new ArgumentNullException(nameof(fileNameResolver));
    private readonly IClassNodeFactory _classNodeFactory = classNodeFactory ?? throw new ArgumentNullException(nameof(classNodeFactory));
    private readonly ISyntaxUtilities _syntaxUtilities = syntaxUtilities ?? throw new ArgumentNullException(nameof(syntaxUtilities));
    private readonly IInheritanceAnalyzer _inheritanceAnalyzer = inheritanceAnalyzer ?? throw new ArgumentNullException(nameof(inheritanceAnalyzer));
    private readonly IInterfaceImplementationAnalyzer _interfaceImplementationAnalyzer = interfaceImplementationAnalyzer ?? throw new ArgumentNullException(nameof(interfaceImplementationAnalyzer));
    private readonly ICompositionAnalyzer _compositionAnalyzer = compositionAnalyzer ?? throw new ArgumentNullException(nameof(compositionAnalyzer));
    private readonly IDependencyAnalyzer _dependencyAnalyzer = dependencyAnalyzer ?? throw new ArgumentNullException(nameof(dependencyAnalyzer));
    private readonly INestedClassAnalyzer _nestedClassAnalyzer = nestedClassAnalyzer ?? throw new ArgumentNullException(nameof(nestedClassAnalyzer));


    /// <summary>
    /// Analyzes class relationships within a compilation unit (file)
    /// </summary>
    public async Task<ClassRelationshipGraph?> AnalyzeFileAsync(
        Compilation compilation,
        CompilationUnitSyntax compilationUnit,
        string fileName = "")
    {
        try
        {
            var effectiveFileName = _fileNameResolver.ResolveEffectiveFileName(compilationUnit, fileName);
            _logger.LogInformation("Starting class relationship analysis for file: {FileName}", effectiveFileName);

            var semanticModel = compilation.GetSemanticModel(compilationUnit.SyntaxTree);
            var graph = new ClassRelationshipGraph
            {
                ScopeName = effectiveFileName,
                ScopeType = ClassAnalysisScope.File,
                Location = _syntaxUtilities.GetLocationInfo(compilationUnit, effectiveFileName)
            };

            // Extract all class/type declarations
            var typeDeclarations = compilationUnit.DescendantNodes()
                .OfType<BaseTypeDeclarationSyntax>()
                .ToList();

            _logger.LogDebug("Found {Count} type declarations in file", typeDeclarations.Count);

            // Create class nodes using the factory
            foreach (var typeDeclaration in typeDeclarations)
            {
                var classNode = await _classNodeFactory.CreateClassNodeAsync(semanticModel, typeDeclaration, effectiveFileName);
                if (classNode != null)
                {
                    graph.Classes.Add(classNode);
                }
            }

            // Analyze relationships using specialized analyzers
            await _inheritanceAnalyzer.AnalyzeAsync(semanticModel, graph, typeDeclarations);
            await _interfaceImplementationAnalyzer.AnalyzeAsync(semanticModel, graph, typeDeclarations);
            await _compositionAnalyzer.AnalyzeAsync(semanticModel, graph, typeDeclarations);
            await _dependencyAnalyzer.AnalyzeAsync(semanticModel, graph, typeDeclarations);
            await _nestedClassAnalyzer.AnalyzeAsync(semanticModel, graph, typeDeclarations);

            _logger.LogInformation("Completed class relationship analysis. Found {ClassCount} classes, {InheritanceCount} inheritance relationships, {InterfaceCount} interface implementations, {CompositionCount} composition relationships, {DependencyCount} dependency relationships, {NestedCount} nested class relationships",
                graph.Classes.Count,
                graph.InheritanceRelationships.Count,
                graph.InterfaceImplementations.Count,
                graph.CompositionRelationships.Count,
                graph.DependencyRelationships.Count,
                graph.NestedClassRelationships.Count);

            return graph;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing class relationships for file: {FileName}", fileName);
            return null;
        }
    }
}