using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using KnowledgeNetwork.Tests.Shared;
using Shouldly;
using Xunit;

namespace KnowledgeNetwork.Domains.Code.Tests.Unit.Analyzers.Blocks;

/// <summary>
/// Diagnostic test to understand how Roslyn operations work with different method types
/// </summary>
public class RoslynDiagnosticsTest
{

    [Fact]
    public async Task DiagnoseOperationTypes_SimpleMethod_ShouldShowOperationHierarchy()
    {
        // Arrange
        var sampleCode = @"
            using System;
            public class TestClass 
            {
                public void SimpleMethod()
                {
                    int x = 10;
                    Console.WriteLine(x);
                }
            }";

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        var root = await syntaxTree.GetRootAsync(TestContext.Current.CancellationToken);
        var method = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First();

        var semanticModel = compilation.GetSemanticModel(syntaxTree);

        // Act - Get operation from method body
        var bodyOperation = semanticModel.GetOperation(method.Body!, TestContext.Current.CancellationToken);

        // Diagnose what we actually got - using assertions instead of output
        bodyOperation.ShouldNotBeNull("Should get some operation from method body");
        
        // Check if it's directly a block operation
        if (bodyOperation is IBlockOperation directBlock)
        {
            // This would be ideal but unlikely
            directBlock.Operations.Length.ShouldBeGreaterThan(0, "Direct block should have operations");
        }
        
        // Check if it's a method body operation (most likely scenario)
        if (bodyOperation is IMethodBodyOperation methodBodyOp)
        {
            methodBodyOp.BlockBody.ShouldNotBeNull("MethodBodyOperation should have BlockBody");
            methodBodyOp.BlockBody!.Operations.Length.ShouldBeGreaterThan(0, "BlockBody should have operations");
        }

        // Assert that we can understand the structure
        bodyOperation.ShouldNotBeNull("Should get some operation from method body");
    }

    [Fact] 
    public async Task DiagnoseOperationTypes_EmptyMethod_ShouldShowStructure()
    {
        // Arrange
        var sampleCode = @"
            public class TestClass 
            {
                public void EmptyMethod() { }
            }";

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        var root = await syntaxTree.GetRootAsync(TestContext.Current.CancellationToken);
        var method = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First();

        var semanticModel = compilation.GetSemanticModel(syntaxTree);

        // Act
        var bodyOperation = semanticModel.GetOperation(method.Body!, TestContext.Current.CancellationToken);

        // Diagnose empty method structure
        bodyOperation.ShouldNotBeNull("Empty method should still have operation");

        if (bodyOperation is IMethodBodyOperation methodBodyOp)
        {
            // Empty methods should still have a BlockBody, just with 0 operations
            methodBodyOp.BlockBody.ShouldNotBeNull("Even empty methods should have BlockBody");
        }
    }

    [Fact]
    public async Task DiagnoseOperationTypes_ExpressionMethod_ShouldShowStructure()
    {
        // Arrange
        var sampleCode = @"
            public class TestClass 
            {
                public int ExpressionMethod() => 42;
            }";

        var (compilation, syntaxTree) = CompilationFactory.CreateBasic(sampleCode);
        var root = await syntaxTree.GetRootAsync(TestContext.Current.CancellationToken);
        var method = root.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .First();

        var semanticModel = compilation.GetSemanticModel(syntaxTree);

        // Act - Check both body types
        var bodyOperation = method.Body != null 
            ? semanticModel.GetOperation(method.Body, TestContext.Current.CancellationToken)
            : null;
            
        var expressionOperation = method.ExpressionBody != null
            ? semanticModel.GetOperation(method.ExpressionBody, TestContext.Current.CancellationToken)
            : null;

        // Diagnose expression method structure
        bodyOperation.ShouldBeNull("Expression-bodied methods should not have Body");
        expressionOperation.ShouldNotBeNull("Expression-bodied methods should have ExpressionBody operation");
    }
}