using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Blocks.Abstractions;

/// <summary>
/// Service responsible for extracting ControlFlowGraph directly from C# method and constructor declarations.
/// This service combines operation extraction and CFG creation in one cohesive interface.
/// </summary>
public interface IRoslynCfgExtractor
{
    /// <summary>
    /// Extract ControlFlowGraph from a method declaration
    /// </summary>
    /// <param name="compilation">Compilation context</param>
    /// <param name="methodDeclaration">Method syntax node</param>
    /// <returns>ControlFlowGraph or null if extraction fails</returns>
    Task<ControlFlowGraph?> ExtractControlFlowGraphAsync(
        Compilation compilation,
        MethodDeclarationSyntax methodDeclaration);

    /// <summary>
    /// Extract ControlFlowGraph from a constructor declaration
    /// </summary>
    /// <param name="compilation">Compilation context</param>
    /// <param name="constructorDeclaration">Constructor syntax node</param>
    /// <returns>ControlFlowGraph or null if extraction fails</returns>
    Task<ControlFlowGraph?> ExtractControlFlowGraphAsync(
        Compilation compilation,
        ConstructorDeclarationSyntax constructorDeclaration);
}