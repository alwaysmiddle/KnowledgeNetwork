using KnowledgeNetwork.Domains.Code.Models.Files;
using Microsoft.CodeAnalysis;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Files.Abstractions;

/// <summary>
/// Extracts declared namespaces from C# syntax trees
/// </summary>
public interface INamespaceExtractor
{
    /// <summary>
    /// Extracts declared namespaces from a syntax tree root and populates the file node
    /// </summary>
    /// <param name="fileNode">The file node to populate with declared namespaces</param>
    /// <param name="root">The syntax tree root to extract from</param>
    void ExtractDeclaredNamespaces(FileNode fileNode, SyntaxNode root);
}