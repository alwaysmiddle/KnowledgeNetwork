using KnowledgeNetwork.Domains.Code.Analyzers.Classes.Abstractions;
using KnowledgeNetwork.Domains.Code.Models.Classes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Classes.RelationshipAnalyzers;

/// <summary>
/// Analyzes dependency relationships (usage without ownership)
/// </summary>
public class DependencyAnalyzer(ILogger<DependencyAnalyzer> logger, ISyntaxUtilities syntaxUtilities) : IDependencyAnalyzer
{
    private readonly ILogger<DependencyAnalyzer> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ISyntaxUtilities _syntaxUtilities = syntaxUtilities ?? throw new ArgumentNullException(nameof(syntaxUtilities));

    /// <summary>
    /// Analyzes dependency relationships within the provided type declarations
    /// </summary>
    public async Task AnalyzeAsync(SemanticModel semanticModel, ClassRelationshipGraph graph, List<BaseTypeDeclarationSyntax> typeDeclarations)
    {
        _logger.LogDebug("Starting dependency relationship analysis for {TypeCount} types", typeDeclarations.Count);

        try
        {
            var dependencyCount = 0;

            foreach (var typeDeclaration in typeDeclarations)
            {
                var classSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration);
                if (classSymbol == null)
                {
                    _logger.LogWarning("Could not get symbol for type declaration: {TypeName}", 
                        typeDeclaration.GetType().Name);
                    continue;
                }

                var classNode = graph.Classes.FirstOrDefault(c => 
                    c.FullName == classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                if (classNode == null)
                {
                    _logger.LogWarning("Could not find class in graph: {ClassName}", classSymbol.Name);
                    continue;
                }

                // Analyze method parameters and return types
                var methods = _syntaxUtilities.GetMethods(typeDeclaration);
                dependencyCount += methods.Sum(method => AnalyzeDependenciesInMethod(method, semanticModel, graph, classNode));

                // Analyze local variables and method calls
                var allNodes = typeDeclaration.DescendantNodes();
                dependencyCount += AnalyzeDependenciesInNodes(allNodes, semanticModel, graph, classNode);
            }

            _logger.LogDebug("Completed dependency analysis. Found {DependencyCount} dependency relationships", 
                dependencyCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during dependency relationship analysis");
            throw;
        }
    }

    /// <summary>
    /// Analyzes dependencies in a method
    /// </summary>
    private int AnalyzeDependenciesInMethod(MethodDeclarationSyntax method, SemanticModel semanticModel, ClassRelationshipGraph graph, 
        ClassNode classNode)
    {
        var dependencyCount = 0;

        try
        {
            // Return type dependency
            if (method.ReturnType != null)
            {
                if (AnalyzeDependencyFromType(method.ReturnType, semanticModel, graph, classNode, 
                    ClassDependencyType.ReturnType, method))
                {
                    dependencyCount++;
                }
            }

            // Parameter dependencies
            foreach (var parameter in method.ParameterList.Parameters)
            {
                if (parameter.Type == null) continue;
                if (AnalyzeDependencyFromType(parameter.Type, semanticModel, graph, classNode, 
                        ClassDependencyType.Parameter, parameter))
                {
                    dependencyCount++;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error analyzing dependencies in method: {MethodName}", method.Identifier.ValueText);
        }

        return dependencyCount;
    }

    /// <summary>
    /// Analyzes dependencies in syntax nodes
    /// </summary>
    private int AnalyzeDependenciesInNodes(IEnumerable<SyntaxNode> nodes, SemanticModel semanticModel, ClassRelationshipGraph graph, 
        ClassNode classNode)
    {
        var dependencyCount = 0;

        try
        {
            foreach (var node in nodes)
            {
                switch (node)
                {
                    case VariableDeclarationSyntax variable:
                        if (AnalyzeDependencyFromType(variable.Type, semanticModel, graph, classNode, 
                            ClassDependencyType.LocalVariable, variable))
                        {
                            dependencyCount++;
                        }
                        break;

                    case InvocationExpressionSyntax invocation:
                        if (AnalyzeDependencyFromInvocation(invocation, semanticModel, graph, classNode))
                        {
                            dependencyCount++;
                        }
                        break;

                    case CastExpressionSyntax cast:
                        if (AnalyzeDependencyFromType(cast.Type, semanticModel, graph, classNode, 
                            ClassDependencyType.Usage, cast))
                        {
                            dependencyCount++;
                        }
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error analyzing dependencies in nodes for class: {ClassName}", classNode.Name);
        }

        return dependencyCount;
    }

    /// <summary>
    /// Analyzes dependency from a type reference
    /// </summary>
    private bool AnalyzeDependencyFromType(TypeSyntax typeSyntax, SemanticModel semanticModel, ClassRelationshipGraph graph, 
        ClassNode classNode, ClassDependencyType dependencyType, SyntaxNode location)
    {
        try
        {
            if (semanticModel.GetSymbolInfo(typeSyntax).Symbol is not ITypeSymbol typeSymbol)
            {
                _logger.LogTrace("Could not resolve type symbol for dependency analysis");
                return false;
            }

            // Skip primitive types, system types
            if (ShouldSkipType(typeSymbol))
            {
                _logger.LogTrace("Skipping system/primitive type: {TypeName}", typeSymbol.ToDisplayString());
                return false;
            }

            var typeFullName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            // Check if this is already a composition relationship
            if (!HasExistingComposition(graph, classNode, typeFullName))
            {
                return CreateOrUpdateDependencyEdge(graph, classNode, typeSymbol, typeFullName, dependencyType,
                    location);
            }
                
            _logger.LogTrace("Skipping dependency - already has composition relationship: {TypeName}", typeSymbol.Name);
            return false;

        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error analyzing dependency from type");
            return false;
        }
    }

    /// <summary>
    /// Analyzes dependency from method invocation
    /// </summary>
    private bool AnalyzeDependencyFromInvocation(InvocationExpressionSyntax invocation, SemanticModel semanticModel, 
        ClassRelationshipGraph graph, ClassNode classNode)
    {
        try
        {
            var symbolInfo = semanticModel.GetSymbolInfo(invocation);
            if (symbolInfo.Symbol is not IMethodSymbol methodSymbol) return false;
            
            var containingType = methodSymbol.ContainingType;
            if (containingType is not { SpecialType: SpecialType.None }) return false;
                
            var typeFullName = containingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            // Skip if this is the same class
            if (typeFullName == classNode.FullName)
            {
                _logger.LogTrace("Skipping self-reference in method invocation");
                return false;
            }

            // Create a synthetic type syntax for the containing type
            var typeSyntax = SyntaxFactory.IdentifierName(containingType.Name);
            var dependencyType = methodSymbol.IsStatic ? ClassDependencyType.StaticCall : ClassDependencyType.Usage;

            return AnalyzeDependencyFromType(typeSyntax, semanticModel, graph, classNode, dependencyType, invocation);

        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error analyzing dependency from invocation");
            return false;
        }
    }

    /// <summary>
    /// Creates or updates a dependency edge
    /// </summary>
    private bool CreateOrUpdateDependencyEdge(ClassRelationshipGraph graph, ClassNode classNode, ITypeSymbol typeSymbol,
        string typeFullName, ClassDependencyType dependencyType, SyntaxNode location)
    {
        // Find existing dependency edge
        var existingDependency = graph.DependencyRelationships.FirstOrDefault(d =>
            d.SourceClassId == classNode.Id &&
            d.TargetClassId == typeFullName);

        if (existingDependency != null)
        {
            // Update existing dependency
            existingDependency.ReferenceCount++;
            existingDependency.UsageLocations.Add(_syntaxUtilities.GetLocationInfo(location));

            var usageType = GetDependencyUsage(dependencyType);
            if (!existingDependency.UsageTypes.Contains(usageType))
            {
                existingDependency.UsageTypes.Add(usageType);
            }

            _logger.LogTrace("Updated existing dependency: {Source} -> {Target} (count: {Count})",
                classNode.Name, typeSymbol.Name, existingDependency.ReferenceCount);

            return false; // Didn't create new dependency
        }
        else
        {
            // Create new dependency edge
            var dependencyEdge = new ClassDependencyEdge
            {
                SourceClassId = classNode.Id,
                TargetClassId = typeFullName,
                TargetClassName = typeSymbol.ToDisplayString(),
                DependencyType = dependencyType,
                UsageTypes = [GetDependencyUsage(dependencyType)],
                Strength = DetermineDependencyStrength(dependencyType),
                ReferenceCount = 1,
                IsCrossNamespace = classNode.Namespace != typeSymbol.ContainingNamespace?.ToDisplayString(),
                IsGenericTarget = typeSymbol is INamedTypeSymbol namedType && namedType.IsGenericType,
                UsageLocations = [_syntaxUtilities.GetLocationInfo(location)]
            };

            // Handle generic type arguments
            if (dependencyEdge.IsGenericTarget && typeSymbol is INamedTypeSymbol genericType)
            {
                dependencyEdge.GenericTypeArguments = genericType.TypeArguments
                    .Select(ta => ta.ToDisplayString())
                    .ToList();
            }

            graph.DependencyRelationships.Add(dependencyEdge);

            _logger.LogTrace("Created new dependency: {Source} -> {Target} ({Type})",
                classNode.Name, typeSymbol.Name, dependencyType);

            return true; // Created new dependency
        }
    }

    /// <summary>
    /// Determines if a type should be skipped from dependency analysis
    /// </summary>
    private bool ShouldSkipType(ITypeSymbol typeSymbol)
    {
        // Skip primitive types
        if (typeSymbol.SpecialType != SpecialType.None)
            return true;

        // Skip system types
        var namespaceName = typeSymbol.ContainingNamespace?.ToDisplayString();
        if (namespaceName?.StartsWith("System") == true)
            return true;

        return false;
    }

    /// <summary>
    /// Checks if there's already a composition relationship
    /// </summary>
    private static bool HasExistingComposition(ClassRelationshipGraph graph, ClassNode classNode, string typeFullName)
    {
        return graph.CompositionRelationships.Any(c =>
            c.ContainerClassId == classNode.Id &&
            c.ContainedClassId == typeFullName);
    }

    /// <summary>
    /// Maps dependency type to usage type
    /// </summary>
    private DependencyUsage GetDependencyUsage(ClassDependencyType dependencyType)
    {
        return dependencyType switch
        {
            ClassDependencyType.Parameter => DependencyUsage.MethodParameter,
            ClassDependencyType.ReturnType => DependencyUsage.ReturnType,
            ClassDependencyType.LocalVariable => DependencyUsage.LocalVariable,
            ClassDependencyType.StaticCall => DependencyUsage.StaticMethodCall,
            _ => DependencyUsage.LocalVariable
        };
    }

    /// <summary>
    /// Determines dependency strength based on usage type
    /// </summary>
    private DependencyStrength DetermineDependencyStrength(ClassDependencyType dependencyType)
    {
        return dependencyType switch
        {
            ClassDependencyType.Parameter => DependencyStrength.Moderate,
            ClassDependencyType.ReturnType => DependencyStrength.Strong,
            ClassDependencyType.LocalVariable => DependencyStrength.Weak,
            ClassDependencyType.StaticCall => DependencyStrength.Weak,
            _ => DependencyStrength.Weak
        };
    }
}