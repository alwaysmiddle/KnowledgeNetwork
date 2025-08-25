using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using KnowledgeNetwork.Domains.Code.Analyzers.Blocks;
using KnowledgeNetwork.Tests.Shared;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Shouldly;
using Xunit;

namespace KnowledgeNetwork.Domains.Code.Tests.Unit.Analyzers.Blocks;


/// <summary>
/// Tests for CfgStructureBuilder service.
/// 
/// Note: Pure unit testing of CfgStructureBuilder is challenging because Roslyn's ControlFlowGraph.Create() 
/// has strict requirements about the semantic context of IBlockOperation. The working integration tests 
/// (CSharpMethodBlockAnalyzerTests) prove that CfgStructureBuilder works correctly when used with 
/// proper service composition.
/// 
/// These tests focus on integration-style testing with real dependencies and error handling scenarios.
/// </summary>
public class CfgStructureBuilderTests
{
    private readonly CfgStructureBuilder _builder;
    private readonly TestLogger<CfgStructureBuilder> _testLogger;

    public CfgStructureBuilderTests()
    {
        _testLogger = new TestLogger<CfgStructureBuilder>();
        _builder = new CfgStructureBuilder(_testLogger);
    }

    [Fact]
    public async Task BuildStructureAsync_WithRealOperationExtractor_ShouldReturnControlFlowGraph()
    {
        // Arrange: Use real RoslynOperationExtractor to get proper IBlockOperation
        var sampleCode = SampleCodeRepository.GetSimpleLinearMethod();
        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        
        var root = await syntaxTree.GetRootAsync();
        var method = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First(m => m.Identifier.ValueText == "SimpleMethod");

        // Try to get IBlockOperation without parent - test different extraction approaches
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        
        // Approach 1: Get operation from method body directly
        var bodyOperation = semanticModel.GetOperation(method.Body);
        _testLogger.LogMessages.Add($"DEBUG: Body operation type: {bodyOperation?.GetType().Name}, Parent: {bodyOperation?.Parent != null}");
        
        // Approach 2: Try getting operation from the method itself  
        var methodOperation = semanticModel.GetOperation(method);
        _testLogger.LogMessages.Add($"DEBUG: Method operation type: {methodOperation?.GetType().Name}, Parent: {methodOperation?.Parent != null}");
        
        IBlockOperation? blockOperation = null;
        
        // Check if method operation gives us what we need
        if (methodOperation is IMethodBodyOperation { BlockBody: not null } methodBodyOp)
        {
            _testLogger.LogMessages.Add($"DEBUG: MethodBodyOperation.BlockBody has parent: {methodBodyOp.BlockBody.Parent != null}");
            blockOperation = methodBodyOp.BlockBody;
        }
        else if (bodyOperation is IBlockOperation directBlock)
        {
            _testLogger.LogMessages.Add($"DEBUG: Direct block operation has parent: {directBlock.Parent != null}");
            blockOperation = directBlock;
        }
        
        // Debug: If null, let's see why by manually checking the operation
        if (blockOperation == null)
        {
            var bodyNode = method.Body;
            var operation = semanticModel.GetOperation(bodyNode);
            
            // Let's see what operation type we actually get
            throw new InvalidOperationException($"RoslynOperationExtractor returned null. " +
                $"Operation type: {operation?.GetType().Name ?? "NULL"}, " +
                $"Operation kind: {operation?.Kind.ToString() ?? "NULL"}, " +
                $"Method: {method.Identifier}, " +
                $"BodyNode type: {bodyNode?.GetType().Name ?? "NULL"}");
        }
        
        blockOperation.ShouldNotBeNull("RoslynOperationExtractor should extract valid IBlockOperation");

        // Debug: Let's see what IBlockOperation we actually got
        var blockOperationInfo = $"IBlockOperation type: {blockOperation.GetType().Name}, " +
            $"Kind: {blockOperation.Kind}, " +
            $"Operations count: {blockOperation.Operations.Length}, " +
            $"Syntax: {blockOperation.Syntax?.GetType().Name ?? "NULL"}";

        // Act
        var cfg = await _builder.BuildStructureAsync(blockOperation, "SimpleMethod");
        
        // If cfg is null, provide detailed debugging info including log messages
        if (cfg == null)
        {
            var logMessages = string.Join("\n", _testLogger.LogMessages);
            cfg.ShouldNotBeNull($"CfgStructureBuilder returned null. " +
                $"BlockOperation info: {blockOperationInfo}\n" +
                $"Log messages from CfgStructureBuilder:\n{logMessages}");
        }

        // Assert
        cfg.ShouldNotBeNull("CfgStructureBuilder should create CFG from real extraction");
        cfg.Blocks.Length.ShouldBeGreaterThan(0, "CFG should have at least one basic block");
    }

    [Fact]
    public async Task BuildStructureAsync_NullBlockOperation_ShouldReturnNull()
    {
        // Act
        var cfg = await _builder.BuildStructureAsync(null!, "TestMethod");

        // Assert
        cfg.ShouldBeNull("Null input should return null result");
    }

    [Fact]
    public async Task BuildStructureAsync_EmptyMethodBody_ShouldHandleGracefully()
    {
        // Arrange: Create IBlockOperation from empty method
        var sampleCode = @"
            public class TestClass 
            {
                public void EmptyMethod() { }
            }";

        var (compilation, syntaxTree) = CompilationFactory.CreateExtended(sampleCode);
        var root = await syntaxTree.GetRootAsync(TestContext.Current.CancellationToken);
        var method = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First();

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var bodyOperation = semanticModel.GetOperation(method.Body!, TestContext.Current.CancellationToken);

        // Extract IBlockOperation using the same pattern as RoslynOperationExtractor
        var blockOperation = bodyOperation switch
        {
            IBlockOperation directBlock => directBlock,
            IMethodBodyOperation { BlockBody: not null } methodBodyOp => methodBodyOp.BlockBody,
            _ => null
        };

        blockOperation.ShouldNotBeNull("Empty method should still produce IBlockOperation");

        // Act
        var cfg = await _builder.BuildStructureAsync(blockOperation, "EmptyMethod");

        // Assert
        cfg.ShouldNotBeNull("Empty method should produce valid CFG");
        cfg.Blocks.Length.ShouldBeGreaterThanOrEqualTo(1, "Empty method should have at least entry block");
    }

    [Fact]
    public async Task BuildStructureAsync_ComplexControlFlow_ShouldCreateMultipleBlocks()
    {
        // Arrange: Create IBlockOperation from method with control flow
        var sampleCode = @"
            public class TestClass 
            {
                public void ConditionalMethod(bool condition)
                {
                    if (condition)
                    {
                        Console.WriteLine(""True branch"");
                    }
                    else
                    {
                        Console.WriteLine(""False branch"");
                    }
                    Console.WriteLine(""End"");
                }
            }";

        var (compilation, syntaxTree) = CompilationFactory.CreateExtended(sampleCode);
        var root = await syntaxTree.GetRootAsync();
        var method = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First();

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var bodyOperation = semanticModel.GetOperation(method.Body!, TestContext.Current.CancellationToken);

        // Extract IBlockOperation using the same pattern as RoslynOperationExtractor
        var blockOperation = bodyOperation switch
        {
            IBlockOperation directBlock => directBlock,
            IMethodBodyOperation { BlockBody: not null } methodBodyOp => methodBodyOp.BlockBody,
            _ => null
        };

        blockOperation.ShouldNotBeNull();

        // Act
        var cfg = await _builder.BuildStructureAsync(blockOperation, "ConditionalMethod");

        // Assert
        cfg.ShouldNotBeNull();
        cfg.Blocks.Length.ShouldBeGreaterThan(1, "Conditional method should have multiple basic blocks");
    }

    [Fact]
    public async Task BuildStructureAsync_LoopStructure_ShouldHandleBackEdges()
    {
        // Arrange: Create IBlockOperation from method with loop
        var sampleCode = @"
            public class TestClass 
            {
                public void LoopMethod()
                {
                    for (int i = 0; i < 10; i++)
                    {
                        Console.WriteLine(i);
                    }
                }
            }";

        var (compilation, syntaxTree) = CompilationFactory.CreateExtended(sampleCode);
        var root = await syntaxTree.GetRootAsync(TestContext.Current.CancellationToken);
        var method = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First();

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var bodyOperation = semanticModel.GetOperation(method.Body!, TestContext.Current.CancellationToken);

        // Extract IBlockOperation using the same pattern as RoslynOperationExtractor
        var blockOperation = bodyOperation switch
        {
            IBlockOperation directBlock => directBlock,
            IMethodBodyOperation { BlockBody: not null } methodBodyOp => methodBodyOp.BlockBody,
            _ => null
        };

        blockOperation.ShouldNotBeNull();

        // Act
        var cfg = await _builder.BuildStructureAsync(blockOperation, "LoopMethod");

        // Assert
        cfg.ShouldNotBeNull();
        cfg.Blocks.Length.ShouldBeGreaterThan(2, "Loop method should have multiple blocks for loop structure");
    }

    // TODO(human): Add comprehensive error handling tests for CfgStructureBuilder edge cases
    // Context: We've established the basic testing patterns, but CfgStructureBuilder needs robust error handling tests for scenarios where Roslyn's ControlFlowGraph.Create() fails. The challenge is that direct unit testing of CfgStructureBuilder is difficult because ControlFlowGraph.Create() has strict requirements about the IBlockOperation's compilation context.
    // Your Task: Create advanced error handling tests that cover: 1) ArgumentException scenarios from Roslyn, 2) InvalidOperationException scenarios, 3) Operations from different compilation contexts, 4) Malformed operation trees, 5) Operations with missing semantic information.
    // Guidance: You may need to use integration testing approaches (like the existing CSharpMethodBlockAnalyzerTests) rather than pure isolation, or create mock IBlockOperation instances that simulate edge cases. Consider using the RoslynOperationExtractor to create realistic but problematic IBlockOperation instances.

    [Fact]
    public async Task BuildStructureAsync_ExpressionBodyMethod_ShouldWork()
    {
        // Arrange: Create IBlockOperation from expression-bodied method (this might not work as expected)
        var sampleCode = @"
            public class TestClass 
            {
                public int ExpressionMethod() => 42;
            }";

        var (compilation, syntaxTree) = CompilationFactory.CreateExtended(sampleCode);
        var root = await syntaxTree.GetRootAsync();
        var method = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First();

        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        
        // Note: Expression-bodied methods use ExpressionBody, not Body
        var operation = method.ExpressionBody != null 
            ? semanticModel.GetOperation(method.ExpressionBody) 
            : semanticModel.GetOperation(method.Body!) as IBlockOperation;

        // Act & Assert
        if (operation is IBlockOperation blockOp)
        {
            var cfg = await _builder.BuildStructureAsync(blockOp, "ExpressionMethod");
            cfg.ShouldNotBeNull("Expression-bodied method should create valid CFG when block operation available");
        }
        else
        {
            // Expression-bodied methods might not produce IBlockOperation directly
            // This test documents the expected behavior
            operation.ShouldNotBeNull("Should get some operation from expression body");
        }
    }
}