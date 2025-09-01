using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace KnowledgeNetwork.Tests.Shared;

/// <summary>
/// Factory for creating test compilations with proper references and configuration.
/// Provides reusable compilation creation for all test projects.
/// </summary>
public static class CompilationFactory
{
    /// <summary>
    /// Create a basic compilation with essential .NET references
    /// </summary>
    /// <param name="code">C# source code to compile</param>
    /// <param name="assemblyName">Name for the test assembly (defaults to "TestAssembly")</param>
    /// <returns>Compilation and syntax tree tuple</returns>
    public static (Compilation compilation, SyntaxTree syntaxTree) CreateBasic(
        string code, 
        string assemblyName = "TestAssembly")
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var compilation = CSharpCompilation.Create(assemblyName)
            .AddReferences(GetBasicReferences())
            .AddSyntaxTrees(syntaxTree);
        
        return (compilation, syntaxTree);
    }

    /// <summary>
    /// Create compilation with multiple source files
    /// </summary>
    /// <param name="sources">Dictionary of filename -> source code</param>
    /// <param name="assemblyName">Name for the test assembly</param>
    /// <returns>Compilation with all syntax trees</returns>
    public static (Compilation compilation, SyntaxTree[] syntaxTrees) CreateMultiFile(
        Dictionary<string, string> sources,
        string assemblyName = "TestAssembly")
    {
        var syntaxTrees = sources.Select(kvp => 
            CSharpSyntaxTree.ParseText(kvp.Value, path: kvp.Key)).ToArray();
            
        var compilation = CSharpCompilation.Create(assemblyName)
            .AddReferences(GetBasicReferences())
            .AddSyntaxTrees(syntaxTrees);
            
        return (compilation, syntaxTrees);
    }

    /// <summary>
    /// Create compilation with extended references (includes LINQ, Collections, etc.)
    /// </summary>
    /// <param name="code">C# source code to compile</param>
    /// <param name="assemblyName">Name for the test assembly</param>
    /// <returns>Compilation and syntax tree tuple</returns>
    public static (Compilation compilation, SyntaxTree syntaxTree) CreateExtended(
        string code,
        string assemblyName = "TestAssembly")
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var compilation = CSharpCompilation.Create(assemblyName)
            .AddReferences(GetExtendedReferences())
            .AddSyntaxTrees(syntaxTree);
        
        return (compilation, syntaxTree);
    }

    /// <summary>
    /// Create compilation and verify no compilation errors exist
    /// </summary>
    /// <param name="code">C# source code to compile</param>
    /// <param name="assemblyName">Name for the test assembly</param>
    /// <returns>Compilation and syntax tree if successful</returns>
    /// <exception cref="InvalidOperationException">Thrown if compilation has errors</exception>
    public static (Compilation compilation, SyntaxTree syntaxTree) CreateValidated(
        string code,
        string assemblyName = "TestAssembly")
    {
        var (compilation, syntaxTree) = CreateBasic(code, assemblyName);
        
        var diagnostics = compilation.GetDiagnostics();
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        
        if (errors.Any())
        {
            var errorMessages = string.Join(Environment.NewLine, errors.Select(e => e.ToString()));
            throw new InvalidOperationException($"Compilation has errors:{Environment.NewLine}{errorMessages}");
        }
        
        return (compilation, syntaxTree);
    }

    /// <summary>
    /// Get basic .NET references for compilation
    /// </summary>
    private static IEnumerable<MetadataReference> GetBasicReferences()
    {
        return
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location)
        ];
    }

    /// <summary>
    /// Get extended .NET references including LINQ, Collections, etc.
    /// </summary>
    private static IEnumerable<MetadataReference> GetExtendedReferences()
    {
        return
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Collections.Generic.List<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Threading.Tasks.Task).Assembly.Location)
        ];
    }
}