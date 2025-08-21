using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using KnowledgeNetwork.Core.Models.Core;
using KnowledgeNetwork.Core.Models.Constants;
using KnowledgeNetwork.Domains.Code.Models;
using KnowledgeNetwork.Domains.Code.Models.Analysis;
using KnowledgeNetwork.Domains.Code.Converters;

namespace KnowledgeNetwork.Domains.Code.Services;

/// <summary>
/// Service for analyzing C# code using Microsoft Roslyn compiler APIs.
/// Encapsulates all Roslyn-specific logic for syntax tree parsing and semantic analysis.
/// </summary>
public class CSharpAnalysisService
{
    private readonly CSharpControlFlowAnalyzer _controlFlowAnalyzer = new();
    private readonly CfgToKnowledgeNodeConverter _cfgConverter = new();

    /// <summary>
    /// Language identifier for this service
    /// </summary>
    public string LanguageId => "csharp";

    /// <summary>
    /// Analyze C# code and extract comprehensive information
    /// </summary>
    /// <param name="code">C# source code to analyze</param>
    /// <returns>Analysis result containing extracted information</returns>
    public async Task<CSharpCodeAnalysisResult> AnalyzeAsync(string code)
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
                return new CSharpCodeAnalysisResult
                {
                    Success = false,
                    Errors = errors.Select(e => e.ToString()).ToList(),
                    LanguageId = LanguageId
                };
            }

            // Get the root syntax node
            var root = await syntaxTree.GetRootAsync();

            // Extract basic information
            var result = new CSharpCodeAnalysisResult
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
            return new CSharpCodeAnalysisResult
            {
                Success = false,
                Errors = [$"Analysis failed: {ex.Message}"],
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
    public async Task<List<CSharpControlFlowGraph>> ExtractControlFlowAsync(string code)
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
            // System.Diagnostics.Debug.WriteLine($"CFG extraction failed: {ex.Message}");
            return [];
        }
    }
    /// <summary>
    /// Extract control flow graph from C# code and convert to unified KnowledgeNode format
    /// </summary>
    /// <param name="code">C# source code to analyze</param>
    /// <param name="includeOperations">Whether to include operation nodes within basic blocks</param>
    /// <returns>CSharpCfgAnalysisResult containing nodes and edges</returns>
    public async Task<CSharpCfgAnalysisResult> AnalyzeControlFlowAsync(string code, bool includeOperations = true)
    {
        var result = new CSharpCfgAnalysisResult();
        var startTime = DateTime.UtcNow;

        try
        {
            // Extract CFGs using the real analyzer (now fixed!)
            var cfgs = await ExtractControlFlowAsync(code);

            if (cfgs.Count == 0)
            {
                result.Success = false;
                result.Errors.Add("No control flow graphs could be extracted from the provided code");
                return result;
            }

            // Convert each CFG to nodes and edges using the new converter method
            foreach (var cfg in cfgs)
            {
                var graphData = _cfgConverter.ConvertCfgToGraph(cfg, includeOperations);
                result.Nodes.AddRange(graphData.Nodes);
                result.Edges.AddRange(graphData.Edges);
            }

            result.Success = true;
            result.Duration = DateTime.UtcNow - startTime;

            // Add metadata
            result.Metadata = new Dictionary<string, object?>
            {
                ["totalMethods"] = cfgs.Count,
                ["totalBlocks"] = cfgs.Sum(cfg => cfg.BasicBlocks.Count),
                ["totalOperations"] = result.Nodes.Count(n => n.Type.Primary == "operation"),
                ["analysisTime"] = result.Duration?.TotalMilliseconds,
                ["includeOperations"] = includeOperations
            };

            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Errors.Add($"CFG analysis failed: {ex.Message}");
            result.Duration = DateTime.UtcNow - startTime;
            return result;
        }
    }/// <summary>
     /// Extract a single method's control flow as a KnowledgeNode structure
     /// </summary>
     /// <param name="code">C# source code to analyze</param>
     /// <param name="methodName">Name of the specific method to analyze</param>
     /// <param name="includeOperations">Whether to include operation nodes within basic blocks</param>
     /// <returns>KnowledgeNode representing the method or null if not found</returns>
    public async Task<KnowledgeNode?> AnalyzeMethodControlFlowAsync(string code, string methodName,
        bool includeOperations = true)
    {
        try
        {
            var analysisResult = await AnalyzeControlFlowAsync(code, includeOperations);

            if (!analysisResult.Success)
            {
                return null;
            }

            // Find the method node
            var methodNode = analysisResult.Nodes.FirstOrDefault(n =>
                n.Type.Primary == PrimaryNodeType.Method &&
                n.Label.Equals(methodName, StringComparison.OrdinalIgnoreCase));

            return methodNode;
        }
        catch (Exception ex)
        {
            // Log error and return null
            return null;
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
    private List<CSharpClassInfo> ExtractClasses(SyntaxNode root)
    {
        return root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Select(c => new CSharpClassInfo
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
    private List<CSharpMethodInfo> ExtractMethods(SyntaxNode root)
    {
        return root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Select(m => new CSharpMethodInfo
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
    private List<CSharpPropertyInfo> ExtractProperties(SyntaxNode root)
    {
        return root.DescendantNodes()
            .OfType<PropertyDeclarationSyntax>()
            .Select(p => new CSharpPropertyInfo
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
    private string GetNamespace(SyntaxNode node) =>
        node.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString() ?? string.Empty;

    /// <summary>
    /// Get the class containing a syntax node
    /// </summary>
    private string GetContainingClass(SyntaxNode node) =>
        node.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault()?.Identifier.ValueText ?? string.Empty;

    /// <summary>
    /// Get basic references needed for compilation
    /// </summary>
    private IEnumerable<MetadataReference> GetBasicReferences()
    {
        // Add basic .NET references
        return
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location)
        ];
    }
    /// <summary>
    /// Create a simple mock CFG for testing the edge-centric pipeline
    /// </summary>
    /// <param name="code">C# source code (used for basic analysis)</param>
    /// <returns>Simple mock CFG with basic blocks and edges</returns>
    private CSharpControlFlowGraph CreateSimpleMockCfg(string code)
    {
        var cfg = new CSharpControlFlowGraph
        {
            MethodName = "MockMethod",
            TypeName = "MockClass",
            Location = new KnowledgeNetwork.Domains.Code.Models.Common.CSharpLocationInfo
            {
                StartLine = 1,
                StartColumn = 1,
                EndLine = 10,
                EndColumn = 1
            }
        };

        // Create a simple 3-block CFG: Entry -> Conditional -> Exit
        var entryBlock = new KnowledgeNetwork.Domains.Code.Models.CSharpBasicBlock
        {
            Id = 0,
            Ordinal = 0,
            Kind = KnowledgeNetwork.Domains.Code.Models.Enums.CSharpBasicBlockKind.Entry,
            IsReachable = true
        };

        var conditionalBlock = new KnowledgeNetwork.Domains.Code.Models.CSharpBasicBlock
        {
            Id = 1,
            Ordinal = 1,
            Kind = KnowledgeNetwork.Domains.Code.Models.Enums.CSharpBasicBlockKind.Block,
            IsReachable = true,
            BranchInfo = new KnowledgeNetwork.Domains.Code.Models.ControlFlow.CSharpBranchInfo
            {
                Condition = "x > 0",
                BranchType = KnowledgeNetwork.Domains.Code.Models.Enums.CSharpBranchType.Conditional
            }
        };

        var exitBlock = new KnowledgeNetwork.Domains.Code.Models.CSharpBasicBlock
        {
            Id = 2,
            Ordinal = 2,
            Kind = KnowledgeNetwork.Domains.Code.Models.Enums.CSharpBasicBlockKind.Exit,
            IsReachable = true
        };

        // Add some mock operations
        entryBlock.Operations.Add(new KnowledgeNetwork.Domains.Code.Models.ControlFlow.CSharpOperationInfo
        {
            OperationKind = "VariableDeclarator",
            Syntax = "int x",
            Summary = "var x"
        });

        conditionalBlock.Operations.Add(new KnowledgeNetwork.Domains.Code.Models.ControlFlow.CSharpOperationInfo
        {
            OperationKind = "BinaryOperator",
            Syntax = "x > 0",
            Summary = "x > 0"
        });

        // Set up relationships
        entryBlock.Successors.Add(1);
        conditionalBlock.Predecessors.Add(0);
        conditionalBlock.Successors.Add(2);
        exitBlock.Predecessors.Add(1);

        cfg.BasicBlocks.Add(entryBlock);
        cfg.BasicBlocks.Add(conditionalBlock);
        cfg.BasicBlocks.Add(exitBlock);
        cfg.EntryBlock = entryBlock;
        cfg.ExitBlock = exitBlock;

        // Create edges
        var edge1 = new KnowledgeNetwork.Domains.Code.Models.CSharpControlFlowEdge
        {
            Source = 0,
            Target = 1,
            Kind = KnowledgeNetwork.Domains.Code.Models.Enums.CSharpEdgeKind.Regular,
            Label = "entry"
        };

        var edge2 = new KnowledgeNetwork.Domains.Code.Models.CSharpControlFlowEdge
        {
            Source = 1,
            Target = 2,
            Kind = KnowledgeNetwork.Domains.Code.Models.Enums.CSharpEdgeKind.ConditionalTrue,
            Label = "condition",
            Condition = new KnowledgeNetwork.Domains.Code.Models.ControlFlow.CSharpEdgeCondition
            {
                BooleanValue = true,
                Description = "x > 0"
            }
        };

        cfg.Edges.Add(edge1);
        cfg.Edges.Add(edge2);

        // Mock complexity metrics
        cfg.Metrics = new KnowledgeNetwork.Domains.Code.Models.Common.CSharpComplexityMetrics
        {
            BlockCount = 3,
            EdgeCount = 2,
            CyclomaticComplexity = 2,
            DecisionPoints = 1,
            LoopCount = 0,
            HasExceptionHandling = false
        };

        return cfg;
    }
    #endregion
}