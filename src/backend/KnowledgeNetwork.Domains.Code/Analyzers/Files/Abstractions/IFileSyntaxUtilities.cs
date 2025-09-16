using KnowledgeNetwork.Domains.Code.Models.Common;
using Microsoft.CodeAnalysis;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Files.Abstractions;

/// <summary>
/// Provides common syntax operations and utilities for file analysis
/// </summary>
public interface IFileSyntaxUtilities
{
    /// <summary>
    /// Gets location information from a syntax node
    /// </summary>
    /// <param name="node">The syntax node to get location for</param>
    /// <returns>Location information including line and column positions</returns>
    CSharpLocationInfo GetLocationInfo(SyntaxNode? node);
}