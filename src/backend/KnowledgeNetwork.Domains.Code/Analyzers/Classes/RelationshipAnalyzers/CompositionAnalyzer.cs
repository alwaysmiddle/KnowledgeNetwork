using KnowledgeNetwork.Domains.Code.Analyzers.Classes.Abstractions;
using KnowledgeNetwork.Domains.Code.Models.Classes;
using KnowledgeNetwork.Domains.Code.Models.Classes.ClassEnums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Classes.RelationshipAnalyzers;

/// <summary>
/// Analyzes composition relationships (has-a relationships through fields/properties)
/// </summary>
public class CompositionAnalyzer(ILogger<CompositionAnalyzer> logger, ISyntaxUtilities syntaxUtilities) : ICompositionAnalyzer
{
    private readonly ILogger<CompositionAnalyzer> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ISyntaxUtilities _syntaxUtilities = syntaxUtilities ?? throw new ArgumentNullException(nameof(syntaxUtilities));

    /// <summary>
    /// Analyzes composition relationships within the provided type declarations
    /// </summary>
    public async Task AnalyzeAsync(SemanticModel semanticModel, ClassRelationshipGraph graph, List<BaseTypeDeclarationSyntax> typeDeclarations)
    {
        _logger.LogDebug("Starting composition relationship analysis for {TypeCount} types", typeDeclarations.Count);

        try
        {
            var compositionCount = 0;

            foreach (var typeDeclaration in typeDeclarations)
            {
                var classSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration);
                if (classSymbol == null)
                {
                    _logger.LogWarning("Could not get symbol for type declaration: {TypeName}", typeDeclaration.GetType().Name);
                    continue;
                }

                var classNode = graph.Classes.FirstOrDefault(c => 
                    c.FullName == classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                if (classNode == null)
                {
                    _logger.LogWarning("Could not find class in graph: {ClassName}", classSymbol.Name);
                    continue;
                }

                var allMembers = _syntaxUtilities.GetAllMembers(typeDeclaration);

                // Analyze fields for composition relationships
                var memberDeclarationSyntaxes = allMembers as MemberDeclarationSyntax[] ?? allMembers.ToArray();
                var fields = memberDeclarationSyntaxes.OfType<FieldDeclarationSyntax>();
                foreach (var field in fields)
                {
                    var memberName = field.Declaration.Variables.FirstOrDefault()?.Identifier.ValueText ?? "";
                    if (AnalyzeCompositionFromMember(field.Declaration.Type, memberName, 
                        semanticModel, graph, classNode, field, CompositionAccessType.Field))
                    {
                        compositionCount++;
                    }
                }

                // Analyze properties for composition relationships  
                var properties = memberDeclarationSyntaxes.OfType<PropertyDeclarationSyntax>();
                foreach (var property in properties)
                {
                    if (AnalyzeCompositionFromMember(property.Type, property.Identifier.ValueText,
                        semanticModel, graph, classNode, property, CompositionAccessType.Property))
                    {
                        compositionCount++;
                    }
                }
            }

            _logger.LogDebug("Completed composition analysis. Found {CompositionCount} composition relationships", 
                compositionCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during composition relationship analysis");
            throw;
        }
    }

    /// <summary>
    /// Analyzes composition from a field or property
    /// </summary>
    private bool AnalyzeCompositionFromMember(TypeSyntax typeSyntax, string memberName, SemanticModel semanticModel,
        ClassRelationshipGraph graph, ClassNode containerClass, SyntaxNode memberNode, CompositionAccessType accessType)
    {
        try
        {
            if (semanticModel.GetSymbolInfo(typeSyntax).Symbol is not ITypeSymbol typeSymbol)
            {
                _logger.LogTrace("Could not resolve type symbol for member: {MemberName}", memberName);
                return false;
            }

            // Skip primitive types and system types
            if (ShouldSkipType(typeSymbol))
            {
                _logger.LogTrace("Skipping system/primitive type: {TypeName}", typeSymbol.ToDisplayString());
                return false;
            }

            var compositionEdge = CreateCompositionEdge(
                containerClass, typeSymbol, memberName, accessType, memberNode);

            graph.CompositionRelationships.Add(compositionEdge);

            _logger.LogTrace("Found composition: {Container} contains {Contained} via {Member}",
                containerClass.Name, typeSymbol.Name, memberName);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error analyzing composition for member: {MemberName}", memberName);
            return false;
        }
    }

    /// <summary>
    /// Creates a composition edge from the analyzed components
    /// </summary>
    private CompositionEdge CreateCompositionEdge(ClassNode containerClass, ITypeSymbol typeSymbol, string memberName,
        CompositionAccessType accessType, SyntaxNode memberNode) 
    {
        var compositionEdge = new CompositionEdge
        {
            ContainerClassId = containerClass.Id,
            ContainedClassId = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            ContainedClassName = typeSymbol.ToDisplayString(),
            MemberName = memberName,
            AccessType = accessType,
            CompositionType = DetermineCompositionType(typeSymbol),
            Multiplicity = DetermineMultiplicity(typeSymbol),
            IsNullable = typeSymbol.CanBeReferencedByName && typeSymbol.NullableAnnotation == NullableAnnotation.Annotated,
            IsGenericType = typeSymbol is INamedTypeSymbol namedType && namedType.IsGenericType,
            CompositionLocation = _syntaxUtilities.GetLocationInfo(memberNode)
        };

        // Handle generic type arguments
        if (compositionEdge.IsGenericType && typeSymbol is INamedTypeSymbol genericType)
        {
            compositionEdge.GenericTypeArguments = genericType.TypeArguments
                .Select(ta => ta.ToDisplayString())
                .ToList();
        }

        return compositionEdge;
    }

    /// <summary>
    /// Determines if a type should be skipped from composition analysis
    /// </summary>
    private bool ShouldSkipType(ITypeSymbol typeSymbol)
    {
        // Skip primitive types
        if (typeSymbol.SpecialType != SpecialType.None)
            return true;

        // Skip system types
        var namespaceName = typeSymbol.ContainingNamespace?.ToDisplayString();
        return namespaceName?.StartsWith("System") == true;
    }

    /// <summary>
    /// Determines the type of composition relationship
    /// </summary>
    private CompositionType DetermineCompositionType(ITypeSymbol typeSymbol)
    {
        // This is a simplified heuristic - in practice, this could be more sophisticated
        var typeName = typeSymbol.ToDisplayString();

        // Collection types typically indicate aggregation rather than composition
        if (IsCollectionType(typeName))
        {
            _logger.LogTrace("Determined aggregation for collection type: {TypeName}", typeName);
            return CompositionType.Aggregation;
        }

        // Default to composition for single-instance relationships
        _logger.LogTrace("Determined composition for type: {TypeName}", typeName);
        return CompositionType.Composition;
    }

    /// <summary>
    /// Determines the multiplicity of the relationship
    /// </summary>
    private CompositionMultiplicity DetermineMultiplicity(ITypeSymbol typeSymbol)
    {
        var typeName = typeSymbol.ToDisplayString();

        // Collection types indicate many-to-many relationships
        if (IsCollectionType(typeName))
        {
            _logger.LogTrace("Determined many multiplicity for collection: {TypeName}", typeName);
            return CompositionMultiplicity.Many;
        }

        // Nullable types indicate optional relationships
        if (typeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
        {
            _logger.LogTrace("Determined zero-or-one multiplicity for nullable: {TypeName}", typeName);
            return CompositionMultiplicity.ZeroOrOne;
        }

        // Default to one-to-one relationship
        _logger.LogTrace("Determined one multiplicity for type: {TypeName}", typeName);
        return CompositionMultiplicity.One;
    }

    /// <summary>
    /// Determines if a type name represents a collection type
    /// </summary>
    private bool IsCollectionType(string typeName)
    {
        return typeName.Contains("List") || 
               typeName.Contains("Collection") || 
               typeName.Contains("Array") ||
               typeName.Contains("IEnumerable") ||
               typeName.Contains("HashSet") ||
               typeName.Contains("Dictionary");
    }
}