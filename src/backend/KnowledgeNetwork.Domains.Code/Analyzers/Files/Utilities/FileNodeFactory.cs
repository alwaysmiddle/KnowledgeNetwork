using KnowledgeNetwork.Domains.Code.Analyzers.Files.Abstractions;
using KnowledgeNetwork.Domains.Code.Models.Files;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Files.Utilities;

/// <summary>
/// Factory for creating FileNode instances with complete metadata and content analysis
/// </summary>
public class FileNodeFactory(ILogger<FileNodeFactory> logger, IFilePathResolver filePathResolver, IFileSyntaxUtilities syntaxUtilities,
    IUsingDirectiveExtractor usingDirectiveExtractor, INamespaceExtractor namespaceExtractor, ITypeExtractor typeExtractor,
    IFileMetricsCalculator metricsCalculator) : IFileNodeFactory
{
    /// <summary>
    /// Creates a complete file node from a syntax tree with all metadata and content extracted
    /// </summary>
    public async Task<FileNode?> CreateFileNodeAsync(Compilation compilation, SyntaxTree syntaxTree, string projectName)
    {
        try
        {
            var filePath = syntaxTree.FilePath;
            if (string.IsNullOrEmpty(filePath)) return null;

            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var root = await syntaxTree.GetRootAsync();

            var fileNode = new FileNode
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                RelativePath = filePathResolver.GetRelativePath(filePath),
                DirectoryPath = Path.GetDirectoryName(filePath) ?? string.Empty,
                FileExtension = Path.GetExtension(filePath),
                Language = FileLanguage.CSharp,
                ProjectName = projectName,
                Location = syntaxUtilities.GetLocationInfo(root)
            };

            // Determine file type
            fileNode.FileType = filePathResolver.DetermineFileType(fileNode.FileName, filePath);

            // Get file size and last modified time
            if (File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                fileNode.FileSize = fileInfo.Length;
                fileNode.LastModified = fileInfo.LastWriteTime;
            }

            // Extract using directives
            usingDirectiveExtractor.ExtractUsingDirectives(fileNode, root);

            // Extract declared namespaces
            namespaceExtractor.ExtractDeclaredNamespaces(fileNode, root);

            // Extract declared types
            await typeExtractor.ExtractDeclaredTypesAsync(fileNode, root, semanticModel);

            // Extract referenced types
            await typeExtractor.ExtractReferencedTypesAsync(fileNode, root, semanticModel);

            // Calculate metrics
            fileNode.Metrics = metricsCalculator.CalculateFileMetrics(fileNode, root);

            return fileNode;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to create file node for syntax tree: {FilePath}", syntaxTree.FilePath);
            return null;
        }
    }
}