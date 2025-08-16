using KnowledgeNetwork.Domains.Code.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KnowledgeNetwork.Domains.Code.Services;

public interface ICSharpControlFlowAnalyzer
{
    /// <summary>
    /// Extract control flow graph from a method body
    /// </summary>
    /// <param name="compilation">Compilation context</param>
    /// <param name="methodDeclaration">Method syntax node</param>
    /// <returns>Control flow graph or null if extraction fails</returns>
    Task<CSharpControlFlowGraph?> ExtractControlFlowAsync(
        Compilation compilation, 
        MethodDeclarationSyntax methodDeclaration);

    /// <summary>
    /// Extract control flow graphs for all methods in a syntax tree
    /// </summary>
    /// <param name="compilation">Compilation context</param>
    /// <param name="syntaxTree">Syntax tree to analyze</param>
    /// <returns>List of control flow graphs</returns>
    Task<List<CSharpControlFlowGraph>> ExtractAllControlFlowsAsync(
        Compilation compilation,
        SyntaxTree syntaxTree);
}