using KnowledgeNetwork.Domains.Code.Models.Classes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Classes.Abstractions;

/// <summary>
/// Analyzes interface implementation relationships
/// </summary>
public interface IInterfaceImplementationAnalyzer
{
    /// <summary>
    /// Analyzes interface implementations within the provided type declarations
    /// </summary>
    Task AnalyzeAsync(
        SemanticModel semanticModel, 
        ClassRelationshipGraph graph, 
        List<BaseTypeDeclarationSyntax> typeDeclarations);
}