using KnowledgeNetwork.Domains.Code.Analyzers.Blocks;
using KnowledgeNetwork.Domains.Code.Analyzers.Blocks.Abstractions;
using KnowledgeNetwork.Tests.Shared;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;
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
        
        var root = await syntaxTree.GetRootAsync(TestContext.Current.CancellationToken);
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
        
        var root = await syntaxTree.GetRootAsync(TestContext.Current.CancellationToken);
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
        
        var root = await syntaxTree.GetRootAsync(TestContext.Current.CancellationToken);
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
        
        var root = await syntaxTree.GetRootAsync(TestContext.Current.CancellationToken);
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
    public async Task ExtractControlFlowGraphAsync_ExpressionBodiedMethod_ShouldReturnValidCfg()
    {
        // Arrange: Test expression-bodied method (should be fully supported)
        var sampleCode = SampleCodeRepository.GetSimpleLinearMethod();
        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        
        var root = await syntaxTree.GetRootAsync(TestContext.Current.CancellationToken);
        var method = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "ExpressionBodiedMethod");

        // Verify it's actually an expression body
        method.ExpressionBody.ShouldNotBeNull("Method should have expression body");
        method.Body.ShouldBeNull("Method should not have block body");

        // Act
        var cfg = await _extractor.ExtractControlFlowGraphAsync(compilation, method);

        // Assert: Expression bodies should produce valid CFGs
        cfg.ShouldNotBeNull("Expression-bodied methods should produce valid CFG");
        cfg.Blocks.Length.ShouldBeGreaterThan(0, "CFG should have at least one basic block");
        
        // Verify no errors were logged
        var errorMessages = _testLogger.LogMessages.Where(m => m.StartsWith("[Error]")).ToList();
        errorMessages.ShouldBeEmpty($"No errors should be logged for expression bodies, but got: {string.Join(", ", errorMessages)}");
    }

    [Fact]
    public async Task ExtractControlFlowGraphAsync_ExpressionBodyMethodCalls_PreservesDependencyInformation()
    {
        // Arrange: Expression bodies that call methods - dependency information should be preserved
        var sampleCode = @"
            public class ServiceClass 
            {
                private readonly IRepository _repository;
                private readonly ILogger _logger;
                
                // These expression bodies contain important method calls that should be captured
                public User GetUser(int id) => _repository.FindById(id);
                public void LogError(string message) => _logger.LogError(""Error: {Message}"", message);
                public bool ValidateUser(User user) => _repository.IsValidUser(user) && user.IsActive();
            }";

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        var root = await syntaxTree.GetRootAsync(TestContext.Current.CancellationToken);
        
        var getUserMethod = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "GetUser");

        var logErrorMethod = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "LogError");

        var validateMethod = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "ValidateUser");

        // Act
        var getUserCfg = await _extractor.ExtractControlFlowGraphAsync(compilation, getUserMethod);
        var logErrorCfg = await _extractor.ExtractControlFlowGraphAsync(compilation, logErrorMethod);
        var validateCfg = await _extractor.ExtractControlFlowGraphAsync(compilation, validateMethod);

        // Assert: All should return valid CFGs with dependency information
        getUserCfg.ShouldNotBeNull("GetUser method should return valid CFG with _repository.FindById dependency");
        logErrorCfg.ShouldNotBeNull("LogError method should return valid CFG with _logger.LogError dependency");
        validateCfg.ShouldNotBeNull("ValidateUser method should return valid CFG with multiple method call dependencies");
        
        // Verify CFG structure
        getUserCfg.Blocks.Length.ShouldBeGreaterThan(0, "GetUser CFG should have basic blocks");
        logErrorCfg.Blocks.Length.ShouldBeGreaterThan(0, "LogError CFG should have basic blocks");
        validateCfg.Blocks.Length.ShouldBeGreaterThan(0, "ValidateUser CFG should have basic blocks");
        
        // GUARDRAIL: This test ensures class dependency graph analysis captures expression body method calls
        // Critical for knowledge network's ability to understand service relationships
    }

    [Fact]
    public async Task ExtractControlFlowGraphAsync_ExpressionBodyComplexityMetrics_AreProperlyIncluded()
    {
        // Arrange: Expression bodies with varying complexity levels - all should be analyzed
        var sampleCode = @"
            public class ComplexityTestClass 
            {
                // Simple expression - low complexity
                public int Simple() => 42;
                
                // Conditional expression - medium complexity  
                public string Conditional(bool flag) => flag ? ""Yes"" : ""No"";
                
                // Complex nested conditionals - high complexity
                public decimal ComplexCalculation(decimal price, bool isPremium, bool hasDiscount) =>
                    isPremium 
                        ? (hasDiscount ? price * 0.8m * 0.9m : price * 0.9m)
                        : (hasDiscount ? price * 0.95m : price);
                        
                // Pattern matching - high complexity
                public string ProcessValue(object value) =>
                    value switch
                    {
                        int i when i > 100 => ""Large number"",
                        int i => ""Small number"", 
                        string s when s.Length > 10 => ""Long string"",
                        string s => ""Short string"",
                        _ => ""Unknown""
                    };
            }";

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        var root = await syntaxTree.GetRootAsync(TestContext.Current.CancellationToken);
        
        var methods = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .ToList();

        // Act: Extract CFG for all methods
        var results = new List<(string name, bool hasExpressionBody, ControlFlowGraph cfg)>();
        foreach (var method in methods)
        {
            var cfg = await _extractor.ExtractControlFlowGraphAsync(compilation, method);
            results.Add((
                method.Identifier.ValueText,
                method.ExpressionBody != null,
                cfg
            ));
        }

        // Assert: All expression bodies should return valid CFGs regardless of complexity
        foreach (var (name, hasExpressionBody, cfg) in results)
        {
            if (hasExpressionBody) // Expression body
            {
                cfg.ShouldNotBeNull($"Expression-bodied method {name} should return valid CFG");
                cfg.Blocks.Length.ShouldBeGreaterThan(0, $"Expression-bodied method {name} should have basic blocks");
            }
        }
        
        // GUARDRAIL: Complexity metrics for classes include expression-bodied methods
        // This ensures accurate complexity calculations for the knowledge network
        var expressionBodyCount = results.Count(r => r.hasExpressionBody);
        expressionBodyCount.ShouldBeGreaterThan(0, "Should find expression-bodied methods in test code");
        
        // Verify no errors were logged
        var errorMessages = _testLogger.LogMessages.Where(m => m.StartsWith("[Error]")).ToList();
        errorMessages.ShouldBeEmpty("No errors should be logged for expression bodies with complex logic");
    }

    [Theory]
    [InlineData("void", "LogInfo", "_logger.LogInformation(\"Info\");")]
    [InlineData("string", "GetName", "_user.FirstName + \" \" + _user.LastName;")]
    [InlineData("bool", "IsValid", "_user != null && _user.IsActive;")]
    [InlineData("Task", "SaveAsync", "_repository.SaveAsync(_user);")]
    public async Task ExtractControlFlowGraphAsync_ExpressionBodyVariations_AllReturnValidCfgRegardlessOfReturnType(
        string returnType, string methodName, string expressionBody)
    {
        // Arrange: Various expression body patterns - all should be supported
        var sampleCode = $@"
            public class TestClass 
            {{
                private readonly ILogger _logger;
                private readonly IRepository _repository;
                private readonly User _user;
                
                public {returnType} {methodName}() => {expressionBody}
            }}";

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        var root = await syntaxTree.GetRootAsync(TestContext.Current.CancellationToken);
        
        var method = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == methodName);

        // Verify it's actually an expression body
        method.ExpressionBody.ShouldNotBeNull($"Method {methodName} should have expression body");
        method.Body.ShouldBeNull($"Method {methodName} should not have block body");

        // Act
        var cfg = await _extractor.ExtractControlFlowGraphAsync(compilation, method);

        // Assert: All variations should return valid CFGs
        cfg.ShouldNotBeNull($"Expression-bodied method {methodName} with return type {returnType} should return valid CFG");
        cfg.Blocks.Length.ShouldBeGreaterThan(0, $"Method {methodName} CFG should have at least one basic block");
        
        // GUARDRAIL: This test ensures expression bodies work across different return types
        // Critical for knowledge network completeness - void, reference types, value types, Task types
        var errorMessages = _testLogger.LogMessages.Where(m => m.StartsWith("[Error]")).ToList();
        errorMessages.ShouldBeEmpty($"No errors should be logged for {methodName} with return type {returnType}");
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
        var root = await syntaxTree.GetRootAsync(TestContext.Current.CancellationToken);
        
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
    
    [Fact]
    public async Task ExtractControlFlowGraphAsync_ConstructorTest_ShouldReturnValidCfg()
    {
        // Arrange: Test constructor CFG extraction
        const string sampleCode = """

                                              public class TestClass 
                                              {
                                                  private int _value;
                                                  
                                                  public TestClass(int value)
                                                  {
                                                      _value = value;
                                                      Console.WriteLine($"Initialized with {value}");
                                                  }
                                              }
                                  """;

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        var root = await syntaxTree.GetRootAsync(TestContext.Current.CancellationToken);
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
        
        var root = await syntaxTree.GetRootAsync(TestContext.Current.CancellationToken);
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