using KnowledgeNetwork.Domains.Code.Analyzers.Classes.Abstractions;
using KnowledgeNetwork.Domains.Code.Analyzers.Classes.Utilities;
using KnowledgeNetwork.Domains.Code.Models.Classes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Classes.RelationshipAnalyzers;

/// <summary>
/// Analyzes nested class relationships
/// </summary>
public class NestedClassAnalyzer(
    ILogger<NestedClassAnalyzer> logger,
    ISyntaxUtilities syntaxUtilities) : INestedClassAnalyzer
{
    private readonly ILogger<NestedClassAnalyzer> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ISyntaxUtilities _syntaxUtilities = syntaxUtilities ?? throw new ArgumentNullException(nameof(syntaxUtilities));

    /// <summary>
    /// Analyzes nested class relationships within the provided type declarations
    /// </summary>
    public async Task AnalyzeAsync(SemanticModel semanticModel, ClassRelationshipGraph graph, List<BaseTypeDeclarationSyntax> typeDeclarations)
    {
        _logger.LogDebug("Starting nested class relationship analysis for {TypeCount} types", typeDeclarations.Count);

        try
        {
            var nestedClassCount = 0;

            foreach (var typeDeclaration in typeDeclarations)
            {
                var classSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration);
                if (classSymbol == null)
                {
                    _logger.LogWarning("Could not get symbol for type declaration: {TypeName}", 
                        typeDeclaration.GetType().Name);
                    continue;
                }

                var containerClass = graph.Classes.FirstOrDefault(c => 
                    c.FullName == classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                if (containerClass == null)
                {
                    _logger.LogWarning("Could not find container class in graph: {ClassName}", classSymbol.Name);
                    continue;
                }

                // Find nested types within this container
                var allMembers = _syntaxUtilities.GetAllMembers(typeDeclaration);
                var nestedTypes = allMembers.OfType<BaseTypeDeclarationSyntax>();

                nestedClassCount += nestedTypes.Count(nestedType => AnalyzeNestedType(nestedType, semanticModel, graph, containerClass));
            }

            _logger.LogDebug("Completed nested class analysis. Found {NestedClassCount} nested class relationships", 
                nestedClassCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during nested class relationship analysis");
            throw;
        }
    }

    /// <summary>
    /// Analyzes a single nested type and creates the relationship edge
    /// </summary>
    private bool AnalyzeNestedType(BaseTypeDeclarationSyntax nestedType, SemanticModel semanticModel, ClassRelationshipGraph graph,
        ClassNode containerClass)
    {
        try
        {
            var nestedSymbol = semanticModel.GetDeclaredSymbol(nestedType);
            if (nestedSymbol == null)
            {
                _logger.LogTrace("Could not get symbol for nested type");
                return false;
            }

            var nestedClass = graph.Classes.FirstOrDefault(c => 
                c.FullName == nestedSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
            if (nestedClass == null)
            {
                _logger.LogTrace("Nested class not found in graph: {NestedClassName}", nestedSymbol.Name);
                return false;
            }

            var nestedEdge = CreateNestedClassEdge(containerClass, nestedClass, nestedSymbol, nestedType);
            graph.NestedClassRelationships.Add(nestedEdge);

            _logger.LogTrace("Found nested class relationship: {Container} contains {Nested} (level {Level})",
                containerClass.Name, nestedSymbol.Name, nestedEdge.NestingLevel);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error analyzing nested type in container: {ContainerName}", containerClass.Name);
            return false;
        }
    }

    /// <summary>
    /// Creates a nested class relationship edge
    /// </summary>
    private NestedClassEdge CreateNestedClassEdge(ClassNode containerClass, ClassNode nestedClass, ISymbol nestedSymbol,
        BaseTypeDeclarationSyntax nestedType)
    {
        return new NestedClassEdge
        {
            ContainerClassId = containerClass.Id,
            NestedClassId = nestedClass.Id,
            NestedClassName = nestedSymbol.Name,
            NestingLevel = CalculateNestingLevel(nestedSymbol),
            NestedClassVisibility = nestedSymbol.DeclaredAccessibility.ToString(),
            IsStaticNested = nestedType.Modifiers.Any(SyntaxKind.StaticKeyword),
            NestedClassType = GetNestedClassType(nestedType),
            NestingLocation = _syntaxUtilities.GetLocationInfo(nestedType)
        };
    }

    /// <summary>
    /// Calculates the nesting level of a type (how deeply nested it is)
    /// </summary>
    private int CalculateNestingLevel(ISymbol symbol)
    {
        var level = 0;
        var containingType = symbol.ContainingType;

        while (containingType != null)
        {
            level++;
            containingType = containingType.ContainingType;
        }

        return level;
    }

    /// <summary>
    /// Gets a friendly name for the nested class type
    /// </summary>
    private string GetNestedClassType(BaseTypeDeclarationSyntax nestedType)
    {
        return nestedType switch
        {
            ClassDeclarationSyntax => "Class",
            InterfaceDeclarationSyntax => "Interface",
            StructDeclarationSyntax => "Struct",
            EnumDeclarationSyntax => "Enum",
            RecordDeclarationSyntax => "Record",
            _ => nestedType.GetType().Name
        };
    }
}