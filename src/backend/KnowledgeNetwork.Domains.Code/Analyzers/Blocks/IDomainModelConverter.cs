using KnowledgeNetwork.Domains.Code.Models.Blocks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Blocks;

/// <summary>
/// Service responsible for converting Roslyn ControlFlowGraph structures into 
/// domain-specific MethodBlockGraph models. This service encapsulates all the
/// complexity of mapping between Roslyn's representation and our business domain.
/// </summary>
public interface IDomainModelConverter
{
    /// <summary>
    /// Convert a Roslyn ControlFlowGraph to our domain model MethodBlockGraph
    /// </summary>
    /// <param name="cfg">The Roslyn ControlFlowGraph to convert</param>
    /// <param name="syntaxNode">The original syntax node (method or constructor)</param>
    /// <param name="methodSymbol">The symbol representing the method/constructor</param>
    /// <returns>Converted MethodBlockGraph with all metadata and relationships</returns>
    Task<MethodBlockGraph> ConvertToDomainModelAsync(
        ControlFlowGraph cfg, 
        SyntaxNode syntaxNode, 
        ISymbol methodSymbol);

    /// <summary>
    /// Convert a Roslyn ControlFlowGraph for a constructor with special naming
    /// </summary>
    /// <param name="cfg">The Roslyn ControlFlowGraph to convert</param>
    /// <param name="constructorDeclaration">The constructor syntax node</param>
    /// <param name="constructorSymbol">The constructor symbol</param>
    /// <returns>Converted MethodBlockGraph with constructor-specific metadata</returns>
    Task<MethodBlockGraph> ConvertConstructorToDomainModelAsync(
        ControlFlowGraph cfg,
        ConstructorDeclarationSyntax constructorDeclaration,
        ISymbol constructorSymbol);
}