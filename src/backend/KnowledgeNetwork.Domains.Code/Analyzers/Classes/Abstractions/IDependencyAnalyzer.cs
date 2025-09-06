using KnowledgeNetwork.Domains.Code.Models.Classes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Classes.Abstractions;

/// <summary>
/// Analyzes dependency relationships (usage without ownership)
/// </summary>
public interface IDependencyAnalyzer
{
    /// <summary>
    /// Analyzes dependency relationships within the provided type declarations
    /// </summary>
    Task AnalyzeAsync(
        SemanticModel semanticModel, 
        ClassRelationshipGraph graph, 
        List<BaseTypeDeclarationSyntax> typeDeclarations);
}