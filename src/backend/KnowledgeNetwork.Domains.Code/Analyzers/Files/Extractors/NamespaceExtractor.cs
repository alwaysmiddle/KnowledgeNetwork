using KnowledgeNetwork.Domains.Code.Analyzers.Files.Abstractions;
using KnowledgeNetwork.Domains.Code.Models.Files;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Files.Extractors;

/// <summary>
/// Extracts declared namespaces from C# syntax trees
/// </summary>
public class NamespaceExtractor : INamespaceExtractor
{
    /// <summary>
    /// Extracts declared namespaces from a syntax tree root and populates the file node
    /// </summary>
    public void ExtractDeclaredNamespaces(FileNode fileNode, SyntaxNode root)
    {
        // File-scoped namespaces (C# 10+)
        var fileScopedNamespaces = root.DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>();
        foreach (var ns in fileScopedNamespaces)
        {
            var namespaceName = ns.Name.ToString();
            if (!fileNode.DeclaredNamespaces.Contains(namespaceName))
            {
                fileNode.DeclaredNamespaces.Add(namespaceName);
            }
        }

        // Traditional block-scoped namespaces
        var blockScopedNamespaces = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>();
        foreach (var ns in blockScopedNamespaces)
        {
            var namespaceName = ns.Name.ToString();
            if (!fileNode.DeclaredNamespaces.Contains(namespaceName))
            {
                fileNode.DeclaredNamespaces.Add(namespaceName);
            }
        }
    }
}