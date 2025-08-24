using KnowledgeNetwork.Domains.Code.Models.Blocks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Blocks;

public interface ICSharpMethodBlockAnalyzer
{
    /// <summary>
    /// Extract control flow graph from a method body
    /// </summary>
    /// <param name="compilation">Compilation context</param>
    /// <param name="methodDeclaration">Method syntax node</param>
    /// <returns>Control flow graph or null if extraction fails</returns>
    Task<MethodBlockGraph?> ExtractControlFlowAsync(
        Compilation compilation,
        MethodDeclarationSyntax methodDeclaration);

    /// <summary>
    /// Extract control flow graphs for all methods in a syntax tree
    /// </summary>
    /// <param name="compilation">Compilation context</param>
    /// <param name="syntaxTree">Syntax tree to analyze</param>
    /// <returns>List of control flow graphs</returns>
    Task<List<MethodBlockGraph>> ExtractAllControlFlowsAsync(
        Compilation compilation,
        SyntaxTree syntaxTree);
}