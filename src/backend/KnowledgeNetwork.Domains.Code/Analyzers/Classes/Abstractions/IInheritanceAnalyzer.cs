using KnowledgeNetwork.Domains.Code.Models.Classes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Classes.Abstractions;

/// <summary>
/// Analyzes inheritance relationships between classes
/// </summary>
public interface IInheritanceAnalyzer
{
    /// <summary>
    /// Analyzes inheritance relationships within the provided type declarations
    /// </summary>
    Task AnalyzeAsync(
        SemanticModel semanticModel, 
        ClassRelationshipGraph graph, 
        List<BaseTypeDeclarationSyntax> typeDeclarations);
}