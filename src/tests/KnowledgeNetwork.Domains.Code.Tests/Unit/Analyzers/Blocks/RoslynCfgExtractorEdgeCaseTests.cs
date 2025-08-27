using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using KnowledgeNetwork.Domains.Code.Analyzers.Blocks;
using KnowledgeNetwork.Domains.Code.Analyzers.Blocks.Abstractions;
using KnowledgeNetwork.Tests.Shared;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;
using Xunit;

namespace KnowledgeNetwork.Domains.Code.Tests.Unit.Analyzers.Blocks;

public class RoslynCfgExtractorEdgeCaseTests
{
    private readonly IRoslynCfgExtractor _extractor;

    public RoslynCfgExtractorEdgeCaseTests()
    {
        var logger = NullLogger<RoslynCfgExtractor>.Instance;
        _extractor = new RoslynCfgExtractor(logger);
    }

    [Fact]
    public async Task ExtractControlFlowGraphAsync_AsyncMethod_ShouldHandleAsyncOperations()
    {
        // Arrange
        var sampleCode = SampleCodeRepository.GetAsyncMethods();
        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        
        var root = await syntaxTree.GetRootAsync(TestContext.Current.CancellationToken);
        var asyncMethod = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "SimpleAsyncMethod");

        // Act
        var cfg = await _extractor.ExtractControlFlowGraphAsync(compilation, asyncMethod);

        // Assert
        cfg.ShouldNotBeNull();
        cfg.Blocks.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task ExtractControlFlowGraphAsync_TryCatchFinally_ShouldCreateCorrectStructure()
    {
        // Arrange
        var sampleCode = SampleCodeRepository.GetExceptionHandling();
        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        
        var root = await syntaxTree.GetRootAsync(TestContext.Current.CancellationToken);
        var tryCatchMethod = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "TryCatchFinally");

        // Act
        var cfg = await _extractor.ExtractControlFlowGraphAsync(compilation, tryCatchMethod);

        // Assert
        cfg.ShouldNotBeNull();
        // Try-catch-finally creates multiple blocks: try, catch, finally, and exit
        cfg.Blocks.Length.ShouldBeGreaterThanOrEqualTo(4);
    }

    [Fact]
    public async Task ExtractControlFlowGraphAsync_PatternMatching_ShouldHandleComplexPatterns()
    {
        // Arrange
        var sampleCode = SampleCodeRepository.GetComplexExpressions();
        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        
        var root = await syntaxTree.GetRootAsync(TestContext.Current.CancellationToken);
        var patternMethod = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "PatternMatching");

        // Act
        var cfg = await _extractor.ExtractControlFlowGraphAsync(compilation, patternMethod);

        // Assert
        cfg.ShouldNotBeNull();
        // Switch expression with multiple patterns should create decision blocks
        cfg.Blocks.Length.ShouldBeGreaterThan(2);
    }

    [Fact]
    public async Task ExtractControlFlowGraphAsync_GotoStatement_ShouldCreateBackEdges()
    {
        // Arrange
        var sampleCode = SampleCodeRepository.GetControlFlowVariations();
        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        
        var root = await syntaxTree.GetRootAsync(TestContext.Current.CancellationToken);
        var gotoMethod = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "GotoStatement");

        // Act
        var cfg = await _extractor.ExtractControlFlowGraphAsync(compilation, gotoMethod);

        // Assert
        cfg.ShouldNotBeNull();
        // Goto creates unconditional jumps in the CFG
        cfg.Blocks.Length.ShouldBeGreaterThan(1);
    }

    [Fact]
    public async Task ExtractControlFlowGraphAsync_ComplexLoopWithMultipleExits_ShouldHandleAllPaths()
    {
        // Arrange
        var sampleCode = SampleCodeRepository.GetControlFlowVariations();
        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        
        var root = await syntaxTree.GetRootAsync(TestContext.Current.CancellationToken);
        var complexLoopMethod = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "ComplexLoopWithMultipleExits");

        // Act
        var cfg = await _extractor.ExtractControlFlowGraphAsync(compilation, complexLoopMethod);

        // Assert
        cfg.ShouldNotBeNull();
        // Complex loop with continue, break, and nested conditions
        cfg.Blocks.Length.ShouldBeGreaterThan(3);
    }

    [Fact]
    public async Task ExtractControlFlowGraphAsync_RecursiveMethod_ShouldAnalyzeCorrectly()
    {
        // Arrange
        var sampleCode = SampleCodeRepository.GetMethodVariations();
        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        
        var root = await syntaxTree.GetRootAsync(TestContext.Current.CancellationToken);
        var recursiveMethod = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "RecursiveMethod");

        // Act
        var cfg = await _extractor.ExtractControlFlowGraphAsync(compilation, recursiveMethod);

        // Assert
        cfg.ShouldNotBeNull();
        // Recursive method has condition check and recursive call
        cfg.Blocks.Length.ShouldBeGreaterThan(1);
    }

    [Fact]
    public async Task ExtractControlFlowGraphAsync_GenericMethod_ShouldHandleTypeParameters()
    {
        // Arrange
        var sampleCode = SampleCodeRepository.GetMethodVariations();
        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        
        var root = await syntaxTree.GetRootAsync(TestContext.Current.CancellationToken);
        var genericMethod = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "GenericMethod");

        // Act
        var cfg = await _extractor.ExtractControlFlowGraphAsync(compilation, genericMethod);

        // Assert
        cfg.ShouldNotBeNull();
        cfg.Blocks.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task ExtractControlFlowGraphAsync_LambdaExpressions_ShouldHandleNestedScopes()
    {
        // Arrange
        var sampleCode = SampleCodeRepository.GetMethodVariations();
        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        
        var root = await syntaxTree.GetRootAsync(TestContext.Current.CancellationToken);
        var lambdaMethod = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "LambdaVariations");

        // Act
        var cfg = await _extractor.ExtractControlFlowGraphAsync(compilation, lambdaMethod);

        // Assert
        cfg.ShouldNotBeNull();
        // Method with multiple lambda expressions
        cfg.Blocks.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task ExtractControlFlowGraphAsync_Constructor_ShouldAnalyzeCorrectly()
    {
        // Arrange
        var sampleCode = SampleCodeRepository.GetMethodVariations();
        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        
        var root = await syntaxTree.GetRootAsync(TestContext.Current.CancellationToken);
        var constructor = root.DescendantNodes()
            .OfType<ConstructorDeclarationSyntax>()
            .First(c => c.ParameterList.Parameters.Count == 1); // Constructor with int value

        // Act
        var cfg = await _extractor.ExtractControlFlowGraphAsync(compilation, constructor);

        // Assert
        cfg.ShouldNotBeNull();
        cfg.Blocks.Length.ShouldBeGreaterThan(0);
    }

    [Theory]
    [InlineData("UsingStatement")]
    [InlineData("MultipleCatchBlocks")]
    [InlineData("NestedTryCatch")]
    public async Task ExtractControlFlowGraphAsync_VariousEdgeCases_ShouldNotReturnNull(string methodName)
    {
        // Arrange
        var sampleCode = SampleCodeRepository.GetExceptionHandling();
        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        
        var root = await syntaxTree.GetRootAsync(TestContext.Current.CancellationToken);
        var method = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == methodName);

        // Act
        var cfg = await _extractor.ExtractControlFlowGraphAsync(compilation, method);

        // Assert
        cfg.ShouldNotBeNull("CFG should be created for method: " + methodName);
        cfg.Blocks.Length.ShouldBeGreaterThan(0, "CFG should have at least one block for method: " + methodName);
    }
}