using KnowledgeNetwork.Domains.Code.Analyzers.Files.Utilities;
using KnowledgeNetwork.Domains.Code.Tests.Unit.Analyzers.Files.TestInfrastructure;
using KnowledgeNetwork.Tests.Shared;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;
using Xunit;

namespace KnowledgeNetwork.Domains.Code.Tests.Unit.Analyzers.Files.Utilities;

public class FileSyntaxUtilitiesTests
{
    private readonly FileSyntaxUtilities _utilities;

    public FileSyntaxUtilitiesTests()
    {
        _utilities = new FileSyntaxUtilities();
    }

    [Fact]
    public void GetLocationInfo_WithClassDeclaration_ShouldReturnCorrectLocation()
    {
        // Arrange
        var testCode = """
using System;

namespace TestNamespace
{
    public class TestClass
    {
        public void Method() { }
    }
}
""";

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(testCode);
        var root = syntaxTree.GetRoot();
        var classDeclaration = root.DescendantNodes().OfType<ClassDeclarationSyntax>().First();

        // Act
        var locationInfo = _utilities.GetLocationInfo(classDeclaration);

        // Assert
        locationInfo.ShouldNotBeNull();
        locationInfo.StartLine.ShouldBeGreaterThan(0);
        locationInfo.StartColumn.ShouldBeGreaterThanOrEqualTo(0);
        locationInfo.EndLine.ShouldBeGreaterThanOrEqualTo(locationInfo.StartLine);
        locationInfo.EndColumn.ShouldBeGreaterThanOrEqualTo(locationInfo.StartColumn);
        locationInfo.FilePath.ShouldBe(syntaxTree.FilePath);
    }

    [Fact]
    public void GetLocationInfo_WithMethodDeclaration_ShouldReturnCorrectLocation()
    {
        // Arrange
        var testCode = """
namespace TestNamespace
{
    public class TestClass
    {
        public void TestMethod()
        {
            Console.WriteLine("Hello");
        }
    }
}
""";

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(testCode);
        var root = syntaxTree.GetRoot();
        var methodDeclaration = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var locationInfo = _utilities.GetLocationInfo(methodDeclaration);

        // Assert
        locationInfo.ShouldNotBeNull();
        locationInfo.StartLine.ShouldBeGreaterThan(0);
        locationInfo.StartColumn.ShouldBeGreaterThanOrEqualTo(0);
        locationInfo.EndLine.ShouldBeGreaterThanOrEqualTo(locationInfo.StartLine);
        locationInfo.EndColumn.ShouldBeGreaterThanOrEqualTo(locationInfo.StartColumn);
    }

    [Fact]
    public void GetLocationInfo_WithUsingDirective_ShouldReturnCorrectLocation()
    {
        // Arrange
        var testCode = """
using System;
using System.Collections.Generic;

namespace TestNamespace
{
    public class TestClass { }
}
""";

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(testCode);
        var root = syntaxTree.GetRoot();
        var usingDirectives = root.DescendantNodes().OfType<UsingDirectiveSyntax>().ToList();

        // Act & Assert
        foreach (var usingDirective in usingDirectives)
        {
            var locationInfo = _utilities.GetLocationInfo(usingDirective);

            locationInfo.ShouldNotBeNull();
            locationInfo.StartLine.ShouldBeGreaterThan(0);
            locationInfo.StartColumn.ShouldBeGreaterThanOrEqualTo(0);
            locationInfo.EndLine.ShouldBeGreaterThanOrEqualTo(locationInfo.StartLine);
            locationInfo.EndColumn.ShouldBeGreaterThanOrEqualTo(locationInfo.StartColumn);
        }
    }

    [Fact]
    public void GetLocationInfo_WithNamespaceDeclaration_ShouldReturnCorrectLocation()
    {
        // Arrange
        var testCode = """
using System;

namespace TestNamespace.SubNamespace
{
    public class TestClass { }
}
""";

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(testCode);
        var root = syntaxTree.GetRoot();
        var namespaceDeclaration = root.DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>().First();

        // Act
        var locationInfo = _utilities.GetLocationInfo(namespaceDeclaration);

        // Assert
        locationInfo.ShouldNotBeNull();
        locationInfo.StartLine.ShouldBeGreaterThan(0);
        locationInfo.StartColumn.ShouldBeGreaterThanOrEqualTo(0);
        locationInfo.EndLine.ShouldBeGreaterThanOrEqualTo(locationInfo.StartLine);
        locationInfo.EndColumn.ShouldBeGreaterThanOrEqualTo(locationInfo.StartColumn);
    }

    [Fact]
    public void GetLocationInfo_WithFileScopedNamespace_ShouldReturnCorrectLocation()
    {
        // Arrange
        var testCode = """
using System;

namespace TestNamespace;

public class TestClass
{
    public void Method() { }
}
""";

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(testCode);
        var root = syntaxTree.GetRoot();
        var fileScopedNamespace = root.DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>().First();

        // Act
        var locationInfo = _utilities.GetLocationInfo(fileScopedNamespace);

        // Assert
        locationInfo.ShouldNotBeNull();
        locationInfo.StartLine.ShouldBeGreaterThan(0);
        locationInfo.StartColumn.ShouldBeGreaterThanOrEqualTo(0);
        locationInfo.EndLine.ShouldBeGreaterThanOrEqualTo(locationInfo.StartLine);
        locationInfo.EndColumn.ShouldBeGreaterThanOrEqualTo(locationInfo.StartColumn);
    }

    [Fact]
    public void GetLocationInfo_WithPropertyDeclaration_ShouldReturnCorrectLocation()
    {
        // Arrange
        var testCode = """
namespace TestNamespace
{
    public class TestClass
    {
        public string Name { get; set; }

        public int Age { get; private set; }

        public bool IsActive => true;
    }
}
""";

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(testCode);
        var root = syntaxTree.GetRoot();
        var properties = root.DescendantNodes().OfType<PropertyDeclarationSyntax>().ToList();

        // Act & Assert
        properties.Count.ShouldBe(3);

        foreach (var property in properties)
        {
            var locationInfo = _utilities.GetLocationInfo(property);

            locationInfo.ShouldNotBeNull();
            locationInfo.StartLine.ShouldBeGreaterThan(0);
            locationInfo.StartColumn.ShouldBeGreaterThanOrEqualTo(0);
            locationInfo.EndLine.ShouldBeGreaterThanOrEqualTo(locationInfo.StartLine);
            locationInfo.EndColumn.ShouldBeGreaterThanOrEqualTo(locationInfo.StartColumn);
        }
    }

    [Fact]
    public void GetLocationInfo_WithInterfaceDeclaration_ShouldReturnCorrectLocation()
    {
        // Arrange
        var testCode = """
namespace TestNamespace
{
    public interface ITestInterface
    {
        void Method();
        string Property { get; set; }
    }
}
""";

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(testCode);
        var root = syntaxTree.GetRoot();
        var interfaceDeclaration = root.DescendantNodes().OfType<InterfaceDeclarationSyntax>().First();

        // Act
        var locationInfo = _utilities.GetLocationInfo(interfaceDeclaration);

        // Assert
        locationInfo.ShouldNotBeNull();
        locationInfo.StartLine.ShouldBeGreaterThan(0);
        locationInfo.StartColumn.ShouldBeGreaterThanOrEqualTo(0);
        locationInfo.EndLine.ShouldBeGreaterThanOrEqualTo(locationInfo.StartLine);
        locationInfo.EndColumn.ShouldBeGreaterThanOrEqualTo(locationInfo.StartColumn);
    }

    [Fact]
    public void GetLocationInfo_WithEnumDeclaration_ShouldReturnCorrectLocation()
    {
        // Arrange
        var testCode = """
namespace TestNamespace
{
    public enum TestEnum
    {
        Value1,
        Value2,
        Value3
    }
}
""";

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(testCode);
        var root = syntaxTree.GetRoot();
        var enumDeclaration = root.DescendantNodes().OfType<EnumDeclarationSyntax>().First();

        // Act
        var locationInfo = _utilities.GetLocationInfo(enumDeclaration);

        // Assert
        locationInfo.ShouldNotBeNull();
        locationInfo.StartLine.ShouldBeGreaterThan(0);
        locationInfo.StartColumn.ShouldBeGreaterThanOrEqualTo(0);
        locationInfo.EndLine.ShouldBeGreaterThanOrEqualTo(locationInfo.StartLine);
        locationInfo.EndColumn.ShouldBeGreaterThanOrEqualTo(locationInfo.StartColumn);
    }

    [Fact]
    public void GetLocationInfo_WithStructDeclaration_ShouldReturnCorrectLocation()
    {
        // Arrange
        var testCode = """
namespace TestNamespace
{
    public struct TestStruct
    {
        public int X;
        public int Y;

        public TestStruct(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
""";

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(testCode);
        var root = syntaxTree.GetRoot();
        var structDeclaration = root.DescendantNodes().OfType<StructDeclarationSyntax>().First();

        // Act
        var locationInfo = _utilities.GetLocationInfo(structDeclaration);

        // Assert
        locationInfo.ShouldNotBeNull();
        locationInfo.StartLine.ShouldBeGreaterThan(0);
        locationInfo.StartColumn.ShouldBeGreaterThanOrEqualTo(0);
        locationInfo.EndLine.ShouldBeGreaterThanOrEqualTo(locationInfo.StartLine);
        locationInfo.EndColumn.ShouldBeGreaterThanOrEqualTo(locationInfo.StartColumn);
    }

    [Fact]
    public void GetLocationInfo_WithComplexNestedStructure_ShouldReturnCorrectLocations()
    {
        // Arrange
        var testCode = """
using System;
using System.Collections.Generic;

namespace OuterNamespace.InnerNamespace
{
    public class OuterClass
    {
        public class NestedClass
        {
            public enum NestedEnum { A, B, C }

            public interface INestedInterface
            {
                void NestedMethod();
            }
        }

        public void OuterMethod()
        {
            var list = new List<string>();
        }
    }
}
""";

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(testCode);
        var root = syntaxTree.GetRoot();

        // Act & Assert - Test multiple nested elements
        var outerClass = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
            .First(c => c.Identifier.ValueText == "OuterClass");
        var outerClassLocation = _utilities.GetLocationInfo(outerClass);
        outerClassLocation.ShouldNotBeNull();

        var nestedClass = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
            .First(c => c.Identifier.ValueText == "NestedClass");
        var nestedClassLocation = _utilities.GetLocationInfo(nestedClass);
        nestedClassLocation.ShouldNotBeNull();
        nestedClassLocation.StartLine.ShouldBeGreaterThan(outerClassLocation.StartLine);

        var nestedEnum = root.DescendantNodes().OfType<EnumDeclarationSyntax>().First();
        var nestedEnumLocation = _utilities.GetLocationInfo(nestedEnum);
        nestedEnumLocation.ShouldNotBeNull();
        nestedEnumLocation.StartLine.ShouldBeGreaterThan(nestedClassLocation.StartLine);

        var nestedInterface = root.DescendantNodes().OfType<InterfaceDeclarationSyntax>().First();
        var nestedInterfaceLocation = _utilities.GetLocationInfo(nestedInterface);
        nestedInterfaceLocation.ShouldNotBeNull();
        nestedInterfaceLocation.StartLine.ShouldBeGreaterThan(nestedEnumLocation.StartLine);
    }

    [Fact]
    public void GetLocationInfo_WithRootNode_ShouldReturnFileLocation()
    {
        // Arrange
        var testCode = """
using System;

namespace TestNamespace
{
    public class TestClass { }
}
""";

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(testCode);
        var root = syntaxTree.GetRoot();

        // Act
        var locationInfo = _utilities.GetLocationInfo(root);

        // Assert
        locationInfo.ShouldNotBeNull();
        locationInfo.StartLine.ShouldBe(1);
        locationInfo.StartColumn.ShouldBe(0);
        locationInfo.EndLine.ShouldBeGreaterThan(1);
        locationInfo.EndColumn.ShouldBeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void GetLocationInfo_WithEmptyFile_ShouldReturnValidLocation()
    {
        // Arrange
        var testCode = "";
        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(testCode);
        var root = syntaxTree.GetRoot();

        // Act
        var locationInfo = _utilities.GetLocationInfo(root);

        // Assert
        locationInfo.ShouldNotBeNull();
        locationInfo.StartLine.ShouldBe(1);
        locationInfo.StartColumn.ShouldBe(0);
        locationInfo.EndLine.ShouldBe(1);
        locationInfo.EndColumn.ShouldBe(0);
    }

    [Fact]
    public void GetLocationInfo_WithMultiLineElements_ShouldCalculateCorrectSpans()
    {
        // Arrange
        var testCode = """
namespace TestNamespace
{
    public class TestClass
    {
        public void MultiLineMethod(
            string parameter1,
            int parameter2,
            bool parameter3)
        {
            if (parameter3)
            {
                Console.WriteLine($"Param1: {parameter1}, Param2: {parameter2}");
            }
        }
    }
}
""";

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(testCode);
        var root = syntaxTree.GetRoot();
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().First();

        // Act
        var locationInfo = _utilities.GetLocationInfo(method);

        // Assert
        locationInfo.ShouldNotBeNull();
        locationInfo.StartLine.ShouldBeGreaterThan(0);
        locationInfo.EndLine.ShouldBeGreaterThan(locationInfo.StartLine);
        (locationInfo.EndLine - locationInfo.StartLine).ShouldBeGreaterThan(5); // Multi-line method
    }
}