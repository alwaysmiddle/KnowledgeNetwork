using KnowledgeNetwork.Domains.Code.Analyzers.Files.Abstractions;
using KnowledgeNetwork.Domains.Code.Models.Common;
using Microsoft.CodeAnalysis;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Files.Utilities;

/// <summary>
/// Provides common syntax operations and utilities for file analysis
/// </summary>
public class FileSyntaxUtilities : IFileSyntaxUtilities
{
    /// <summary>
    /// Gets location information from a syntax node
    /// </summary>
    public CSharpLocationInfo GetLocationInfo(SyntaxNode? node)
    {
        if (node == null) return new CSharpLocationInfo();

        var location = node.GetLocation();
        var span = location.GetLineSpan();

        return new CSharpLocationInfo
        {
            FilePath = span.Path ?? "",
            StartLine = span.StartLinePosition.Line + 1,
            StartColumn = span.StartLinePosition.Character + 1,
            EndLine = span.EndLinePosition.Line + 1,
            EndColumn = span.EndLinePosition.Character + 1
        };
    }
}