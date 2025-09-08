using KnowledgeNetwork.Domains.Code.Analyzers.Files.Abstractions;
using KnowledgeNetwork.Domains.Code.Models.Files;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Files.Utilities;

/// <summary>
/// Calculates various metrics for C# files
/// </summary>
public class FileMetricsCalculator : IFileMetricsCalculator
{
    /// <summary>
    /// Calculates comprehensive metrics for a file node including complexity, maintainability, and technical debt
    /// </summary>
    public FileMetrics CalculateFileMetrics(FileNode fileNode, SyntaxNode root)
    {
        var text = root.SyntaxTree.GetText();
        var lines = text.Lines;

        var metrics = new FileMetrics
        {
            TotalLines = lines.Count,
            UsingDirectiveCount = fileNode.UsingDirectives.Count + fileNode.GlobalUsings.Count,
            DeclaredTypeCount = fileNode.DeclaredTypes.Count,
            ReferencedTypeCount = fileNode.ReferencedTypes.Count
        };

        // Calculate lines of code, comments, and blank lines
        foreach (var line in lines)
        {
            var lineText = line.ToString().Trim();
            
            if (string.IsNullOrEmpty(lineText))
            {
                metrics.BlankLines++;
            }
            else if (lineText.StartsWith("//") || lineText.StartsWith("/*") || lineText.StartsWith("*"))
            {
                metrics.CommentLines++;
            }
            else
            {
                metrics.LinesOfCode++;
            }
        }

        // Simple complexity calculation based on control flow statements
        var complexityNodes = root.DescendantNodes().Where(n => 
            n.IsKind(SyntaxKind.IfStatement) ||
            n.IsKind(SyntaxKind.WhileStatement) ||
            n.IsKind(SyntaxKind.ForStatement) ||
            n.IsKind(SyntaxKind.ForEachStatement) ||
            n.IsKind(SyntaxKind.SwitchStatement) ||
            n.IsKind(SyntaxKind.TryStatement));

        metrics.CyclomaticComplexity = complexityNodes.Count() + 1; // +1 for base complexity

        // Calculate maintainability index (simplified)
        if (metrics.LinesOfCode > 0)
        {
            var commentRatio = (double)metrics.CommentLines / metrics.TotalLines;
            var complexityRatio = (double)metrics.CyclomaticComplexity / metrics.LinesOfCode;
            
            metrics.MaintainabilityIndex = Math.Max(0, 
                100 - (metrics.LinesOfCode / 10.0) - (metrics.CyclomaticComplexity * 2) + (commentRatio * 20));
            
            metrics.TechnicalDebtRatio = Math.Min(1.0, complexityRatio * 0.5 + (1.0 - commentRatio) * 0.3);
        }

        return metrics;
    }
}