using Microsoft.Extensions.Logging;
using KnowledgeNetwork.Domains.Code.Analyzers.Blocks;
using KnowledgeNetwork.Tests.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Shouldly;
using Xunit;

namespace KnowledgeNetwork.Domains.Code.Tests.Unit.Analyzers.Blocks;

/// <summary>
/// Unit tests for DomainModelConverter service.
/// Tests the conversion from Roslyn ControlFlowGraph to domain-specific MethodBlockGraph.
/// </summary>
public class DomainModelConverterTests
{
    private readonly DomainModelConverter _converter;
    private readonly RoslynCfgExtractor _cfgExtractor;
    private readonly TestLogger<DomainModelConverter> _testLogger;
    private readonly TestLogger<RoslynCfgExtractor> _cfgLogger;

    public DomainModelConverterTests()
    {
        _testLogger = new TestLogger<DomainModelConverter>();
        _cfgLogger = new TestLogger<RoslynCfgExtractor>();
        _converter = new DomainModelConverter(_testLogger);
        _cfgExtractor = new RoslynCfgExtractor(_cfgLogger);
    }

    [Fact]
    public async Task ConvertToDomainModelAsync_SimpleLinearMethod_ShouldCreateValidDomainModel()
    {
        // Arrange: Get a simple method with known structure
        var sampleCode = SampleCodeRepository.GetSimpleLinearMethod();
        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        
        var root = await syntaxTree.GetRootAsync();
        var method = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "SimpleMethod");

        // First, create CFG using RoslynCfgExtractor
        var cfg = await _cfgExtractor.ExtractControlFlowGraphAsync(compilation, method);
        cfg.ShouldNotBeNull("CFG extraction should succeed");

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var methodSymbol = semanticModel.GetDeclaredSymbol(method);
        methodSymbol.ShouldNotBeNull("Method symbol should be available");

        // TODO(human): Implement the test validation
        // Context: We have a working CFG and method symbol for a simple linear method. The DomainModelConverter should create a MethodBlockGraph with proper blocks, edges, and metadata.
        // Your Task: Add test logic to: 1) Call ConvertToDomainModelAsync with the CFG, method syntax, and symbol, 2) Validate the resulting MethodBlockGraph has correct method name and type, 3) Check that basic blocks are properly converted, 4) Verify entry/exit blocks are identified, 5) Ensure complexity metrics are calculated
        // Guidance: Use Shouldly assertions like .ShouldNotBeNull(), .ShouldBe(), .ShouldBeGreaterThan(). Focus on the most important conversion aspects rather than testing every detail.
    }

    [Fact]
    public async Task ConvertToDomainModelAsync_ConditionalMethod_ShouldCreateMultipleBlocksAndEdges()
    {
        // Arrange: Get conditional method with branching
        var sampleCode = SampleCodeRepository.GetConditionalMethods();
        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        
        var root = await syntaxTree.GetRootAsync();
        var method = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "IfElseMethod");

        var cfg = await _cfgExtractor.ExtractControlFlowGraphAsync(compilation, method);
        cfg.ShouldNotBeNull("CFG extraction should succeed");

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var methodSymbol = semanticModel.GetDeclaredSymbol(method);
        methodSymbol.ShouldNotBeNull("Method symbol should be available");

        // Act
        var domainModel = await _converter.ConvertToDomainModelAsync(cfg, method, methodSymbol);

        // Assert
        domainModel.ShouldNotBeNull("Domain model conversion should succeed");
        domainModel.MethodName.ShouldNotBeEmpty("Method name should be set");
        domainModel.BasicBlocks.Count.ShouldBeGreaterThan(1, "Conditional method should have multiple blocks");
        domainModel.Edges.Count.ShouldBeGreaterThan(0, "Should have control flow edges");
        domainModel.EntryBlock.ShouldNotBeNull("Should have entry block");
        domainModel.Metrics.ShouldNotBeNull("Should calculate complexity metrics");
        domainModel.Metrics.CyclomaticComplexity.ShouldBeGreaterThan(1, "Conditional method should have complexity > 1");
    }

    [Fact]
    public async Task ConvertToDomainModelAsync_LoopMethod_ShouldDetectBackEdges()
    {
        // Arrange: Get method with loops
        var sampleCode = SampleCodeRepository.GetLoopMethods();
        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        
        var root = await syntaxTree.GetRootAsync();
        var method = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "SimpleForLoop");

        var cfg = await _cfgExtractor.ExtractControlFlowGraphAsync(compilation, method);
        cfg.ShouldNotBeNull("CFG extraction should succeed");

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var methodSymbol = semanticModel.GetDeclaredSymbol(method);
        methodSymbol.ShouldNotBeNull("Method symbol should be available");

        // Act
        var domainModel = await _converter.ConvertToDomainModelAsync(cfg, method, methodSymbol);

        // Assert
        domainModel.ShouldNotBeNull("Domain model conversion should succeed");
        domainModel.BasicBlocks.Count.ShouldBeGreaterThan(2, "Loop method should have multiple blocks");
        domainModel.Metrics.LoopCount.ShouldBeGreaterThan(0, "Should detect loops");
        
        // Check for back edges (indicates loop structure)
        var backEdges = domainModel.Edges.Where(e => e.Kind == KnowledgeNetwork.Domains.Code.Models.Enums.CSharpEdgeKind.BackEdge).ToList();
        backEdges.ShouldNotBeEmpty("Loop method should have back edges");
    }

    [Fact]
    public async Task ConvertConstructorToDomainModelAsync_ShouldSetConstructorName()
    {
        // Arrange: Constructor test
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

        var cfg = await _cfgExtractor.ExtractControlFlowGraphAsync(compilation, constructor);
        cfg.ShouldNotBeNull("CFG extraction should succeed for constructor");

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var constructorSymbol = semanticModel.GetDeclaredSymbol(constructor);
        constructorSymbol.ShouldNotBeNull("Constructor symbol should be available");

        // Act
        var domainModel = await _converter.ConvertConstructorToDomainModelAsync(cfg, constructor, constructorSymbol);

        // Assert
        domainModel.ShouldNotBeNull("Constructor domain model conversion should succeed");
        domainModel.MethodName.ShouldStartWith(".ctor");
        domainModel.MethodName.ShouldContain("int");
        domainModel.BasicBlocks.Count.ShouldBeGreaterThan(0, "Constructor should have basic blocks");
    }

    [Fact]
    public async Task ConvertToDomainModelAsync_ShouldCalculateReachability()
    {
        // Arrange
        var sampleCode = SampleCodeRepository.GetSimpleLinearMethod();
        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        
        var root = await syntaxTree.GetRootAsync();
        var method = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "SimpleMethod");

        var cfg = await _cfgExtractor.ExtractControlFlowGraphAsync(compilation, method);
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var methodSymbol = semanticModel.GetDeclaredSymbol(method);

        // Act
        var domainModel = await _converter.ConvertToDomainModelAsync(cfg, method, methodSymbol);

        // Assert - all blocks in a simple linear method should be reachable
        domainModel.BasicBlocks.ShouldAllBe(block => block.IsReachable, "All blocks should be reachable in simple method");
    }

    [Fact]
    public async Task ConvertToDomainModelAsync_ShouldSetLocationInformation()
    {
        // Arrange
        var sampleCode = SampleCodeRepository.GetSimpleLinearMethod();
        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        
        var root = await syntaxTree.GetRootAsync();
        var method = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "SimpleMethod");

        var cfg = await _cfgExtractor.ExtractControlFlowGraphAsync(compilation, method);
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var methodSymbol = semanticModel.GetDeclaredSymbol(method);

        // Act
        var domainModel = await _converter.ConvertToDomainModelAsync(cfg, method, methodSymbol);

        // Assert
        domainModel.Location.ShouldNotBeNull("Should set location information");
        domainModel.Location.StartLine.ShouldBeGreaterThan(0, "Should have valid start line");
        domainModel.Location.StartColumn.ShouldBeGreaterThan(0, "Should have valid start column");
    }

    [Fact]
    public async Task ConvertToDomainModelAsync_NullParameters_ShouldThrowArgumentNullException()
    {
        // Arrange
        var sampleCode = SampleCodeRepository.GetSimpleLinearMethod();
        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        
        var root = await syntaxTree.GetRootAsync();
        var method = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First();

        var cfg = await _cfgExtractor.ExtractControlFlowGraphAsync(compilation, method);
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var methodSymbol = semanticModel.GetDeclaredSymbol(method);

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(() => _converter.ConvertToDomainModelAsync(null!, method, methodSymbol));
        await Should.ThrowAsync<ArgumentNullException>(() => _converter.ConvertToDomainModelAsync(cfg, null!, methodSymbol));
        await Should.ThrowAsync<ArgumentNullException>(() => _converter.ConvertToDomainModelAsync(cfg, method, null!));
    }

    [Fact]
    public async Task ConvertToDomainModelAsync_ShouldLogDebugMessages()
    {
        // Arrange
        var sampleCode = SampleCodeRepository.GetSimpleLinearMethod();
        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        
        var root = await syntaxTree.GetRootAsync();
        var method = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "SimpleMethod");

        var cfg = await _cfgExtractor.ExtractControlFlowGraphAsync(compilation, method);
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var methodSymbol = semanticModel.GetDeclaredSymbol(method);

        // Act
        await _converter.ConvertToDomainModelAsync(cfg, method, methodSymbol);

        // Assert
        var debugMessages = _testLogger.LogMessages.Where(m => m.StartsWith("[Debug]")).ToList();
        debugMessages.ShouldNotBeEmpty("Should log debug messages during conversion");
        debugMessages.ShouldContain(m => m.Contains("Converting CFG to domain model"));
        debugMessages.ShouldContain(m => m.Contains("Successfully converted CFG"));
    }
}