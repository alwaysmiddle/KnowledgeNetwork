using KnowledgeNetwork.Domains.Code.Analyzers.Files.Utilities;
using KnowledgeNetwork.Domains.Code.Models.Files;
using Shouldly;
using Xunit;

namespace KnowledgeNetwork.Domains.Code.Tests.Unit.Analyzers.Files.Utilities;

public class FilePathResolverTests
{
    private readonly FilePathResolver _resolver;

    public FilePathResolverTests()
    {
        _resolver = new FilePathResolver();
    }

    [Theory]
    [InlineData(@"C:\Projects\MyApp\src\Services\UserService.cs", "UserService.cs")]
    [InlineData(@"C:\Projects\MyApp\Models\User.cs", "User.cs")]
    [InlineData(@"/home/user/project/src/Controllers/HomeController.cs", "HomeController.cs")]
    [InlineData(@"Services/IUserService.cs", "IUserService.cs")]
    [InlineData(@"UserService.cs", "UserService.cs")]
    public void GetRelativePath_WithValidFilePath_ShouldReturnFileName(string filePath, string expectedFileName)
    {
        // Act
        var result = _resolver.GetRelativePath(filePath);

        // Assert
        result.ShouldBe(expectedFileName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void GetRelativePath_WithInvalidFilePath_ShouldReturnEmptyString(string filePath)
    {
        // Act
        var result = _resolver.GetRelativePath(filePath);

        // Assert
        result.ShouldBe(string.Empty);
    }

    [Theory]
    [InlineData("UserService.cs", @"C:\Projects\MyApp\src\Services\UserService.cs", FileType.Source)]
    [InlineData("IUserService.cs", @"C:\Projects\MyApp\src\Interfaces\IUserService.cs", FileType.Interface)]
    [InlineData("UserServiceTests.cs", @"C:\Projects\MyApp\tests\UserServiceTests.cs", FileType.Test)]
    [InlineData("UserService.Test.cs", @"C:\Projects\MyApp\tests\UserService.Test.cs", FileType.Test)]
    [InlineData("UserService.Tests.cs", @"C:\Projects\MyApp\tests\UserService.Tests.cs", FileType.Test)]
    public void DetermineFileType_WithVariousFileNames_ShouldReturnCorrectType(string fileName, string filePath, FileType expectedType)
    {
        // Act
        var result = _resolver.DetermineFileType(fileName, filePath);

        // Assert
        result.ShouldBe(expectedType);
    }

    [Theory]
    [InlineData("UserModel.cs", @"C:\Projects\MyApp\src\Models\UserModel.cs", FileType.Source)]
    [InlineData("UserDto.cs", @"C:\Projects\MyApp\src\DTOs\UserDto.cs", FileType.Source)]
    [InlineData("UserViewModel.cs", @"C:\Projects\MyApp\src\ViewModels\UserViewModel.cs", FileType.Source)]
    public void DetermineFileType_WithModelFiles_ShouldReturnSourceType(string fileName, string filePath, FileType expectedType)
    {
        // Act
        var result = _resolver.DetermineFileType(fileName, filePath);

        // Assert
        result.ShouldBe(expectedType);
    }

    [Theory]
    [InlineData("UserService.Designer.cs", @"C:\Projects\MyApp\src\UserService.Designer.cs", FileType.Designer)]
    [InlineData("Form1.designer.cs", @"C:\Projects\MyApp\src\Forms\Form1.designer.cs", FileType.Designer)]
    [InlineData("MainWindow.Designer.cs", @"C:\Projects\MyApp\src\MainWindow.Designer.cs", FileType.Designer)]
    public void DetermineFileType_WithDesignerFiles_ShouldReturnDesignerType(string fileName, string filePath, FileType expectedType)
    {
        // Act
        var result = _resolver.DetermineFileType(fileName, filePath);

        // Assert
        result.ShouldBe(expectedType);
    }

    [Theory]
    [InlineData("AssemblyInfo.cs", @"C:\Projects\MyApp\Properties\AssemblyInfo.cs", FileType.Generated)]
    [InlineData("GlobalAssemblyInfo.cs", @"C:\Projects\MyApp\GlobalAssemblyInfo.cs", FileType.Generated)]
    [InlineData("TemporaryGeneratedFile.cs", @"C:\Projects\MyApp\obj\TemporaryGeneratedFile.cs", FileType.Generated)]
    public void DetermineFileType_WithGeneratedFiles_ShouldReturnGeneratedType(string fileName, string filePath, FileType expectedType)
    {
        // Act
        var result = _resolver.DetermineFileType(fileName, filePath);

        // Assert
        result.ShouldBe(expectedType);
    }

    [Theory]
    [InlineData("appsettings.json", @"C:\Projects\MyApp\appsettings.json", FileType.Configuration)]
    [InlineData("web.config", @"C:\Projects\MyApp\web.config", FileType.Configuration)]
    [InlineData("app.config", @"C:\Projects\MyApp\app.config", FileType.Configuration)]
    public void DetermineFileType_WithConfigFiles_ShouldReturnConfigurationType(string fileName, string filePath, FileType expectedType)
    {
        // Act
        var result = _resolver.DetermineFileType(fileName, filePath);

        // Assert
        result.ShouldBe(expectedType);
    }

    [Theory]
    [InlineData("User.partial.cs", @"C:\Projects\MyApp\src\Models\User.partial.cs", FileType.Partial)]
    [InlineData("UserService.Partial.cs", @"C:\Projects\MyApp\src\Services\UserService.Partial.cs", FileType.Partial)]
    [InlineData("Repository.part.cs", @"C:\Projects\MyApp\src\Repository.part.cs", FileType.Partial)]
    public void DetermineFileType_WithPartialFiles_ShouldReturnPartialType(string fileName, string filePath, FileType expectedType)
    {
        // Act
        var result = _resolver.DetermineFileType(fileName, filePath);

        // Assert
        result.ShouldBe(expectedType);
    }

    [Theory]
    [InlineData("logo.png", @"C:\Projects\MyApp\Resources\logo.png", FileType.Resource)]
    [InlineData("strings.resx", @"C:\Projects\MyApp\Resources\strings.resx", FileType.Resource)]
    [InlineData("icon.ico", @"C:\Projects\MyApp\Resources\icon.ico", FileType.Resource)]
    [InlineData("style.css", @"C:\Projects\MyApp\wwwroot\css\style.css", FileType.Resource)]
    [InlineData("app.js", @"C:\Projects\MyApp\wwwroot\js\app.js", FileType.Resource)]
    public void DetermineFileType_WithResourceFiles_ShouldReturnResourceType(string fileName, string filePath, FileType expectedType)
    {
        // Act
        var result = _resolver.DetermineFileType(fileName, filePath);

        // Assert
        result.ShouldBe(expectedType);
    }

    [Fact]
    public void DetermineFileType_WithInterfacePrefix_ShouldReturnInterfaceType()
    {
        // Arrange
        var testCases = new[]
        {
            ("IUserService.cs", @"C:\Projects\MyApp\src\Interfaces\IUserService.cs"),
            ("IRepository.cs", @"C:\Projects\MyApp\src\IRepository.cs"),
            ("IEmailService.cs", @"C:\Projects\MyApp\src\Services\IEmailService.cs")
        };

        foreach (var (fileName, filePath) in testCases)
        {
            // Act
            var result = _resolver.DetermineFileType(fileName, filePath);

            // Assert
            result.ShouldBe(FileType.Interface, $"Failed for {fileName}");
        }
    }

    [Fact]
    public void DetermineFileType_WithTestDirectoryPaths_ShouldReturnTestType()
    {
        // Arrange
        var testCases = new[]
        {
            ("UserService.cs", @"C:\Projects\MyApp\tests\UserService.cs"),
            ("UserService.cs", @"C:\Projects\MyApp\test\UserService.cs"),
            ("UserService.cs", @"C:\Projects\MyApp\Tests\UserService.cs"),
            ("UserService.cs", @"C:\Projects\MyApp\Test\UserService.cs"),
            ("UserService.cs", @"C:\Projects\MyApp\src\Tests\UserService.cs"),
            ("UserService.cs", @"C:\Projects\MyApp\UnitTests\UserService.cs")
        };

        foreach (var (fileName, filePath) in testCases)
        {
            // Act
            var result = _resolver.DetermineFileType(fileName, filePath);

            // Assert
            result.ShouldBe(FileType.Test, $"Failed for path: {filePath}");
        }
    }

    [Fact]
    public void DetermineFileType_WithGeneratedDirectoryPaths_ShouldReturnGeneratedType()
    {
        // Arrange
        var testCases = new[]
        {
            ("TempFile.cs", @"C:\Projects\MyApp\obj\Debug\TempFile.cs"),
            ("Generated.cs", @"C:\Projects\MyApp\bin\Generated.cs"),
            ("AssemblyInfo.cs", @"C:\Projects\MyApp\Properties\AssemblyInfo.cs"),
            ("Reference.cs", @"C:\Projects\MyApp\.vs\Reference.cs")
        };

        foreach (var (fileName, filePath) in testCases)
        {
            // Act
            var result = _resolver.DetermineFileType(fileName, filePath);

            // Assert
            result.ShouldBe(FileType.Generated, $"Failed for path: {filePath}");
        }
    }

    [Theory]
    [InlineData("", "", FileType.Source)]
    [InlineData(null, null, FileType.Source)]
    [InlineData("   ", "   ", FileType.Source)]
    public void DetermineFileType_WithInvalidInputs_ShouldReturnDefaultSourceType(string fileName, string filePath, FileType expectedType)
    {
        // Act
        var result = _resolver.DetermineFileType(fileName, filePath);

        // Assert
        result.ShouldBe(expectedType);
    }

    [Fact]
    public void DetermineFileType_WithMixedCaseNames_ShouldBeCaseInsensitive()
    {
        // Arrange
        var testCases = new[]
        {
            ("IUSERSERVICE.CS", @"C:\Projects\MyApp\src\IUSERSERVICE.CS", FileType.Interface),
            ("userservice.DESIGNER.cs", @"C:\Projects\MyApp\src\userservice.DESIGNER.cs", FileType.Designer),
            ("UserServiceTESTS.cs", @"C:\Projects\MyApp\UserServiceTESTS.cs", FileType.Test),
            ("User.PARTIAL.cs", @"C:\Projects\MyApp\User.PARTIAL.cs", FileType.Partial)
        };

        foreach (var (fileName, filePath, expectedType) in testCases)
        {
            // Act
            var result = _resolver.DetermineFileType(fileName, filePath);

            // Assert
            result.ShouldBe(expectedType, $"Failed for {fileName} - expected {expectedType}");
        }
    }

    [Fact]
    public void DetermineFileType_WithComplexNestedPaths_ShouldReturnCorrectType()
    {
        // Arrange
        var testCases = new[]
        {
            ("IUserService.cs", @"C:\Projects\MyApp\src\Domain\Interfaces\Services\IUserService.cs", FileType.Interface),
            ("UserServiceTests.cs", @"C:\Projects\MyApp\tests\Unit\Services\UserServiceTests.cs", FileType.Test),
            ("Form1.Designer.cs", @"C:\Projects\MyApp\src\UI\Forms\Form1.Designer.cs", FileType.Designer),
            ("User.partial.cs", @"C:\Projects\MyApp\src\Models\Entities\User.partial.cs", FileType.Partial),
            ("TempAssembly.cs", @"C:\Projects\MyApp\obj\Debug\net8.0\TempAssembly.cs", FileType.Generated)
        };

        foreach (var (fileName, filePath, expectedType) in testCases)
        {
            // Act
            var result = _resolver.DetermineFileType(fileName, filePath);

            // Assert
            result.ShouldBe(expectedType, $"Failed for complex path: {filePath}");
        }
    }

    [Theory]
    [InlineData(@"C:\Projects\MyApp\src\Services\UserService.cs")]
    [InlineData(@"/home/user/project/src/Controllers/HomeController.cs")]
    [InlineData(@"..\..\Services\UserService.cs")]
    [InlineData(@".\Models\User.cs")]
    public void GetRelativePath_WithDifferentPathFormats_ShouldHandleCorrectly(string filePath)
    {
        // Act
        var result = _resolver.GetRelativePath(filePath);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldNotContain(@"\");
        result.ShouldNotContain("/");
        result.ShouldEndWith(".cs");
    }

    [Fact]
    public void GetRelativePath_WithSpecialCharactersInPath_ShouldHandleCorrectly()
    {
        // Arrange
        var testCases = new[]
        {
            @"C:\Projects\My App\src\User Service.cs",
            @"C:\Projects\App(2021)\src\UserService.cs",
            @"C:\Projects\App-v2.0\src\UserService.cs",
            @"C:\Projects\App_Production\src\UserService.cs"
        };

        foreach (var filePath in testCases)
        {
            // Act
            var result = _resolver.GetRelativePath(filePath);

            // Assert
            result.ShouldNotBeNullOrEmpty($"Failed for path: {filePath}");
            result.ShouldEndWith(".cs");
        }
    }

    [Fact]
    public void DetermineFileType_WithEdgeCaseFileNames_ShouldReturnAppropriateType()
    {
        // Arrange - Test files that could match multiple patterns
        var testCases = new[]
        {
            ("ITestService.Tests.cs", @"C:\Projects\MyApp\ITestService.Tests.cs", FileType.Test), // Interface + Test
            ("TestBase.Designer.cs", @"C:\Projects\MyApp\TestBase.Designer.cs", FileType.Designer), // Test + Designer
            ("IPartialService.partial.cs", @"C:\Projects\MyApp\IPartialService.partial.cs", FileType.Partial), // Interface + Partial
            ("GeneratedTests.cs", @"C:\Projects\MyApp\obj\GeneratedTests.cs", FileType.Generated) // Test + Generated path
        };

        foreach (var (fileName, filePath, expectedType) in testCases)
        {
            // Act
            var result = _resolver.DetermineFileType(fileName, filePath);

            // Assert
            result.ShouldBe(expectedType, $"Failed for edge case: {fileName} at {filePath}");
        }
    }
}