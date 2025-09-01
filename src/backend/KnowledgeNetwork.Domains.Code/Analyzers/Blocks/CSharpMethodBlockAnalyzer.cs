using KnowledgeNetwork.Domains.Code.Analyzers.Blocks.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using KnowledgeNetwork.Domains.Code.Models.Blocks;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Blocks;

/// <summary>
/// Analyzer for extracting basic block-level control flow graphs from individual C# methods.
/// This analyzer coordinates two focused services for method-level CFG analysis:
/// - IRoslynCfgExtractor: Handles direct method-to-CFG extraction using Roslyn APIs
/// - IDomainModelConverter: Converts Roslyn CFG to our domain model
/// </summary>
public class CSharpMethodBlockAnalyzer : ICSharpMethodBlockAnalyzer
{
    private readonly IRoslynCfgExtractor _cfgExtractor;
    private readonly IDomainModelConverter _domainConverter;
    private readonly ILogger<CSharpMethodBlockAnalyzer> _logger;

    /// <summary>
    /// Initializes a new instance of the CSharpMethodBlockAnalyzer with composed services
    /// </summary>
    /// <param name="cfgExtractor">Service for extracting ControlFlowGraph directly from methods</param>
    /// <param name="domainConverter">Service for converting to domain models</param>
    /// <param name="logger">Logger instance for diagnostic output</param>
    public CSharpMethodBlockAnalyzer(
        IRoslynCfgExtractor cfgExtractor,
        IDomainModelConverter domainConverter,
        ILogger<CSharpMethodBlockAnalyzer> logger)
    {
        _cfgExtractor = cfgExtractor ?? throw new ArgumentNullException(nameof(cfgExtractor));
        _domainConverter = domainConverter ?? throw new ArgumentNullException(nameof(domainConverter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Extract control flow graph from a method body using the refined 2-step pipeline
    /// </summary>
    /// <param name="compilation">Compilation context</param>
    /// <param name="methodDeclaration">Method syntax node</param>
    /// <returns>Control flow graph or null if extraction fails</returns>
    public async Task<MethodBlockGraph?> ExtractControlFlowAsync(
        Compilation compilation,
        MethodDeclarationSyntax methodDeclaration)
    {
        try
        {
            _logger.LogDebug("Processing method {MethodName} for CFG extraction", methodDeclaration.Identifier);

            // Phase 1: Extract ControlFlowGraph directly from method using RoslynCfgExtractor
            var cfg = await _cfgExtractor.ExtractControlFlowGraphAsync(compilation, methodDeclaration);
            if (cfg == null)
            {
                _logger.LogDebug("No valid CFG extracted for method {MethodName}", 
                    methodDeclaration.Identifier);
                return null;
            }

            // Phase 2: Convert to domain model using specialized service
            var semanticModel = compilation.GetSemanticModel(methodDeclaration.SyntaxTree);
            var methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration);
            if (methodSymbol == null)
            {
                _logger.LogWarning("Failed to get method symbol for {MethodName}", methodDeclaration.Identifier);
                return null;
            }

            var domainModel = await _domainConverter.ConvertToDomainModelAsync(cfg, methodDeclaration, methodSymbol);
            
            _logger.LogDebug("Successfully extracted CFG for method {MethodName}: {BlockCount} blocks, {EdgeCount} edges",
                methodDeclaration.Identifier, domainModel.BasicBlocks.Count, domainModel.Edges.Count);

            return domainModel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CFG extraction failed for method {MethodName}", methodDeclaration.Identifier);
            return null;
        }
    }

    /// <summary>
    /// Extract control flow graphs for all methods in a syntax tree using composed services
    /// </summary>
    /// <param name="compilation">Compilation context</param>
    /// <param name="syntaxTree">Syntax tree to analyze</param>
    /// <returns>List of control flow graphs</returns>
    public async Task<List<MethodBlockGraph>> ExtractAllControlFlowsAsync(
        Compilation compilation,
        SyntaxTree syntaxTree)
    {
        var cfgs = new List<MethodBlockGraph>();

        try
        {
            _logger.LogDebug("Starting CFG extraction for syntax tree");
            var root = await syntaxTree.GetRootAsync();

            // Find all method declarations
            var methods = root.DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(m => m.Body != null || m.ExpressionBody != null)
                .ToList();

            _logger.LogDebug("Found {MethodCount} methods to analyze", methods.Count);

            // Extract CFG for each method using the main extraction method
            foreach (var method in methods)
            {
                var cfg = await ExtractControlFlowAsync(compilation, method);
                if (cfg != null)
                {
                    cfgs.Add(cfg);
                }
            }

            // Also handle constructors using specialized extraction
            var constructors = root.DescendantNodes()
                .OfType<ConstructorDeclarationSyntax>()
                .Where(c => c.Body != null || c.ExpressionBody != null)
                .ToList();

            foreach (var constructor in constructors)
            {
                var cfg = await ExtractControlFlowFromConstructorAsync(compilation, constructor);
                if (cfg != null)
                {
                    cfgs.Add(cfg);
                }
            }

            _logger.LogDebug("Completed CFG extraction for syntax tree: {CfgCount} CFGs extracted", cfgs.Count);
            return cfgs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CFG extraction failed for syntax tree, returning partial results");
            return cfgs; // Return partial results
        }
    }

    /// <summary>
    /// Extract CFG from constructor declaration using the refined 2-step pipeline
    /// </summary>
    private async Task<MethodBlockGraph?> ExtractControlFlowFromConstructorAsync(
        Compilation compilation,
        ConstructorDeclarationSyntax constructorDeclaration)
    {
        try
        {
            _logger.LogDebug("Processing constructor for CFG extraction");

            // Phase 1: Extract ControlFlowGraph directly from constructor using RoslynCfgExtractor
            var cfg = await _cfgExtractor.ExtractControlFlowGraphAsync(compilation, constructorDeclaration);
            if (cfg == null)
            {
                _logger.LogDebug("No valid CFG extracted for constructor");
                return null;
            }

            // Phase 2: Convert to domain model using specialized constructor service
            var semanticModel = compilation.GetSemanticModel(constructorDeclaration.SyntaxTree);
            var constructorSymbol = semanticModel.GetDeclaredSymbol(constructorDeclaration);
            if (constructorSymbol == null)
            {
                _logger.LogWarning("Failed to get constructor symbol");
                return null;
            }

            var domainModel = await _domainConverter.ConvertConstructorToDomainModelAsync(cfg, constructorDeclaration, constructorSymbol);

            _logger.LogDebug("Successfully extracted CFG for constructor: {BlockCount} blocks, {EdgeCount} edges",
                domainModel.BasicBlocks.Count, domainModel.Edges.Count);

            return domainModel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CFG extraction failed for constructor");
            return null;
        }
    }
}