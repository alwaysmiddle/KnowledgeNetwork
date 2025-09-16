using KnowledgeNetwork.Domains.Code.Analyzers.Files.Abstractions;
using KnowledgeNetwork.Domains.Code.Models.Files;
using KnowledgeNetwork.Domains.Code.Models.Common;
using Moq;

namespace KnowledgeNetwork.Domains.Code.Tests.Unit.Analyzers.Files.TestInfrastructure;

/// <summary>
/// Builder for creating mock file system scenarios for FileNodeFactory testing
/// </summary>
public class MockFileSystemBuilder
{
    private readonly Mock<IFilePathResolver> _mockPathResolver = new();
    private readonly Mock<IFileSyntaxUtilities> _mockSyntaxUtilities = new();
    private readonly Mock<IUsingDirectiveExtractor> _mockUsingExtractor = new();
    private readonly Mock<INamespaceExtractor> _mockNamespaceExtractor = new();
    private readonly Mock<ITypeExtractor> _mockTypeExtractor = new();
    private readonly Mock<IFileMetricsCalculator> _mockMetricsCalculator = new();

    private readonly Dictionary<string, MockFileInfo> _mockFiles = new();

    /// <summary>
    /// Create a new builder instance
    /// </summary>
    public static MockFileSystemBuilder Create() => new();

    /// <summary>
    /// Add a mock file with specified properties
    /// </summary>
    public MockFileSystemBuilder WithFile(string filePath, long fileSize = 1024, DateTime? lastModified = null, FileType fileType = FileType.Source)
    {
        var mockFileInfo = new MockFileInfo
        {
            FilePath = filePath,
            FileName = Path.GetFileName(filePath),
            FileSize = fileSize,
            LastModified = lastModified ?? DateTime.Now,
            FileType = fileType
        };

        _mockFiles[filePath] = mockFileInfo;

        // Setup IFilePathResolver behavior for this file
        _mockPathResolver.Setup(x => x.GetRelativePath(filePath))
            .Returns(mockFileInfo.FileName);

        _mockPathResolver.Setup(x => x.DetermineFileType(mockFileInfo.FileName, filePath))
            .Returns(fileType);

        return this;
    }

    /// <summary>
    /// Add a C# source file with default properties
    /// </summary>
    public MockFileSystemBuilder WithCSharpFile(string filePath, long fileSize = 2048)
    {
        return WithFile(filePath, fileSize, DateTime.Now, FileType.Source);
    }

    /// <summary>
    /// Add a test file
    /// </summary>
    public MockFileSystemBuilder WithTestFile(string filePath, long fileSize = 1536)
    {
        return WithFile(filePath, fileSize, DateTime.Now, FileType.Test);
    }

    /// <summary>
    /// Add a generated file
    /// </summary>
    public MockFileSystemBuilder WithGeneratedFile(string filePath, long fileSize = 512)
    {
        return WithFile(filePath, fileSize, DateTime.Now, FileType.Generated);
    }

    /// <summary>
    /// Setup using directive extraction behavior
    /// </summary>
    public MockFileSystemBuilder WithUsingDirectives(string filePath, params string[] usingDirectives)
    {
        _mockUsingExtractor.Setup(x => x.ExtractUsingDirectives(It.IsAny<FileNode>(), It.IsAny<Microsoft.CodeAnalysis.SyntaxNode>()))
            .Callback<FileNode, Microsoft.CodeAnalysis.SyntaxNode>((fileNode, root) =>
            {
                if (fileNode.FilePath == filePath)
                {
                    fileNode.UsingDirectives.Clear();
                    foreach (var usingDirective in usingDirectives)
                    {
                        fileNode.UsingDirectives.Add(usingDirective);
                    }
                }
            });

        return this;
    }

    /// <summary>
    /// Setup declared namespace extraction behavior
    /// </summary>
    public MockFileSystemBuilder WithDeclaredNamespaces(string filePath, params string[] namespaces)
    {
        _mockNamespaceExtractor.Setup(x => x.ExtractDeclaredNamespaces(It.IsAny<FileNode>(), It.IsAny<Microsoft.CodeAnalysis.SyntaxNode>()))
            .Callback<FileNode, Microsoft.CodeAnalysis.SyntaxNode>((fileNode, root) =>
            {
                if (fileNode.FilePath == filePath)
                {
                    fileNode.DeclaredNamespaces.Clear();
                    foreach (var ns in namespaces)
                    {
                        fileNode.DeclaredNamespaces.Add(ns);
                    }
                }
            });

        return this;
    }

    /// <summary>
    /// Setup declared types extraction behavior
    /// </summary>
    public MockFileSystemBuilder WithDeclaredTypes(string filePath, params MockTypeInfo[] types)
    {
        _mockTypeExtractor.Setup(x => x.ExtractDeclaredTypesAsync(It.IsAny<FileNode>(), It.IsAny<Microsoft.CodeAnalysis.SyntaxNode>(), It.IsAny<Microsoft.CodeAnalysis.SemanticModel>()))
            .Callback<FileNode, Microsoft.CodeAnalysis.SyntaxNode, Microsoft.CodeAnalysis.SemanticModel>((fileNode, root, semanticModel) =>
            {
                if (fileNode.FilePath == filePath)
                {
                    fileNode.DeclaredTypes.Clear();
                    foreach (var type in types)
                    {
                        fileNode.DeclaredTypes.Add(new DeclaredType
                        {
                            Name = type.Name,
                            FullName = type.FullName,
                            Namespace = type.Namespace,
                            TypeKind = type.TypeKind,
                            Visibility = type.Visibility,
                            Location = new CSharpLocationInfo { StartLine = 1, StartColumn = 1 }
                        });
                    }
                }
            })
            .Returns(Task.CompletedTask);

        return this;
    }

    /// <summary>
    /// Setup referenced types extraction behavior
    /// </summary>
    public MockFileSystemBuilder WithReferencedTypes(string filePath, params MockTypeReference[] typeReferences)
    {
        _mockTypeExtractor.Setup(x => x.ExtractReferencedTypesAsync(It.IsAny<FileNode>(), It.IsAny<Microsoft.CodeAnalysis.SyntaxNode>(), It.IsAny<Microsoft.CodeAnalysis.SemanticModel>()))
            .Callback<FileNode, Microsoft.CodeAnalysis.SyntaxNode, Microsoft.CodeAnalysis.SemanticModel>((fileNode, root, semanticModel) =>
            {
                if (fileNode.FilePath == filePath)
                {
                    fileNode.ReferencedTypes.Clear();
                    foreach (var typeRef in typeReferences)
                    {
                        fileNode.ReferencedTypes.Add(new ReferencedType
                        {
                            Name = typeRef.Name,
                            FullName = typeRef.FullName,
                            Namespace = typeRef.Namespace,
                            ReferenceCount = typeRef.ReferenceCount,
                            IsExternal = typeRef.IsExternal,
                            ReferenceKind = typeRef.ReferenceKind,
                            ReferenceLocations = typeRef.ReferenceLocations
                        });
                    }
                }
            })
            .Returns(Task.CompletedTask);

        return this;
    }

    /// <summary>
    /// Setup file metrics calculation behavior
    /// </summary>
    public MockFileSystemBuilder WithFileMetrics(string filePath, int linesOfCode = 100, int complexity = 5)
    {
        _mockMetricsCalculator.Setup(x => x.CalculateFileMetrics(It.IsAny<FileNode>(), It.IsAny<Microsoft.CodeAnalysis.SyntaxNode>()))
            .Returns<FileNode, Microsoft.CodeAnalysis.SyntaxNode>((fileNode, root) =>
            {
                if (fileNode.FilePath == filePath)
                {
                    return new FileMetrics
                    {
                        LinesOfCode = linesOfCode,
                        CommentLines = linesOfCode / 5,
                        BlankLines = linesOfCode / 10,
                        CyclomaticComplexity = complexity,
                        DeclaredTypeCount = 1
                    };
                }
                return new FileMetrics();
            });

        return this;
    }

    /// <summary>
    /// Setup syntax utilities behavior
    /// </summary>
    public MockFileSystemBuilder WithSyntaxUtilities(Action<Mock<IFileSyntaxUtilities>>? setupAction = null)
    {
        // Default setup
        _mockSyntaxUtilities.Setup(x => x.GetLocationInfo(It.IsAny<Microsoft.CodeAnalysis.SyntaxNode>()))
            .Returns(new CSharpLocationInfo { StartLine = 1, StartColumn = 1 });

        // Allow custom setup
        setupAction?.Invoke(_mockSyntaxUtilities);

        return this;
    }

    /// <summary>
    /// Build the mocks and return them
    /// </summary>
    public FileSystemMocks Build()
    {
        // Setup default behaviors if not already configured
        SetupDefaultBehaviors();

        return new FileSystemMocks
        {
            PathResolver = _mockPathResolver.Object,
            SyntaxUtilities = _mockSyntaxUtilities.Object,
            UsingExtractor = _mockUsingExtractor.Object,
            NamespaceExtractor = _mockNamespaceExtractor.Object,
            TypeExtractor = _mockTypeExtractor.Object,
            MetricsCalculator = _mockMetricsCalculator.Object,
            MockPathResolver = _mockPathResolver,
            MockSyntaxUtilities = _mockSyntaxUtilities,
            MockUsingExtractor = _mockUsingExtractor,
            MockNamespaceExtractor = _mockNamespaceExtractor,
            MockTypeExtractor = _mockTypeExtractor,
            MockMetricsCalculator = _mockMetricsCalculator,
            MockFiles = _mockFiles
        };
    }

    private void SetupDefaultBehaviors()
    {
        // Default IFileSyntaxUtilities behavior
        if (!_mockSyntaxUtilities.Setups.Any())
        {
            _mockSyntaxUtilities.Setup(x => x.GetLocationInfo(It.IsAny<Microsoft.CodeAnalysis.SyntaxNode>()))
                .Returns(new CSharpLocationInfo { StartLine = 1, StartColumn = 1 });
        }

        // Default extractor behaviors (no-op if not specifically configured)
        if (!_mockUsingExtractor.Setups.Any())
        {
            _mockUsingExtractor.Setup(x => x.ExtractUsingDirectives(It.IsAny<FileNode>(), It.IsAny<Microsoft.CodeAnalysis.SyntaxNode>()));
        }

        if (!_mockNamespaceExtractor.Setups.Any())
        {
            _mockNamespaceExtractor.Setup(x => x.ExtractDeclaredNamespaces(It.IsAny<FileNode>(), It.IsAny<Microsoft.CodeAnalysis.SyntaxNode>()));
        }

        if (!_mockTypeExtractor.Setups.Any())
        {
            _mockTypeExtractor.Setup(x => x.ExtractDeclaredTypesAsync(It.IsAny<FileNode>(), It.IsAny<Microsoft.CodeAnalysis.SyntaxNode>(), It.IsAny<Microsoft.CodeAnalysis.SemanticModel>()))
                .Returns(Task.CompletedTask);
            _mockTypeExtractor.Setup(x => x.ExtractReferencedTypesAsync(It.IsAny<FileNode>(), It.IsAny<Microsoft.CodeAnalysis.SyntaxNode>(), It.IsAny<Microsoft.CodeAnalysis.SemanticModel>()))
                .Returns(Task.CompletedTask);
        }

        // Default metrics calculator behavior
        if (!_mockMetricsCalculator.Setups.Any())
        {
            _mockMetricsCalculator.Setup(x => x.CalculateFileMetrics(It.IsAny<FileNode>(), It.IsAny<Microsoft.CodeAnalysis.SyntaxNode>()))
                .Returns(new FileMetrics { LinesOfCode = 50, CyclomaticComplexity = 3 });
        }
    }
}

/// <summary>
/// Container for all file system mocks
/// </summary>
public class FileSystemMocks
{
    public IFilePathResolver PathResolver { get; init; } = null!;
    public IFileSyntaxUtilities SyntaxUtilities { get; init; } = null!;
    public IUsingDirectiveExtractor UsingExtractor { get; init; } = null!;
    public INamespaceExtractor NamespaceExtractor { get; init; } = null!;
    public ITypeExtractor TypeExtractor { get; init; } = null!;
    public IFileMetricsCalculator MetricsCalculator { get; init; } = null!;

    // Access to the actual mocks for verification
    public Mock<IFilePathResolver> MockPathResolver { get; init; } = null!;
    public Mock<IFileSyntaxUtilities> MockSyntaxUtilities { get; init; } = null!;
    public Mock<IUsingDirectiveExtractor> MockUsingExtractor { get; init; } = null!;
    public Mock<INamespaceExtractor> MockNamespaceExtractor { get; init; } = null!;
    public Mock<ITypeExtractor> MockTypeExtractor { get; init; } = null!;
    public Mock<IFileMetricsCalculator> MockMetricsCalculator { get; init; } = null!;

    public Dictionary<string, MockFileInfo> MockFiles { get; init; } = new();
}

/// <summary>
/// Mock file information
/// </summary>
public class MockFileInfo
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime LastModified { get; set; }
    public FileType FileType { get; set; }
}

/// <summary>
/// Mock type information for declared types
/// </summary>
public class MockTypeInfo
{
    public string Name { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string TypeKind { get; set; } = "Class";
    public string Visibility { get; set; } = "Public";
}

/// <summary>
/// Mock type reference information
/// </summary>
public class MockTypeReference
{
    public string Name { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public int ReferenceCount { get; set; } = 1;
    public bool IsExternal { get; set; }
    public TypeReferenceKind ReferenceKind { get; set; } = TypeReferenceKind.Direct;
    public List<CSharpLocationInfo> ReferenceLocations { get; set; } = new();
}