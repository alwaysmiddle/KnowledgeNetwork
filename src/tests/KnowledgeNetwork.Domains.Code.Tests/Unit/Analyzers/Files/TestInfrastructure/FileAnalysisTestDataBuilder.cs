using Microsoft.CodeAnalysis;
using KnowledgeNetwork.Tests.Shared;

namespace KnowledgeNetwork.Domains.Code.Tests.Unit.Analyzers.Files.TestInfrastructure;

/// <summary>
/// Builder pattern for creating test compilations with various file analysis scenarios
/// </summary>
public class FileAnalysisTestDataBuilder
{
    private readonly Dictionary<string, string> _sources = new();
    private string _assemblyName = "TestAssembly";

    /// <summary>
    /// Create a new builder instance
    /// </summary>
    public static FileAnalysisTestDataBuilder Create() => new();

    /// <summary>
    /// Set the assembly name for the compilation
    /// </summary>
    public FileAnalysisTestDataBuilder WithAssemblyName(string assemblyName)
    {
        _assemblyName = assemblyName;
        return this;
    }

    /// <summary>
    /// Add a C# file with the specified content
    /// </summary>
    public FileAnalysisTestDataBuilder WithFile(string fileName, string content)
    {
        _sources[fileName] = content;
        return this;
    }

    /// <summary>
    /// Add a simple class file with basic structure
    /// </summary>
    public FileAnalysisTestDataBuilder WithSimpleClass(string fileName = "SimpleClass.cs", string className = "SimpleClass", string namespaceName = "TestNamespace")
    {
        var content = $$"""
using System;

namespace {{namespaceName}}
{
    public class {{className}}
    {
        public void Method()
        {
            Console.WriteLine("Hello");
        }
    }
}
""";
        return WithFile(fileName, content);
    }

    /// <summary>
    /// Add files with cross-dependencies between them
    /// </summary>
    public FileAnalysisTestDataBuilder WithCrossDependentFiles()
    {
        var fileA = """
using System;
using TestProject.Models;

namespace TestProject.Services
{
    public class ServiceA
    {
        public void ProcessUser(User user)
        {
            Console.WriteLine($"Processing {user.Name}");
        }
    }
}
""";

        var fileB = """
using System;

namespace TestProject.Models
{
    public class User
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }
}
""";

        return WithFile("ServiceA.cs", fileA)
               .WithFile("User.cs", fileB);
    }

    /// <summary>
    /// Add files with circular dependencies
    /// </summary>
    public FileAnalysisTestDataBuilder WithCircularDependencies()
    {
        var classA = """
using TestProject.B;

namespace TestProject.A
{
    public class ClassA
    {
        public ClassB CreateB() => new ClassB();
    }
}
""";

        var classB = """
using TestProject.A;

namespace TestProject.B
{
    public class ClassB
    {
        public ClassA CreateA() => new ClassA();
    }
}
""";

        return WithFile("ClassA.cs", classA)
               .WithFile("ClassB.cs", classB);
    }

    /// <summary>
    /// Add files with various using directive types
    /// </summary>
    public FileAnalysisTestDataBuilder WithComplexUsingDirectives()
    {
        var content = """
global using System;
global using static System.Math;
using System.Collections.Generic;
using System.Linq;
using static System.Console;
using JsonAlias = System.Text.Json;

namespace TestProject.Complex
{
    public class ComplexClass
    {
        public void Method()
        {
            WriteLine("Hello");
            var result = Sqrt(16);
            var list = new List<string>();
            var filtered = list.Where(x => x.Length > 0);
            JsonAlias.JsonSerializer.Serialize(list);
        }
    }
}
""";
        return WithFile("ComplexUsings.cs", content);
    }

    /// <summary>
    /// Add files with file-scoped namespaces
    /// </summary>
    public FileAnalysisTestDataBuilder WithFileScopedNamespace()
    {
        var content = """
using System;

namespace TestProject.FileScoped;

public class FileScopedClass
{
    public void Method()
    {
        Console.WriteLine("File-scoped namespace");
    }
}
""";
        return WithFile("FileScopedClass.cs", content);
    }

    /// <summary>
    /// Add files with nested namespaces and types
    /// </summary>
    public FileAnalysisTestDataBuilder WithNestedStructures()
    {
        var content = """
using System;

namespace TestProject.Outer
{
    namespace Inner
    {
        public class OuterInnerClass
        {
            public class NestedClass
            {
                public enum NestedEnum
                {
                    Value1,
                    Value2
                }
            }
        }
    }
}
""";
        return WithFile("NestedStructures.cs", content);
    }

    /// <summary>
    /// Add files with external assembly references
    /// </summary>
    public FileAnalysisTestDataBuilder WithExternalReferences()
    {
        var content = """
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging; // External

namespace TestProject.External
{
    public class ExternalReferencingClass
    {
        private readonly ILogger<ExternalReferencingClass> _logger;

        public ExternalReferencingClass(ILogger<ExternalReferencingClass> logger)
        {
            _logger = logger;
        }

        public async Task<List<T>> ProcessAsync<T>(IEnumerable<T> items)
        {
            _logger.LogInformation("Processing items");
            return await Task.FromResult(items.ToList());
        }
    }
}
""";
        return WithFile("ExternalReferences.cs", content);
    }

    /// <summary>
    /// Add an empty file
    /// </summary>
    public FileAnalysisTestDataBuilder WithEmptyFile(string fileName = "Empty.cs")
    {
        return WithFile(fileName, "");
    }

    /// <summary>
    /// Add a file with only comments
    /// </summary>
    public FileAnalysisTestDataBuilder WithCommentOnlyFile(string fileName = "Comments.cs")
    {
        var content = """
// This file contains only comments
/* Multi-line comment
   spanning multiple lines */
/// XML documentation comment
""";
        return WithFile(fileName, content);
    }

    /// <summary>
    /// Build the compilation from all added files
    /// </summary>
    public (Compilation compilation, SyntaxTree[] syntaxTrees) Build()
    {
        if (_sources.Count == 0)
        {
            throw new InvalidOperationException("No source files have been added to the builder");
        }

        return CompilationFactory.CreateMultiFile(_sources, _assemblyName);
    }

    /// <summary>
    /// Build a single-file compilation (throws if multiple files were added)
    /// </summary>
    public (Compilation compilation, SyntaxTree syntaxTree) BuildSingle()
    {
        if (_sources.Count != 1)
        {
            throw new InvalidOperationException($"Expected exactly one source file, but found {_sources.Count}");
        }

        var kvp = _sources.First();
        return CompilationFactory.CreateBasic(kvp.Value, _assemblyName);
    }

    /// <summary>
    /// Get the number of files that have been added
    /// </summary>
    public int FileCount => _sources.Count;

    /// <summary>
    /// Get the file names that have been added
    /// </summary>
    public IEnumerable<string> FileNames => _sources.Keys;
}