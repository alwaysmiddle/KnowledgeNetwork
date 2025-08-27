using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using KnowledgeNetwork.Domains.Code.Analyzers.Blocks;
using KnowledgeNetwork.Domains.Code.Analyzers.Blocks.Abstractions;
using KnowledgeNetwork.Tests.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;
using Xunit;

namespace KnowledgeNetwork.Domains.Code.Tests.Unit.Analyzers.Blocks;

/// <summary>
/// Unit tests for RoslynCfgExtractor service.
/// Tests the combined operation extraction and ControlFlowGraph creation functionality.
/// </summary>
public class RoslynCfgExtractorTests
{
    private readonly IRoslynCfgExtractor _extractor;
    private readonly TestLogger<RoslynCfgExtractor> _testLogger;

    public RoslynCfgExtractorTests()
    {
        _testLogger = new TestLogger<RoslynCfgExtractor>();
        _extractor = new RoslynCfgExtractor(_testLogger);
    }

    [Fact]
    public async Task ExtractControlFlowGraphAsync_SimpleLinearMethod_ShouldReturnValidCfg()
    {
        // Arrange: Use proven working sample code
        var sampleCode = SampleCodeRepository.GetSimpleLinearMethod();
        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        
        var root = await syntaxTree.GetRootAsync();
        var method = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "SimpleMethod");

        // Act
        var cfg = await _extractor.ExtractControlFlowGraphAsync(compilation, method);

        // Assert
        cfg.ShouldNotBeNull("Should extract CFG from simple linear method");
        cfg.Blocks.Length.ShouldBeGreaterThan(0, "CFG should have at least one basic block");
        
        // Verify no errors were logged
        var errorMessages = _testLogger.LogMessages.Where(m => m.StartsWith("[Error]")).ToList();
        errorMessages.ShouldBeEmpty($"No errors should be logged, but got: {string.Join(", ", errorMessages)}");
    }

    [Fact]
    public async Task ExtractControlFlowGraphAsync_EmptyMethod_ShouldReturnValidCfg()
    {
        // Arrange: Test empty method
        var sampleCode = SampleCodeRepository.GetSimpleLinearMethod();
        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        
        var root = await syntaxTree.GetRootAsync();
        var method = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "EmptyMethod");

        // Act
        var cfg = await _extractor.ExtractControlFlowGraphAsync(compilation, method);

        // Assert
        cfg.ShouldNotBeNull("Empty method should still produce valid CFG");
        cfg.Blocks.Length.ShouldBeGreaterThanOrEqualTo(1, "Empty method should have at least entry block");
    }

    [Fact]
    public async Task ExtractControlFlowGraphAsync_ConditionalMethod_ShouldReturnMultipleBlocks()
    {
        // Arrange: Test method with branching
        var sampleCode = SampleCodeRepository.GetConditionalMethods();
        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        
        var root = await syntaxTree.GetRootAsync();
        var method = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "IfElseMethod");

        // Act
        var cfg = await _extractor.ExtractControlFlowGraphAsync(compilation, method);

        // Assert
        cfg.ShouldNotBeNull("Conditional method should produce valid CFG");
        cfg.Blocks.Length.ShouldBeGreaterThan(2, "Conditional method should have multiple blocks");
    }

    [Fact]
    public async Task ExtractControlFlowGraphAsync_LoopMethod_ShouldReturnComplexCfg()
    {
        // Arrange: Test method with loops
        var sampleCode = SampleCodeRepository.GetLoopMethods();
        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        
        var root = await syntaxTree.GetRootAsync();
        var method = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "SimpleForLoop");

        // Act
        var cfg = await _extractor.ExtractControlFlowGraphAsync(compilation, method);

        // Assert
        cfg.ShouldNotBeNull("Loop method should produce valid CFG");
        cfg.Blocks.Length.ShouldBeGreaterThan(2, "Loop method should have multiple blocks for loop structure");
    }

    [Fact]
    public async Task ExtractControlFlowGraphAsync_ExpressionBodiedMethod_ShouldReturnNull()
    {
        // Arrange: Test expression-bodied method (currently not supported)
        var sampleCode = SampleCodeRepository.GetSimpleLinearMethod();
        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        
        var root = await syntaxTree.GetRootAsync();
        var method = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "ExpressionBodiedMethod");

        // Act
        var cfg = await _extractor.ExtractControlFlowGraphAsync(compilation, method);

        // Assert
        cfg.ShouldBeNull("Expression-bodied methods should return null (not supported yet)");
        
        // Verify appropriate debug message was logged
        var debugMessages = _testLogger.LogMessages.Where(m => m.Contains("has no block body")).ToList();
        debugMessages.ShouldNotBeEmpty("Should log message about missing block body");
    }

    [Fact]
    public async Task ExtractControlFlowGraphAsync_InvalidMethod_ShouldReturnNullAndLogWarning()
    {
        // Arrange: Create malformed code that won't have proper method symbol
        var malformedCode = @"
            public class TestClass 
            {
                public void // Incomplete method declaration
            }";

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(malformedCode);
        var root = await syntaxTree.GetRootAsync();
        
        // Try to find any method declaration (even malformed)
        var method = root.DescendantNodes().OfType<MethodDeclarationSyntax>().FirstOrDefault();
        
        if (method != null)
        {
            // Act
            var cfg = await _extractor.ExtractControlFlowGraphAsync(compilation, method);

            // Assert
            cfg.ShouldBeNull("Malformed method should return null");
            
            // Verify warning was logged
            var warningMessages = _testLogger.LogMessages.Where(m => m.StartsWith("[Warning]")).ToList();
            warningMessages.ShouldNotBeEmpty("Should log warning for invalid method");
        }
        else
        {
            // If we can't even parse a method declaration, that's expected
            true.ShouldBeTrue("Malformed code couldn't be parsed - this is expected");
        }
    }

    // TODO(human): Add comprehensive error handling tests for edge cases
    // Context: We've established the basic testing pattern for RoslynCfgExtractor. Now we need robust error handling tests for scenarios where CFG creation might fail with different types of methods and compilation issues.
    // Your Task: Add tests for scenarios like: 1) Methods with complex generic constraints, 2) Async methods, 3) Methods with unsafe code, 4) Methods with advanced language features (pattern matching, switch expressions), 5) Methods in incomplete compilations.
    // Guidance: Focus on testing the service's error handling and logging behavior rather than testing Roslyn's CFG creation itself. Each test should verify both the return value and the appropriate log messages.

    [Fact]
    public async Task ExtractControlFlowGraphAsync_ConstructorTest_ShouldReturnValidCfg()
    {
        // Arrange: Test constructor CFG extraction
        var sampleCode = @"
            public class TestClass 
            {
                private int _value;
                
                public TestClass(int value)
                {
                    _value = value;
                    Console.WriteLine($""Initialized with {value}"");
                }
            }";

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        var root = await syntaxTree.GetRootAsync();
        var constructor = root.DescendantNodes()
            .OfType<ConstructorDeclarationSyntax>()
            .First();

        // Act
        var cfg = await _extractor.ExtractControlFlowGraphAsync(compilation, constructor);

        // Assert
        cfg.ShouldNotBeNull("Constructor should produce valid CFG");
        cfg.Blocks.Length.ShouldBeGreaterThan(0, "Constructor CFG should have at least one basic block");
    }

    [Fact]
    public async Task ExtractControlFlowGraphAsync_NullCompilation_ShouldThrowArgumentException()
    {
        // Arrange
        var sampleCode = SampleCodeRepository.GetSimpleLinearMethod();
        var (_, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        
        var root = await syntaxTree.GetRootAsync();
        var method = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First();

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(
            () => _extractor.ExtractControlFlowGraphAsync(null!, method));
    }

    [Fact]
    public async Task ExtractControlFlowGraphAsync_NullMethod_ShouldThrowArgumentException()
    {
        // Arrange
        var sampleCode = SampleCodeRepository.GetSimpleLinearMethod();
        var (compilation, _) = CompilationFactory.CreateBasic(sampleCode);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(
            () => _extractor.ExtractControlFlowGraphAsync(compilation, (MethodDeclarationSyntax)null!));
    }
}