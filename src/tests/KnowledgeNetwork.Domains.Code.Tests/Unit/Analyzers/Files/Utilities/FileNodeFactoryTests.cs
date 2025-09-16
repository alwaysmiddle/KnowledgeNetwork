using KnowledgeNetwork.Domains.Code.Analyzers.Files.Utilities;
using KnowledgeNetwork.Domains.Code.Models.Files;
using KnowledgeNetwork.Domains.Code.Tests.Unit.Analyzers.Files.TestInfrastructure;
using KnowledgeNetwork.Tests.Shared;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using Xunit;

namespace KnowledgeNetwork.Domains.Code.Tests.Unit.Analyzers.Files.Utilities;

public class FileNodeFactoryTests
{
    [Fact]
    public async Task CreateFileNodeAsync_WithSimpleClass_ShouldCreateCompleteFileNode()
    {
        // Arrange
        var testCode = """
using System;
using System.Collections.Generic;

namespace TestProject.Services
{
    /// <summary>
    /// A simple user service for testing
    /// </summary>
    public class UserService
    {
        private readonly List<string> _users = new();

        public void AddUser(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                _users.Add(name);
            }
        }

        public List<string> GetUsers() => _users;
    }
}
""";

        var mocks = MockFileSystemBuilder.Create()
            .WithCSharpFile("UserService.cs")
            .WithUsingDirectives("UserService.cs", "System", "System.Collections.Generic")
            .WithDeclaredNamespaces("UserService.cs", "TestProject.Services")
            .WithDeclaredTypes("UserService.cs", new MockTypeInfo
            {
                Name = "UserService",
                FullName = "TestProject.Services.UserService",
                Namespace = "TestProject.Services",
                TypeKind = "Class",
                Visibility = "Public"
            })
            .WithFileMetrics("UserService.cs", linesOfCode: 20, complexity: 3)
            .Build();

        var factory = new FileNodeFactory(
            NullLogger<FileNodeFactory>.Instance,
            mocks.PathResolver,
            mocks.SyntaxUtilities,
            mocks.UsingExtractor,
            mocks.NamespaceExtractor,
            mocks.TypeExtractor,
            mocks.MetricsCalculator
        );

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(testCode);

        // Act
        var result = await factory.CreateFileNodeAsync(compilation, syntaxTree, "TestProject");

        // Assert
        result.ShouldNotBeNull();
        result.FileName.ShouldBe("UserService.cs");
        result.FilePath.ShouldNotBeNullOrEmpty();
        result.FileType.ShouldBe(FileType.Source);
        result.Language.ShouldBe(FileLanguage.CSharp);
        result.ProjectName.ShouldBe("TestProject");

        // Verify all extractors and calculators were called
        mocks.MockUsingExtractor.Verify(x => x.ExtractUsingDirectives(result, It.IsAny<Microsoft.CodeAnalysis.SyntaxNode>()), Times.Once);
        mocks.MockNamespaceExtractor.Verify(x => x.ExtractDeclaredNamespaces(result, It.IsAny<Microsoft.CodeAnalysis.SyntaxNode>()), Times.Once);
        mocks.MockTypeExtractor.Verify(x => x.ExtractDeclaredTypesAsync(result, It.IsAny<Microsoft.CodeAnalysis.SyntaxNode>(), It.IsAny<Microsoft.CodeAnalysis.SemanticModel>()), Times.Once);
        mocks.MockTypeExtractor.Verify(x => x.ExtractReferencedTypesAsync(result, It.IsAny<Microsoft.CodeAnalysis.SyntaxNode>(), It.IsAny<Microsoft.CodeAnalysis.SemanticModel>()), Times.Once);
        mocks.MockMetricsCalculator.Verify(x => x.CalculateFileMetrics(result, It.IsAny<Microsoft.CodeAnalysis.SyntaxNode>()), Times.Once);
    }

    [Fact]
    public async Task CreateFileNodeAsync_WithEmptySyntaxTree_ShouldReturnNull()
    {
        // Arrange
        var mocks = MockFileSystemBuilder.Create().Build();
        var factory = new FileNodeFactory(
            NullLogger<FileNodeFactory>.Instance,
            mocks.PathResolver,
            mocks.SyntaxUtilities,
            mocks.UsingExtractor,
            mocks.NamespaceExtractor,
            mocks.TypeExtractor,
            mocks.MetricsCalculator
        );

        var testCode = "";
        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(testCode);

        // Clear the file path to simulate a syntax tree without a file path
        var syntaxTreeWithoutPath = syntaxTree.WithFilePath("");

        // Act
        var result = await factory.CreateFileNodeAsync(compilation, syntaxTreeWithoutPath, "TestProject");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task CreateFileNodeAsync_WithNullFilePath_ShouldReturnNull()
    {
        // Arrange
        var mocks = MockFileSystemBuilder.Create().Build();
        var factory = new FileNodeFactory(
            NullLogger<FileNodeFactory>.Instance,
            mocks.PathResolver,
            mocks.SyntaxUtilities,
            mocks.UsingExtractor,
            mocks.NamespaceExtractor,
            mocks.TypeExtractor,
            mocks.MetricsCalculator
        );

        var testCode = "public class Test {}";
        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(testCode);
        var syntaxTreeWithNullPath = syntaxTree.WithFilePath(null);

        // Act
        var result = await factory.CreateFileNodeAsync(compilation, syntaxTreeWithNullPath, "TestProject");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task CreateFileNodeAsync_WithComplexFile_ShouldSetAllProperties()
    {
        // Arrange
        var filePath = @"C:\Projects\TestProject\src\Services\UserService.cs";
        var testCode = """
global using System;
using System.Collections.Generic;
using System.Linq;

namespace TestProject.Services;

public interface IUserService
{
    Task<User> GetUserAsync(int id);
}

public class UserService : IUserService
{
    public async Task<User> GetUserAsync(int id)
    {
        // Implementation
        return new User();
    }
}

public record User(int Id, string Name);
""";

        var mocks = MockFileSystemBuilder.Create()
            .WithCSharpFile(filePath, fileSize: 2048)
            .WithUsingDirectives(filePath, "System", "System.Collections.Generic", "System.Linq")
            .WithDeclaredNamespaces(filePath, "TestProject.Services")
            .WithDeclaredTypes(filePath,
                new MockTypeInfo
                {
                    Name = "IUserService",
                    FullName = "TestProject.Services.IUserService",
                    Namespace = "TestProject.Services",
                    TypeKind = "Interface",
                    Visibility = "Public"
                },
                new MockTypeInfo
                {
                    Name = "UserService",
                    FullName = "TestProject.Services.UserService",
                    Namespace = "TestProject.Services",
                    TypeKind = "Class",
                    Visibility = "Public"
                },
                new MockTypeInfo
                {
                    Name = "User",
                    FullName = "TestProject.Services.User",
                    Namespace = "TestProject.Services",
                    TypeKind = "Record",
                    Visibility = "Public"
                })
            .WithReferencedTypes(filePath,
                new MockTypeReference
                {
                    Name = "Task",
                    FullName = "System.Threading.Tasks.Task",
                    Namespace = "System.Threading.Tasks",
                    ReferenceCount = 2,
                    IsExternal = true
                })
            .WithFileMetrics(filePath, linesOfCode: 25, complexity: 2)
            .Build();

        var factory = new FileNodeFactory(
            NullLogger<FileNodeFactory>.Instance,
            mocks.PathResolver,
            mocks.SyntaxUtilities,
            mocks.UsingExtractor,
            mocks.NamespaceExtractor,
            mocks.TypeExtractor,
            mocks.MetricsCalculator
        );

        var (compilation, syntaxTree) = CompilationFactory.CreateExtended(testCode);
        var syntaxTreeWithPath = syntaxTree.WithFilePath(filePath);

        // Act
        var result = await factory.CreateFileNodeAsync(compilation, syntaxTreeWithPath, "TestProject");

        // Assert
        result.ShouldNotBeNull();

        // Basic properties
        result.FilePath.ShouldBe(filePath);
        result.FileName.ShouldBe("UserService.cs");
        result.FileExtension.ShouldBe(".cs");
        result.DirectoryPath.ShouldBe(@"C:\Projects\TestProject\src\Services");
        result.Language.ShouldBe(FileLanguage.CSharp);
        result.ProjectName.ShouldBe("TestProject");
        result.FileType.ShouldBe(FileType.Source);

        // File system properties
        result.FileSize.ShouldBe(2048);
        result.LastModified.ShouldNotBe(default);

        // Extracted content (mocked)
        result.UsingDirectives.Count.ShouldBe(3);
        result.DeclaredNamespaces.Count.ShouldBe(1);
        result.DeclaredTypes.Count.ShouldBe(3);
        result.ReferencedTypes.Count.ShouldBe(1);

        // Metrics
        result.Metrics.ShouldNotBeNull();
        result.Metrics.LinesOfCode.ShouldBe(25);
        result.Metrics.CyclomaticComplexity.ShouldBe(2);
    }

    [Fact]
    public async Task CreateFileNodeAsync_WithTestFile_ShouldSetCorrectFileType()
    {
        // Arrange
        var filePath = @"C:\Projects\TestProject\tests\UserServiceTests.cs";
        var testCode = """
using Xunit;

namespace TestProject.Tests
{
    public class UserServiceTests
    {
        [Fact]
        public void GetUser_ShouldReturnUser()
        {
            // Test implementation
        }
    }
}
""";

        var mocks = MockFileSystemBuilder.Create()
            .WithTestFile(filePath)
            .WithDeclaredNamespaces(filePath, "TestProject.Tests")
            .WithDeclaredTypes(filePath, new MockTypeInfo
            {
                Name = "UserServiceTests",
                TypeKind = "Class",
                Visibility = "Public"
            })
            .Build();

        var factory = new FileNodeFactory(
            NullLogger<FileNodeFactory>.Instance,
            mocks.PathResolver,
            mocks.SyntaxUtilities,
            mocks.UsingExtractor,
            mocks.NamespaceExtractor,
            mocks.TypeExtractor,
            mocks.MetricsCalculator
        );

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(testCode);
        var syntaxTreeWithPath = syntaxTree.WithFilePath(filePath);

        // Act
        var result = await factory.CreateFileNodeAsync(compilation, syntaxTreeWithPath, "TestProject");

        // Assert
        result.ShouldNotBeNull();
        result.FileType.ShouldBe(FileType.Test);
        result.FileName.ShouldBe("UserServiceTests.cs");
    }

    [Fact]
    public async Task CreateFileNodeAsync_WithInterfaceFile_ShouldSetCorrectFileType()
    {
        // Arrange
        var filePath = @"C:\Projects\TestProject\src\Interfaces\IUserService.cs";
        var testCode = """
namespace TestProject.Interfaces
{
    public interface IUserService
    {
        Task<User> GetUserAsync(int id);
    }
}
""";

        var mocks = MockFileSystemBuilder.Create()
            .WithFile(filePath, fileType: FileType.Interface)
            .WithDeclaredNamespaces(filePath, "TestProject.Interfaces")
            .WithDeclaredTypes(filePath, new MockTypeInfo
            {
                Name = "IUserService",
                TypeKind = "Interface",
                Visibility = "Public"
            })
            .Build();

        var factory = new FileNodeFactory(
            NullLogger<FileNodeFactory>.Instance,
            mocks.PathResolver,
            mocks.SyntaxUtilities,
            mocks.UsingExtractor,
            mocks.NamespaceExtractor,
            mocks.TypeExtractor,
            mocks.MetricsCalculator
        );

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(testCode);
        var syntaxTreeWithPath = syntaxTree.WithFilePath(filePath);

        // Act
        var result = await factory.CreateFileNodeAsync(compilation, syntaxTreeWithPath, "TestProject");

        // Assert
        result.ShouldNotBeNull();
        result.FileType.ShouldBe(FileType.Interface);
        result.FileName.ShouldBe("IUserService.cs");
    }

    [Fact]
    public async Task CreateFileNodeAsync_WithFilePathResolverError_ShouldStillCreateNode()
    {
        // Arrange
        var filePath = @"C:\Projects\TestProject\src\UserService.cs";
        var testCode = "public class UserService {}";

        var mocks = MockFileSystemBuilder.Create()
            .WithCSharpFile(filePath)
            .Build();

        // Setup path resolver to return empty string (simulating error)
        mocks.MockPathResolver.Setup(x => x.GetRelativePath(It.IsAny<string>()))
            .Returns(string.Empty);

        var factory = new FileNodeFactory(
            NullLogger<FileNodeFactory>.Instance,
            mocks.PathResolver,
            mocks.SyntaxUtilities,
            mocks.UsingExtractor,
            mocks.NamespaceExtractor,
            mocks.TypeExtractor,
            mocks.MetricsCalculator
        );

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(testCode);
        var syntaxTreeWithPath = syntaxTree.WithFilePath(filePath);

        // Act
        var result = await factory.CreateFileNodeAsync(compilation, syntaxTreeWithPath, "TestProject");

        // Assert
        result.ShouldNotBeNull();
        result.FilePath.ShouldBe(filePath);
        result.RelativePath.ShouldBe(string.Empty); // Should handle the error gracefully
    }

    [Fact]
    public async Task CreateFileNodeAsync_WhenExtractorThrows_ShouldReturnNull()
    {
        // Arrange
        var filePath = @"C:\Projects\TestProject\src\UserService.cs";
        var testCode = "public class UserService {}";

        var mocks = MockFileSystemBuilder.Create()
            .WithCSharpFile(filePath)
            .Build();

        // Setup type extractor to throw an exception
        mocks.MockTypeExtractor.Setup(x => x.ExtractDeclaredTypesAsync(
            It.IsAny<FileNode>(),
            It.IsAny<Microsoft.CodeAnalysis.SyntaxNode>(),
            It.IsAny<Microsoft.CodeAnalysis.SemanticModel>()))
            .ThrowsAsync(new InvalidOperationException("Extractor error"));

        var factory = new FileNodeFactory(
            NullLogger<FileNodeFactory>.Instance,
            mocks.PathResolver,
            mocks.SyntaxUtilities,
            mocks.UsingExtractor,
            mocks.NamespaceExtractor,
            mocks.TypeExtractor,
            mocks.MetricsCalculator
        );

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(testCode);
        var syntaxTreeWithPath = syntaxTree.WithFilePath(filePath);

        // Act
        var result = await factory.CreateFileNodeAsync(compilation, syntaxTreeWithPath, "TestProject");

        // Assert
        result.ShouldBeNull(); // Should return null when extraction fails
    }

    [Fact]
    public async Task CreateFileNodeAsync_WithNonExistentFile_ShouldStillCreateNode()
    {
        // Arrange
        var filePath = @"C:\Projects\TestProject\src\NonExistent.cs";
        var testCode = "public class NonExistent {}";

        var mocks = MockFileSystemBuilder.Create()
            .WithCSharpFile("NonExistent.cs") // Don't set up file system info
            .Build();

        var factory = new FileNodeFactory(
            NullLogger<FileNodeFactory>.Instance,
            mocks.PathResolver,
            mocks.SyntaxUtilities,
            mocks.UsingExtractor,
            mocks.NamespaceExtractor,
            mocks.TypeExtractor,
            mocks.MetricsCalculator
        );

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(testCode);
        var syntaxTreeWithPath = syntaxTree.WithFilePath(filePath);

        // Act
        var result = await factory.CreateFileNodeAsync(compilation, syntaxTreeWithPath, "TestProject");

        // Assert
        result.ShouldNotBeNull();
        result.FilePath.ShouldBe(filePath);
        result.FileSize.ShouldBe(0); // No file info available
        result.LastModified.ShouldBe(default); // No file info available
    }

    [Fact]
    public async Task CreateFileNodeAsync_WithPartialClass_ShouldSetCorrectFileType()
    {
        // Arrange
        var filePath = @"C:\Projects\TestProject\src\User.partial.cs";
        var testCode = """
namespace TestProject.Models
{
    public partial class User
    {
        public string FirstName { get; set; }
    }
}
""";

        var mocks = MockFileSystemBuilder.Create()
            .WithFile(filePath, fileType: FileType.Partial)
            .WithDeclaredTypes(filePath, new MockTypeInfo
            {
                Name = "User",
                TypeKind = "Class",
                Visibility = "Public"
            })
            .Build();

        var factory = new FileNodeFactory(
            NullLogger<FileNodeFactory>.Instance,
            mocks.PathResolver,
            mocks.SyntaxUtilities,
            mocks.UsingExtractor,
            mocks.NamespaceExtractor,
            mocks.TypeExtractor,
            mocks.MetricsCalculator
        );

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(testCode);
        var syntaxTreeWithPath = syntaxTree.WithFilePath(filePath);

        // Act
        var result = await factory.CreateFileNodeAsync(compilation, syntaxTreeWithPath, "TestProject");

        // Assert
        result.ShouldNotBeNull();
        result.FileType.ShouldBe(FileType.Partial);
        result.FileName.ShouldBe("User.partial.cs");
    }
}