using KnowledgeNetwork.Domains.Code.Analyzers.Classes.Abstractions;
using KnowledgeNetwork.Domains.Code.Analyzers.Classes.Utilities;
using KnowledgeNetwork.Domains.Code.Models.Classes;
using KnowledgeNetwork.Domains.Code.Models.Classes.ClassEnums;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Classes.RelationshipAnalyzers;

/// <summary>
/// Analyzes inheritance relationships between classes
/// </summary>
public class InheritanceAnalyzer(ILogger<InheritanceAnalyzer> logger, ISyntaxUtilities syntaxUtilities) : IInheritanceAnalyzer
{
    private readonly ILogger<InheritanceAnalyzer> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly ISyntaxUtilities _syntaxUtilities = syntaxUtilities ?? throw new ArgumentNullException(nameof(syntaxUtilities));

    /// <summary>
    /// Analyzes inheritance relationships within the provided type declarations
    /// </summary>
    public async Task AnalyzeAsync(SemanticModel semanticModel, ClassRelationshipGraph graph, List<BaseTypeDeclarationSyntax> typeDeclarations)
    {
        _logger.LogDebug("Starting inheritance relationship analysis for {TypeCount} types", typeDeclarations.Count);

        try
        {
            var inheritanceCount = 0;
            
            foreach (var typeDeclaration in typeDeclarations)
            {
                if (!(typeDeclaration.BaseList?.Types.Count > 0)) continue;
                
                var childSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration);
                if (childSymbol == null) 
                {
                    _logger.LogWarning("Could not get symbol for type declaration: {TypeName}", 
                        typeDeclaration.GetType().Name);
                    continue;
                }

                var childClass = graph.Classes.FirstOrDefault(c => 
                    c.FullName == childSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                if (childClass == null) 
                {
                    _logger.LogWarning("Could not find child class in graph: {ClassName}", childSymbol.Name);
                    continue;
                }

                // Find base class (first non-interface type in base list)
                foreach (var baseType in typeDeclaration.BaseList.Types)
                {
                    var baseSymbol = semanticModel.GetSymbolInfo(baseType.Type).Symbol as INamedTypeSymbol;
                    if (baseSymbol?.TypeKind != TypeKind.Class) continue;
                    
                    var inheritanceEdge = CreateInheritanceEdge(childClass, childSymbol, baseSymbol, baseType);
                    graph.InheritanceRelationships.Add(inheritanceEdge);
                    inheritanceCount++;
                            
                    _logger.LogTrace("Found inheritance: {Child} inherits from {Parent}", 
                        childSymbol.Name, baseSymbol.Name);
                            
                    break; // Only one base class in C#
                }
            }

            _logger.LogDebug("Completed inheritance analysis. Found {InheritanceCount} inheritance relationships", 
                inheritanceCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during inheritance relationship analysis");
            throw;
        }
    }

    /// <summary>
    /// Creates an inheritance edge from the analyzed symbols
    /// </summary>
    private InheritanceEdge CreateInheritanceEdge(ClassNode childClass, ISymbol childSymbol, INamedTypeSymbol baseSymbol,
        BaseTypeSyntax baseType)
    {
        return new InheritanceEdge
        {
            ChildClassId = childClass.Id,
            ParentClassId = baseSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            ParentClassName = baseSymbol.ToDisplayString(),
            InheritanceType = baseSymbol.IsAbstract ? InheritanceType.AbstractClass : InheritanceType.Class,
            IsCrossNamespace = childSymbol.ContainingNamespace?.ToDisplayString() != baseSymbol.ContainingNamespace?.ToDisplayString(),
            IsCrossAssembly = !SymbolEqualityComparer.Default.Equals(childSymbol.ContainingAssembly, baseSymbol.ContainingAssembly),
            HierarchyLevel = 0, // TODO: Could be calculated if needed
            InheritanceLocation = _syntaxUtilities.GetLocationInfo(baseType)
        };
    }
}