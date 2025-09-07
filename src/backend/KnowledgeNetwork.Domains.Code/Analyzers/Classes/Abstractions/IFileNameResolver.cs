using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Classes.Abstractions;

/// <summary>
/// Resolves effective file names using multiple fallback strategies
/// </summary>
public interface IFileNameResolver
{
    /// <summary>
    /// Resolves the effective file name using multiple fallback strategies
    /// </summary>
    /// <param name="compilationUnit">The compilation unit to resolve the name for</param>
    /// <param name="providedFileName">The provided file name (if any)</param>
    /// <returns>The effective file name</returns>
    string ResolveEffectiveFileName(CompilationUnitSyntax compilationUnit, string providedFileName);
}