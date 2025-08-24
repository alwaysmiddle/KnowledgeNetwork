using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using KnowledgeNetwork.Domains.Code.Analyzers.Blocks;
using KnowledgeNetwork.Domains.Code.Models.Enums;
using KnowledgeNetwork.Tests.Shared;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;
using Xunit;

namespace KnowledgeNetwork.Domains.Code.Tests.Unit.Analyzers.Blocks;

public class CSharpMethodBlockAnalyzerTests
{
    private readonly CSharpMethodBlockAnalyzer _analyzer;

    public CSharpMethodBlockAnalyzerTests()
    {
        // Create logger instances for all services
        var operationExtractorLogger = NullLogger<RoslynOperationExtractor>.Instance;
        var cfgBuilderLogger = NullLogger<CfgStructureBuilder>.Instance;
        var domainConverterLogger = NullLogger<DomainModelConverter>.Instance;
        var analyzerLogger = NullLogger<CSharpMethodBlockAnalyzer>.Instance;

        // Create the composed services that the analyzer depends on
        var operationExtractor = new RoslynOperationExtractor(operationExtractorLogger);
        var cfgBuilder = new CfgStructureBuilder(cfgBuilderLogger);
        var domainConverter = new DomainModelConverter(domainConverterLogger);

        // Create the main analyzer with its dependencies
        _analyzer = new CSharpMethodBlockAnalyzer(
            operationExtractor,
            cfgBuilder,
            domainConverter,
            analyzerLogger);
    }
    
    [Fact]
    public async Task ExtractControlFlowAsync_SimpleLinearMethod_ShouldHaveBasicStructure()
    {
        // Load sample code with simple linear methods
        var sampleCode = SampleCodeRepository.GetSimpleLinearMethod();
        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        
        // Find the SimpleMethod for testing
        var root = await syntaxTree.GetRootAsync(TestContext.Current.CancellationToken);
        var simpleMethod = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "SimpleMethod");
        
        // Extract CFG from the simple method
        var cfg = await _analyzer.ExtractControlFlowAsync(compilation, simpleMethod);
        
        cfg?.Metrics.CyclomaticComplexity.ShouldBe(1);
        cfg?.EntryBlock.ShouldBeEquivalentTo(cfg.BasicBlocks[0]);
        cfg?.ExitBlock.ShouldBeEquivalentTo(cfg.BasicBlocks[0]);
    }
    
    [Fact]
    public async Task ExtractControlFlowAsync_ConditionalMethod_ShouldHaveCorrectComplexity()
    {
        // Load sample code with conditional methods
        var sampleCode = SampleCodeRepository.GetConditionalMethods();
        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        
        // Find the IfElseMethod for testing
        var root = await syntaxTree.GetRootAsync(TestContext.Current.CancellationToken);
        var ifElseMethod = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "IfElseMethod");
        
        // Extract CFG from the conditional method
        var cfg = await _analyzer.ExtractControlFlowAsync(compilation, ifElseMethod);
        
        cfg?.Metrics.CyclomaticComplexity.ShouldBe(2);
        cfg?.Metrics.DecisionPoints.ShouldBeGreaterThan(0);
        cfg?.BasicBlocks.Count(b => b.BranchInfo != null).ShouldBeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task ExtractControlFlowAsync_LoopMethod_ShouldDetectBackEdges()
    {
        // Load sample code with loop methods
        var sampleCode = SampleCodeRepository.GetLoopMethods();
        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        
        // Find the SimpleForLoop method for testing
        var root = await syntaxTree.GetRootAsync(TestContext.Current.CancellationToken);
        var forLoopMethod = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "SimpleForLoop");
        
        // Extract CFG from the loop method
        var cfg = await _analyzer.ExtractControlFlowAsync(compilation, forLoopMethod);
        
        cfg?.Metrics.CyclomaticComplexity.ShouldBeGreaterThan(1);
        cfg?.Metrics.LoopCount.ShouldBeGreaterThanOrEqualTo(1);
        cfg?.Edges.Count(e => e.Kind == CSharpEdgeKind.BackEdge).ShouldBeGreaterThanOrEqualTo(1);
    }
    
    [Fact]
    public async Task ExtractControlFlowAsync_EmptyMethod_ShouldHandleGracefully()
    {
        // Load sample code containing empty method
        var sampleCode = SampleCodeRepository.GetSimpleLinearMethod();
        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        
        // Find the EmptyMethod for testing
        var root = await syntaxTree.GetRootAsync(TestContext.Current.CancellationToken);
        var emptyMethod = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "EmptyMethod");
        
        // Extract CFG from the empty method
        var cfg = await _analyzer.ExtractControlFlowAsync(compilation, emptyMethod);
        
        cfg?.Metrics.CyclomaticComplexity.ShouldBe(1);
        cfg?.BasicBlocks.Sum(b => b.Operations.Count).ShouldBe(0);
    }

    [Fact]
    public async Task ExtractControlFlowAsync_ExpressionBodiedMethod_ShouldWork()
    {
        // Load sample code containing expression-bodied method
        var sampleCode = SampleCodeRepository.GetSimpleLinearMethod();
        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        
        // Find the ExpressionBodiedMethod for testing
        var root = await syntaxTree.GetRootAsync(TestContext.Current.CancellationToken);
        var expressionMethod = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "ExpressionBodiedMethod");
        
        // Extract CFG from the expression-bodied method
        var cfg = await _analyzer.ExtractControlFlowAsync(compilation, expressionMethod);
        
        cfg?.Metrics.CyclomaticComplexity.ShouldBe(1);
        cfg?.BasicBlocks.Sum(b => b.Operations.Count).ShouldBeGreaterThan(0, "Expression-bodied method should have operations");
    }

}