using KnowledgeNetwork.Domains.Code.Analyzers.Files.Utilities;
using KnowledgeNetwork.Domains.Code.Models.Files;
using KnowledgeNetwork.Tests.Shared;
using Shouldly;
using Xunit;

namespace KnowledgeNetwork.Domains.Code.Tests.Unit.Analyzers.Files.Utilities;

public class FileMetricsCalculatorTests
{
    private readonly FileMetricsCalculator _calculator;

    public FileMetricsCalculatorTests()
    {
        _calculator = new FileMetricsCalculator();
    }

    [Fact]
    public void CalculateFileMetrics_WithSimpleClass_ShouldReturnBasicMetrics()
    {
        // Arrange
        var testCode = """
using System;

namespace TestNamespace
{
    public class SimpleClass
    {
        public void Method()
        {
            Console.WriteLine("Hello");
        }
    }
}
""";

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(testCode);
        var root = syntaxTree.GetRoot();
        var fileNode = CreateTestFileNode();

        // Act
        var metrics = _calculator.CalculateFileMetrics(fileNode, root);

        // Assert
        metrics.ShouldNotBeNull();
        metrics.LinesOfCode.ShouldBeGreaterThan(0);
        metrics.TotalLines.ShouldBeGreaterThan(metrics.LinesOfCode);
        metrics.DeclaredTypeCount.ShouldBe(1);
        metrics.UsingDirectiveCount.ShouldBe(1);
        metrics.CyclomaticComplexity.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void CalculateFileMetrics_WithEmptyFile_ShouldReturnZeroMetrics()
    {
        // Arrange
        var testCode = "";
        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(testCode);
        var root = syntaxTree.GetRoot();
        var fileNode = CreateTestFileNode();

        // Act
        var metrics = _calculator.CalculateFileMetrics(fileNode, root);

        // Assert
        metrics.ShouldNotBeNull();
        metrics.LinesOfCode.ShouldBe(0);
        metrics.TotalLines.ShouldBe(1); // Empty file still has one line
        metrics.CommentLines.ShouldBe(0);
        metrics.BlankLines.ShouldBe(0);
        metrics.DeclaredTypeCount.ShouldBe(0);
        metrics.UsingDirectiveCount.ShouldBe(0);
        metrics.CyclomaticComplexity.ShouldBe(0);
    }

    [Fact]
    public void CalculateFileMetrics_WithOnlyComments_ShouldCountCommentLines()
    {
        // Arrange
        var testCode = """
// This is a single line comment
/* This is a
   multi-line
   comment */
/// <summary>
/// XML documentation comment
/// </summary>
""";

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(testCode);
        var root = syntaxTree.GetRoot();
        var fileNode = CreateTestFileNode();

        // Act
        var metrics = _calculator.CalculateFileMetrics(fileNode, root);

        // Assert
        metrics.ShouldNotBeNull();
        metrics.CommentLines.ShouldBeGreaterThan(0);
        metrics.LinesOfCode.ShouldBe(0); // No actual code
        metrics.TotalLines.ShouldBeGreaterThan(0);
        metrics.DeclaredTypeCount.ShouldBe(0);
    }

    [Fact]
    public void CalculateFileMetrics_WithMultipleUsingDirectives_ShouldCountCorrectly()
    {
        // Arrange
        var testCode = """
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
global using Microsoft.Extensions.Logging;

namespace TestNamespace
{
    public class TestClass { }
}
""";

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(testCode);
        var root = syntaxTree.GetRoot();
        var fileNode = CreateTestFileNode();

        // Act
        var metrics = _calculator.CalculateFileMetrics(fileNode, root);

        // Assert
        metrics.ShouldNotBeNull();
        metrics.UsingDirectiveCount.ShouldBe(5); // 4 regular + 1 global using
        metrics.DeclaredTypeCount.ShouldBe(1);
    }

    [Fact]
    public void CalculateFileMetrics_WithMultipleTypes_ShouldCountAllTypes()
    {
        // Arrange
        var testCode = """
namespace TestNamespace
{
    public class TestClass { }

    public interface ITestInterface { }

    public struct TestStruct { }

    public enum TestEnum { Value1, Value2 }

    public delegate void TestDelegate();
}
""";

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(testCode);
        var root = syntaxTree.GetRoot();
        var fileNode = CreateTestFileNode();

        // Act
        var metrics = _calculator.CalculateFileMetrics(fileNode, root);

        // Assert
        metrics.ShouldNotBeNull();
        metrics.DeclaredTypeCount.ShouldBe(5); // class, interface, struct, enum, delegate
    }

    [Fact]
    public void CalculateFileMetrics_WithNestedTypes_ShouldCountAllTypes()
    {
        // Arrange
        var testCode = """
namespace TestNamespace
{
    public class OuterClass
    {
        public class InnerClass
        {
            public enum InnerEnum { A, B, C }
        }

        public interface IInnerInterface { }
    }
}
""";

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(testCode);
        var root = syntaxTree.GetRoot();
        var fileNode = CreateTestFileNode();

        // Act
        var metrics = _calculator.CalculateFileMetrics(fileNode, root);

        // Assert
        metrics.ShouldNotBeNull();
        metrics.DeclaredTypeCount.ShouldBe(4); // OuterClass, InnerClass, InnerEnum, IInnerInterface
    }

    [Fact]
    public void CalculateFileMetrics_WithComplexMethods_ShouldCalculateHigherComplexity()
    {
        // Arrange
        var testCode = """
using System;

namespace TestNamespace
{
    public class ComplexClass
    {
        public void ComplexMethod(int value)
        {
            if (value > 0) // +1
            {
                for (int i = 0; i < value; i++) // +1
                {
                    if (i % 2 == 0) // +1
                    {
                        Console.WriteLine($"Even: {i}");
                    }
                    else // +1
                    {
                        Console.WriteLine($"Odd: {i}");
                    }
                }
            }
            else if (value < 0) // +1
            {
                while (value < 0) // +1
                {
                    value++;
                    if (value == -5) // +1
                        break;
                }
            }
            else // +1
            {
                switch (value) // +1
                {
                    case 0:
                        Console.WriteLine("Zero");
                        break;
                    default:
                        Console.WriteLine("Default");
                        break;
                }
            }
        }

        public void SimpleMethod()
        {
            Console.WriteLine("Simple");
        }
    }
}
""";

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(testCode);
        var root = syntaxTree.GetRoot();
        var fileNode = CreateTestFileNode();

        // Act
        var metrics = _calculator.CalculateFileMetrics(fileNode, root);

        // Assert
        metrics.ShouldNotBeNull();
        metrics.CyclomaticComplexity.ShouldBeGreaterThan(5); // Complex method should increase complexity
        metrics.DeclaredTypeCount.ShouldBe(1);
    }

    [Fact]
    public void CalculateFileMetrics_WithMixedContentAndBlankLines_ShouldCountCorrectly()
    {
        // Arrange
        var testCode = """
using System;

// Comment line

namespace TestNamespace
{
    /// <summary>
    /// XML comment for class
    /// </summary>
    public class TestClass
    {

        // Field comment
        private int _field;


        /// <summary>
        /// Method documentation
        /// </summary>
        public void Method()
        {
            // Inline comment
            Console.WriteLine("Hello");

            // Another comment
        }

    }

}
""";

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(testCode);
        var root = syntaxTree.GetRoot();
        var fileNode = CreateTestFileNode();

        // Act
        var metrics = _calculator.CalculateFileMetrics(fileNode, root);

        // Assert
        metrics.ShouldNotBeNull();
        metrics.CommentLines.ShouldBeGreaterThan(0);
        metrics.BlankLines.ShouldBeGreaterThan(0);
        metrics.LinesOfCode.ShouldBeGreaterThan(0);
        metrics.TotalLines.ShouldBe(metrics.LinesOfCode + metrics.CommentLines + metrics.BlankLines);
    }

    [Fact]
    public void CalculateFileMetrics_WithFileContainingOnlyNamespace_ShouldReturnCorrectMetrics()
    {
        // Arrange
        var testCode = """
namespace TestNamespace
{
}
""";

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(testCode);
        var root = syntaxTree.GetRoot();
        var fileNode = CreateTestFileNode();

        // Act
        var metrics = _calculator.CalculateFileMetrics(fileNode, root);

        // Assert
        metrics.ShouldNotBeNull();
        metrics.DeclaredTypeCount.ShouldBe(0);
        metrics.UsingDirectiveCount.ShouldBe(0);
        metrics.LinesOfCode.ShouldBeGreaterThan(0); // Namespace declaration counts as code
        metrics.CyclomaticComplexity.ShouldBe(0);
    }

    [Fact]
    public void CalculateFileMetrics_WithFileScopedNamespace_ShouldCalculateCorrectly()
    {
        // Arrange
        var testCode = """
using System;

namespace TestNamespace;

public class TestClass
{
    public void Method()
    {
        Console.WriteLine("Hello");
    }
}
""";

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(testCode);
        var root = syntaxTree.GetRoot();
        var fileNode = CreateTestFileNode();

        // Act
        var metrics = _calculator.CalculateFileMetrics(fileNode, root);

        // Assert
        metrics.ShouldNotBeNull();
        metrics.DeclaredTypeCount.ShouldBe(1);
        metrics.UsingDirectiveCount.ShouldBe(1);
        metrics.LinesOfCode.ShouldBeGreaterThan(0);
        metrics.CyclomaticComplexity.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void CalculateFileMetrics_WithLambdaExpressions_ShouldIncludeInComplexity()
    {
        // Arrange
        var testCode = """
using System;
using System.Linq;
using System.Collections.Generic;

namespace TestNamespace
{
    public class LambdaClass
    {
        public void ProcessData()
        {
            var numbers = new List<int> { 1, 2, 3, 4, 5 };

            var evenNumbers = numbers
                .Where(x => x % 2 == 0) // Lambda with condition
                .Select(x => x * 2)
                .ToList();

            var result = numbers.Any(x => x > 3); // Another lambda

            if (result) // +1
            {
                Console.WriteLine("Found numbers greater than 3");
            }
        }
    }
}
""";

        var (compilation, syntaxTree) = CompilationFactory.CreateExtended(testCode);
        var root = syntaxTree.GetRoot();
        var fileNode = CreateTestFileNode();

        // Act
        var metrics = _calculator.CalculateFileMetrics(fileNode, root);

        // Assert
        metrics.ShouldNotBeNull();
        metrics.CyclomaticComplexity.ShouldBeGreaterThan(1); // Should include lambda complexity
        metrics.UsingDirectiveCount.ShouldBe(3);
    }

    [Fact]
    public void CalculateFileMetrics_WithTryCatchBlocks_ShouldIncludeInComplexity()
    {
        // Arrange
        var testCode = """
using System;

namespace TestNamespace
{
    public class ExceptionHandlingClass
    {
        public void HandleExceptions()
        {
            try
            {
                var result = 10 / 0;
            }
            catch (DivideByZeroException ex) // +1
            {
                Console.WriteLine("Division by zero");
            }
            catch (Exception ex) // +1
            {
                Console.WriteLine("General exception");
            }
            finally
            {
                Console.WriteLine("Finally block");
            }
        }
    }
}
""";

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(testCode);
        var root = syntaxTree.GetRoot();
        var fileNode = CreateTestFileNode();

        // Act
        var metrics = _calculator.CalculateFileMetrics(fileNode, root);

        // Assert
        metrics.ShouldNotBeNull();
        metrics.CyclomaticComplexity.ShouldBeGreaterThan(2); // Should include catch blocks
    }

    [Fact]
    public void CalculateFileMetrics_WithRecordTypes_ShouldCountAsTypes()
    {
        // Arrange
        var testCode = """
namespace TestNamespace
{
    public record PersonRecord(string Name, int Age);

    public record struct PersonStruct(string Name, int Age);

    public class RegularClass { }
}
""";

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(testCode);
        var root = syntaxTree.GetRoot();
        var fileNode = CreateTestFileNode();

        // Act
        var metrics = _calculator.CalculateFileMetrics(fileNode, root);

        // Assert
        metrics.ShouldNotBeNull();
        metrics.DeclaredTypeCount.ShouldBe(3); // record, record struct, class
    }

    [Fact]
    public void CalculateFileMetrics_WithVeryLargeFile_ShouldHandleCorrectly()
    {
        // Arrange - Create a larger file to test performance and correctness
        var testCode = """
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestNamespace
{
""";

        // Add multiple classes to simulate a larger file
        for (int i = 0; i < 10; i++)
        {
            testCode += $$"""

    public class TestClass{{i}}
    {
        private int _field{{i}};

        public int Property{{i}} { get; set; }

        public void Method{{i}}()
        {
            if (_field{{i}} > 0)
            {
                for (int j = 0; j < _field{{i}}; j++)
                {
                    Console.WriteLine($"Processing {j}");
                }
            }
        }
    }
""";
        }

        testCode += "\n}";

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(testCode);
        var root = syntaxTree.GetRoot();
        var fileNode = CreateTestFileNode();

        // Act
        var metrics = _calculator.CalculateFileMetrics(fileNode, root);

        // Assert
        metrics.ShouldNotBeNull();
        metrics.DeclaredTypeCount.ShouldBe(10);
        metrics.UsingDirectiveCount.ShouldBe(3);
        metrics.LinesOfCode.ShouldBeGreaterThan(50);
        metrics.CyclomaticComplexity.ShouldBeGreaterThan(20); // Multiple methods with complexity
    }

    private static FileNode CreateTestFileNode()
    {
        return new FileNode
        {
            Id = Guid.NewGuid().ToString(),
            FileName = "TestFile.cs",
            FilePath = @"C:\Test\TestFile.cs",
            FileType = FileType.Source,
            Language = FileLanguage.CSharp
        };
    }
}