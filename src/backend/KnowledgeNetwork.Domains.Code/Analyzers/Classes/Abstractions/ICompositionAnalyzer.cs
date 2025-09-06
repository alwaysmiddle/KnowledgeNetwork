using KnowledgeNetwork.Domains.Code.Models.Classes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Classes.Abstractions;

/// <summary>
/// Analyzes composition relationships (has-a relationships through fields/properties)
/// </summary>
public interface ICompositionAnalyzer
{
    /// <summary>
    /// Analyzes composition relationships within the provided type declarations
    /// </summary>
    Task AnalyzeAsync(
        SemanticModel semanticModel, 
        ClassRelationshipGraph graph, 
        List<BaseTypeDeclarationSyntax> typeDeclarations);
}