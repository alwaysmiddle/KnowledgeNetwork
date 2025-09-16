using KnowledgeNetwork.Domains.Code.Models.Files;
using Microsoft.CodeAnalysis;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Files.Abstractions;

/// <summary>
/// Extracts using directives from C# syntax trees
/// </summary>
public interface IUsingDirectiveExtractor
{
    /// <summary>
    /// Extracts using directives from a syntax tree root and populates the file node
    /// </summary>
    /// <param name="fileNode">The file node to populate with using directives</param>
    /// <param name="root">The syntax tree root to extract from</param>
    void ExtractUsingDirectives(FileNode fileNode, SyntaxNode root);
}