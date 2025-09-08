using KnowledgeNetwork.Domains.Code.Analyzers.Files.Abstractions;
using KnowledgeNetwork.Domains.Code.Models.Files;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Files.Extractors;

/// <summary>
/// Extracts using directives from C# syntax trees
/// </summary>
public class UsingDirectiveExtractor : IUsingDirectiveExtractor
{
    /// <summary>
    /// Extracts using directives from a syntax tree root and populates the file node
    /// </summary>
    public void ExtractUsingDirectives(FileNode fileNode, SyntaxNode root)
    {
        var usingDirectives = root.DescendantNodes().OfType<UsingDirectiveSyntax>().ToList();
        
        foreach (var usingDirective in usingDirectives)
        {
            var nameText = usingDirective.Name?.ToString() ?? string.Empty;
            if (!string.IsNullOrEmpty(nameText))
            {
                if (usingDirective.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword))
                {
                    fileNode.GlobalUsings.Add(nameText);
                }
                else
                {
                    fileNode.UsingDirectives.Add(nameText);
                }
            }
        }
    }
}