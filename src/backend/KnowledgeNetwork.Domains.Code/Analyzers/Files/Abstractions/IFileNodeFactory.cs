using KnowledgeNetwork.Domains.Code.Models.Files;
using Microsoft.CodeAnalysis;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Files.Abstractions;

/// <summary>
/// Factory for creating FileNode instances with complete metadata and content analysis
/// </summary>
public interface IFileNodeFactory
{
    /// <summary>
    /// Creates a complete file node from a syntax tree with all metadata and content extracted
    /// </summary>
    /// <param name="compilation">The compilation containing the syntax tree</param>
    /// <param name="syntaxTree">The syntax tree to analyze</param>
    /// <param name="projectName">The name of the project containing this file</param>
    /// <returns>A complete FileNode or null if creation failed</returns>
    Task<FileNode?> CreateFileNodeAsync(Compilation compilation, SyntaxTree syntaxTree, string projectName);
}