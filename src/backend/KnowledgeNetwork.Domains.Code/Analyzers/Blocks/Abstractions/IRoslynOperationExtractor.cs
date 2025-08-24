using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Blocks.Abstractions;

/// <summary>
/// Service responsible for extracting IBlockOperation from Roslyn syntax nodes.
/// This service handles the complexity of converting various syntax structures
/// into the specific operation type required by ControlFlowGraph.Create().
/// </summary>
public interface IRoslynOperationExtractor
{
    /// <summary>
    /// Extract IBlockOperation from a method declaration
    /// </summary>
    /// <param name="compilation">Compilation context for semantic analysis</param>
    /// <param name="methodDeclaration">Method syntax node</param>
    /// <returns>IBlockOperation if extraction succeeds, null otherwise</returns>
    Task<IBlockOperation?> ExtractFromMethodAsync(
        Compilation compilation, 
        MethodDeclarationSyntax methodDeclaration);
    
    /// <summary>
    /// Extract IBlockOperation from a constructor declaration
    /// </summary>
    /// <param name="compilation">Compilation context for semantic analysis</param>
    /// <param name="constructorDeclaration">Constructor syntax node</param>
    /// <returns>IBlockOperation if extraction succeeds, null otherwise</returns>
    Task<IBlockOperation?> ExtractFromConstructorAsync(
        Compilation compilation, 
        ConstructorDeclarationSyntax constructorDeclaration);

    /// <summary>
    /// Extract IBlockOperation from any syntax node representing a method body
    /// </summary>
    /// <param name="compilation">Compilation context for semantic analysis</param>
    /// <param name="bodyNode">Syntax node representing the method/constructor body</param>
    /// <param name="memberName">Name of the member for logging purposes</param>
    /// <returns>IBlockOperation if extraction succeeds, null otherwise</returns>
    Task<IBlockOperation?> ExtractFromBodyAsync(
        Compilation compilation, 
        SyntaxNode bodyNode,
        string memberName);
}