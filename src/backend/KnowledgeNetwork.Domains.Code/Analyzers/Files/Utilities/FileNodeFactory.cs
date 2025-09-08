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
    private readonly ILogger<FileNodeFactory> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFilePathResolver _filePathResolver = filePathResolver ?? throw new ArgumentNullException(nameof(filePathResolver));
    private readonly IFileSyntaxUtilities _syntaxUtilities = syntaxUtilities ?? throw new ArgumentNullException(nameof(syntaxUtilities));
    private readonly IUsingDirectiveExtractor _usingDirectiveExtractor = usingDirectiveExtractor ?? throw new ArgumentNullException(nameof(usingDirectiveExtractor));
    private readonly INamespaceExtractor _namespaceExtractor = namespaceExtractor ?? throw new ArgumentNullException(nameof(namespaceExtractor));
    private readonly ITypeExtractor _typeExtractor = typeExtractor ?? throw new ArgumentNullException(nameof(typeExtractor));
    private readonly IFileMetricsCalculator _metricsCalculator = metricsCalculator ?? throw new ArgumentNullException(nameof(metricsCalculator));

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
                RelativePath = _filePathResolver.GetRelativePath(filePath),
                DirectoryPath = Path.GetDirectoryName(filePath) ?? string.Empty,
                FileExtension = Path.GetExtension(filePath),
                Language = FileLanguage.CSharp,
                ProjectName = projectName,
                Location = _syntaxUtilities.GetLocationInfo(root)
            };

            // Determine file type
            fileNode.FileType = _filePathResolver.DetermineFileType(fileNode.FileName, filePath);

            // Get file size and last modified time
            if (File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                fileNode.FileSize = fileInfo.Length;
                fileNode.LastModified = fileInfo.LastWriteTime;
            }

            // Extract using directives
            _usingDirectiveExtractor.ExtractUsingDirectives(fileNode, root);

            // Extract declared namespaces
            _namespaceExtractor.ExtractDeclaredNamespaces(fileNode, root);

            // Extract declared types
            await _typeExtractor.ExtractDeclaredTypesAsync(fileNode, root, semanticModel);

            // Extract referenced types
            await _typeExtractor.ExtractReferencedTypesAsync(fileNode, root, semanticModel);

            // Calculate metrics
            fileNode.Metrics = _metricsCalculator.CalculateFileMetrics(fileNode, root);

            return fileNode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create file node for syntax tree: {FilePath}", syntaxTree.FilePath);
            return null;
        }
    }
}