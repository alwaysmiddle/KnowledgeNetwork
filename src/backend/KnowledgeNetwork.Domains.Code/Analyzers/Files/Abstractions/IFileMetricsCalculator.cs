using KnowledgeNetwork.Domains.Code.Models.Files;
using Microsoft.CodeAnalysis;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Files.Abstractions;

/// <summary>
/// Calculates various metrics for C# files
/// </summary>
public interface IFileMetricsCalculator
{
    /// <summary>
    /// Calculates comprehensive metrics for a file node including complexity, maintainability, and technical debt
    /// </summary>
    /// <param name="fileNode">The file node to calculate metrics for</param>
    /// <param name="root">The syntax tree root node</param>
    /// <returns>Complete file metrics</returns>
    FileMetrics CalculateFileMetrics(FileNode fileNode, SyntaxNode root);
}