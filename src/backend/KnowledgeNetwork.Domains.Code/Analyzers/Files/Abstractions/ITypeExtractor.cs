using KnowledgeNetwork.Domains.Code.Models.Files;
using Microsoft.CodeAnalysis;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Files.Abstractions;

/// <summary>
/// Extracts declared and referenced types from C# syntax trees
/// </summary>
public interface ITypeExtractor
{
    /// <summary>
    /// Extracts declared types from a syntax tree and populates the file node
    /// </summary>
    /// <param name="fileNode">The file node to populate with declared types</param>
    /// <param name="root">The syntax tree root to extract from</param>
    /// <param name="semanticModel">The semantic model for type symbol resolution</param>
    Task ExtractDeclaredTypesAsync(FileNode fileNode, SyntaxNode root, SemanticModel semanticModel);

    /// <summary>
    /// Extracts referenced types from a syntax tree and populates the file node
    /// </summary>
    /// <param name="fileNode">The file node to populate with referenced types</param>
    /// <param name="root">The syntax tree root to extract from</param>
    /// <param name="semanticModel">The semantic model for type symbol resolution</param>
    Task ExtractReferencedTypesAsync(FileNode fileNode, SyntaxNode root, SemanticModel semanticModel);
}