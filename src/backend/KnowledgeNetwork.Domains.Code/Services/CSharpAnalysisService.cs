using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using KnowledgeNetwork.Domains.Code.Models.Analysis;

namespace KnowledgeNetwork.Domains.Code.Services;

/// <summary>
/// Service for analyzing C# code using Microsoft Roslyn compiler APIs.
/// Encapsulates all Roslyn-specific logic for syntax tree parsing and semantic analysis.
/// </summary>
public class CSharpAnalysisService
{
    private readonly ControlFlowAnalyzer _controlFlowAnalyzer = new();

    /// <summary>
    /// Language identifier for this service
    /// </summary>
    public string LanguageId => "csharp";

    /// <summary>
    /// Analyze C# code and extract comprehensive information
    /// </summary>
    /// <param name="code">C# source code to analyze</param>
    /// <returns>Analysis result containing extracted information</returns>
    public async Task<CodeAnalysisResult> AnalyzeAsync(string code)
    {
        try
        {
            // Parse the source code into a syntax tree
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            
            // Check for syntax errors
            var diagnostics = syntaxTree.GetDiagnostics();
            var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            
            if (errors.Any())
            {
                return new CodeAnalysisResult
                {
                    Success = false,
                    Errors = errors.Select(e => e.ToString()).ToList(),
                    LanguageId = LanguageId
                };
            }

            // Get the root syntax node
            var root = await syntaxTree.GetRootAsync();
            
            // Extract basic information
            var result = new CodeAnalysisResult
            {
                Success = true,
                LanguageId = LanguageId,
                SyntaxTree = syntaxTree,
                Classes = ExtractClasses(root),
                Methods = ExtractMethods(root),
                Properties = ExtractProperties(root),
                UsingStatements = ExtractUsingStatements(root)
            };

            return result;
        }
        catch (Exception ex)
        {
            return new CodeAnalysisResult
            {
                Success = false,
                Errors = new List<string> { $"Analysis failed: {ex.Message}" },
                LanguageId = LanguageId
            };
        }
    }

    /// <summary>
    /// Create a compilation unit for semantic analysis
    /// </summary>
    /// <param name="syntaxTree">The syntax tree to create compilation for</param>
    /// <returns>Compilation unit for semantic analysis</returns>
    public CSharpCompilation CreateCompilation(SyntaxTree syntaxTree)
    {
        // Create compilation with essential references
        var compilation = CSharpCompilation.Create(
            assemblyName: "AnalysisAssembly",
            syntaxTrees: new[] { syntaxTree },
            references: GetBasicReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        return compilation;
    }

    /// <summary>
    /// Extract control flow graph from C# code
    /// </summary>
    /// <param name="code">C# source code to analyze</param>
    /// <returns>List of control flow graphs for all methods</returns>
    public async Task<List<Models.ControlFlowGraph>> ExtractControlFlowAsync(string code)
    {
        try
        {
            // Parse the source code into a syntax tree
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            
            // Create compilation for semantic analysis
            var compilation = CreateCompilation(syntaxTree);
            
            // Extract CFGs for all methods
            var cfgs = await _controlFlowAnalyzer.ExtractAllControlFlowsAsync(compilation, syntaxTree);
            
            return cfgs;
        }
        catch (Exception ex)
        {
            // Log error but return empty list
            System.Diagnostics.Debug.WriteLine($"CFG extraction failed: {ex.Message}");
            return new List<Models.ControlFlowGraph>();
        }
    }

    /// <summary>
    /// Check if the service is healthy and can perform analysis
    /// </summary>
    /// <returns>True if service is operational</returns>
    public Task<bool> IsHealthyAsync()
    {
        try
        {
            // Quick test - parse a simple piece of code
            var testCode = "class Test { }";
            var syntaxTree = CSharpSyntaxTree.ParseText(testCode);
            var diagnostics = syntaxTree.GetDiagnostics();
            
            // Service is healthy if we can parse without exceptions
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Extract class declarations from syntax tree
    /// </summary>
    private List<ClassInfo> ExtractClasses(SyntaxNode root)
    {
        return root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Select(c => new ClassInfo
            {
                Name = c.Identifier.ValueText,
                Namespace = GetNamespace(c),
                Modifiers = c.Modifiers.ToString(),
                LineNumber = c.GetLocation().GetLineSpan().StartLinePosition.Line + 1
            })
            .ToList();
    }

    /// <summary>
    /// Extract method declarations from syntax tree
    /// </summary>
    private List<MethodInfo> ExtractMethods(SyntaxNode root)
    {
        return root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Select(m => new MethodInfo
            {
                Name = m.Identifier.ValueText,
                ReturnType = m.ReturnType.ToString(),
                Modifiers = m.Modifiers.ToString(),
                Parameters = m.ParameterList.Parameters.Select(p => $"{p.Type} {p.Identifier}").ToList(),
                LineNumber = m.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                ClassName = GetContainingClass(m)
            })
            .ToList();
    }

    /// <summary>
    /// Extract property declarations from syntax tree
    /// </summary>
    private List<PropertyInfo> ExtractProperties(SyntaxNode root)
    {
        return root.DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .Select(p => new PropertyInfo
            {
                Name = p.Identifier.ValueText,
                Type = p.Type.ToString(),
                Modifiers = p.Modifiers.ToString(),
                HasGetter = p.AccessorList?.Accessors.Any(a => a.Keyword.IsKind(SyntaxKind.GetKeyword)) ?? false,
                HasSetter = p.AccessorList?.Accessors.Any(a => a.Keyword.IsKind(SyntaxKind.SetKeyword)) ?? false,
                LineNumber = p.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                ClassName = GetContainingClass(p)
            })
            .ToList();
    }

    /// <summary>
    /// Extract using statements from syntax tree
    /// </summary>
    private List<string> ExtractUsingStatements(SyntaxNode root)
    {
        return root.DescendantNodes()
            .OfType<UsingDirectiveSyntax>()
            .Select(u => u.Name?.ToString() ?? string.Empty)
            .Where(name => !string.IsNullOrEmpty(name))
            .ToList();
    }

    /// <summary>
    /// Get the namespace containing a syntax node
    /// </summary>
    private string GetNamespace(SyntaxNode node)
    {
        var namespaceDeclaration = node.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
        return namespaceDeclaration?.Name.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Get the class containing a syntax node
    /// </summary>
    private string GetContainingClass(SyntaxNode node)
    {
        var classDeclaration = node.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        return classDeclaration?.Identifier.ValueText ?? string.Empty;
    }

    /// <summary>
    /// Get basic references needed for compilation
    /// </summary>
    private IEnumerable<MetadataReference> GetBasicReferences()
    {
        // Add basic .NET references
        return new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location)
        };
    }

    #endregion
}

#region Data Models

/// <summary>
/// Result of C# code analysis
/// </summary>
public class CodeAnalysisResult
{
    public bool Success { get; set; }
    public string LanguageId { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
    public SyntaxTree? SyntaxTree { get; set; }
    public List<ClassInfo> Classes { get; set; } = new();
    public List<MethodInfo> Methods { get; set; } = new();
    public List<PropertyInfo> Properties { get; set; } = new();
    public List<string> UsingStatements { get; set; } = new();
}

/// <summary>
/// Information about a class declaration
/// </summary>
public class ClassInfo
{
    public string Name { get; set; } = string.Empty;
    public string Namespace { get; set; } = string.Empty;
    public string Modifiers { get; set; } = string.Empty;
    public int LineNumber { get; set; }
}

/// <summary>
/// Information about a method declaration
/// </summary>
public class MethodInfo
{
    public string Name { get; set; } = string.Empty;
    public string ReturnType { get; set; } = string.Empty;
    public string Modifiers { get; set; } = string.Empty;
    public List<string> Parameters { get; set; } = new();
    public int LineNumber { get; set; }
    public string ClassName { get; set; } = string.Empty;
}

/// <summary>
/// Information about a property declaration
/// </summary>
public class PropertyInfo
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Modifiers { get; set; } = string.Empty;
    public bool HasGetter { get; set; }
    public bool HasSetter { get; set; }
    public int LineNumber { get; set; }
    public string ClassName { get; set; } = string.Empty;
}

#endregion