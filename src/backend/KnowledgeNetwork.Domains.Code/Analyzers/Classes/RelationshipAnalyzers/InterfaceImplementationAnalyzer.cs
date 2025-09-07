using KnowledgeNetwork.Domains.Code.Analyzers.Classes.Abstractions;
using KnowledgeNetwork.Domains.Code.Analyzers.Classes.Utilities;
using KnowledgeNetwork.Domains.Code.Models.Classes;
using KnowledgeNetwork.Domains.Code.Models.Classes.ClassEnums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Classes.RelationshipAnalyzers;

/// <summary>
/// Analyzes interface implementation relationships
/// </summary>
public class InterfaceImplementationAnalyzer(
    ILogger<InterfaceImplementationAnalyzer> logger,
    ISyntaxUtilities syntaxUtilities) : IInterfaceImplementationAnalyzer
{
    private readonly ILogger<InterfaceImplementationAnalyzer> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ISyntaxUtilities _syntaxUtilities = syntaxUtilities ?? throw new ArgumentNullException(nameof(syntaxUtilities));

    /// <summary>
    /// Analyzes interface implementations within the provided type declarations
    /// </summary>
    public async Task AnalyzeAsync(
        SemanticModel semanticModel, 
        ClassRelationshipGraph graph, 
        List<BaseTypeDeclarationSyntax> typeDeclarations)
    {
        _logger.LogDebug("Starting interface implementation analysis for {TypeCount} types", typeDeclarations.Count);

        try
        {
            var implementationCount = 0;

            foreach (var typeDeclaration in typeDeclarations)
            {
                if (!(typeDeclaration.BaseList?.Types.Count > 0)) continue;
                
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

                // Find interfaces in base list
                foreach (var baseType in typeDeclaration.BaseList.Types)
                {
                    var baseSymbol = semanticModel.GetSymbolInfo(baseType.Type).Symbol as INamedTypeSymbol;
                    if (baseSymbol?.TypeKind != TypeKind.Interface) continue;
                        
                    var implementationEdge = CreateImplementationEdge(
                        classNode, classSymbol, baseSymbol, baseType, typeDeclaration, semanticModel);
                            
                    graph.InterfaceImplementations.Add(implementationEdge);
                    implementationCount++;

                    _logger.LogTrace("Found interface implementation: {Class} implements {Interface}",
                        classSymbol.Name, baseSymbol.Name);
                }
            }

            _logger.LogDebug("Completed interface implementation analysis. Found {ImplementationCount} implementations",
                implementationCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during interface implementation analysis");
            throw;
        }
    }

    /// <summary>
    /// Creates an interface implementation edge from the analyzed symbols
    /// </summary>
    private InterfaceImplementationEdge CreateImplementationEdge(
        ClassNode classNode,
        ISymbol classSymbol,
        INamedTypeSymbol interfaceSymbol,
        BaseTypeSyntax baseType,
        BaseTypeDeclarationSyntax classDeclaration,
        SemanticModel semanticModel)
    {
        var implementationEdge = new InterfaceImplementationEdge
        {
            ClassId = classNode.Id,
            InterfaceName = interfaceSymbol.Name,
            InterfaceFullName = interfaceSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            ImplementationType = InterfaceImplementationType.Direct,
            IsGenericInterface = interfaceSymbol.IsGenericType,
            IsCrossNamespace = classSymbol.ContainingNamespace?.ToDisplayString() != interfaceSymbol.ContainingNamespace?.ToDisplayString(),
            IsCrossAssembly = !SymbolEqualityComparer.Default.Equals(classSymbol.ContainingAssembly, interfaceSymbol.ContainingAssembly),
            ImplementationLocation = _syntaxUtilities.GetLocationInfo(baseType)
        };

        // Handle generic type arguments
        if (interfaceSymbol.IsGenericType)
        {
            implementationEdge.GenericTypeArguments = interfaceSymbol.TypeArguments
                .Select(ta => ta.ToDisplayString())
                .ToList();
        }

        // Extract implemented methods
        ExtractInterfaceMethodImplementations(implementationEdge, interfaceSymbol, classDeclaration, semanticModel);

        return implementationEdge;
    }

    /// <summary>
    /// Extracts interface method implementations
    /// </summary>
    private void ExtractInterfaceMethodImplementations(
        InterfaceImplementationEdge edge, 
        INamedTypeSymbol interfaceSymbol, 
        BaseTypeDeclarationSyntax classDeclaration, 
        SemanticModel semanticModel)
    {
        try
        {
            var interfaceMethods = interfaceSymbol.GetMembers().OfType<IMethodSymbol>().ToList();
            var classMethods = _syntaxUtilities.GetMethods(classDeclaration).ToList();

            _logger.LogTrace("Analyzing {InterfaceMethodCount} interface methods against {ClassMethodCount} class methods",
                interfaceMethods.Count, classMethods.Count);

            foreach (var interfaceMethod in interfaceMethods)
            {
                var implementation = new InterfaceMethodImplementation
                {
                    MethodName = interfaceMethod.Name,
                    MethodSignature = interfaceMethod.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)
                };

                // Find implementing method in class
                var implementingMethod = FindImplementingMethod(classMethods, interfaceMethod);

                if (implementingMethod != null)
                {
                    implementation.ImplementingMethodName = implementingMethod.Identifier.ValueText;
                    implementation.ImplementationLocation = _syntaxUtilities.GetLocationInfo(implementingMethod);
                    implementation.IsExplicit = implementingMethod.ExplicitInterfaceSpecifier != null;
                }
                else
                {
                    _logger.LogTrace("No implementation found for interface method: {MethodName}", interfaceMethod.Name);
                }

                edge.ImplementedMethods.Add(implementation);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting interface method implementations for interface: {InterfaceName}", 
                interfaceSymbol.Name);
        }
    }

    /// <summary>
    /// Finds the implementing method in the class for a given interface method
    /// </summary>
    private MethodDeclarationSyntax? FindImplementingMethod(
        List<MethodDeclarationSyntax> classMethods,
        IMethodSymbol interfaceMethod)
    {
        // Simple name-based matching - could be enhanced to include signature matching
        return classMethods.FirstOrDefault(m => 
            m.Identifier.ValueText == interfaceMethod.Name);
    }
}