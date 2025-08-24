using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.Extensions.Logging;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Blocks;

/// <summary>
/// Implementation of IRoslynOperationExtractor that handles extraction of IBlockOperation
/// from various Roslyn syntax structures. This service encapsulates the complexity of
/// working with Roslyn's semantic model and operation tree.
/// </summary>
public class RoslynOperationExtractor(ILogger<RoslynOperationExtractor> logger) : IRoslynOperationExtractor
{
    private readonly ILogger<RoslynOperationExtractor> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Extract IBlockOperation from a method declaration
    /// </summary>
    public async Task<IBlockOperation?> ExtractFromMethodAsync(
        Compilation compilation, 
        MethodDeclarationSyntax methodDeclaration)
    {
        await Task.CompletedTask; // Maintain async signature

        try
        {
            // Get semantic model for the syntax tree
            var semanticModel = compilation.GetSemanticModel(methodDeclaration.SyntaxTree);

            // Get the method symbol for validation
            var methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration);
            if (methodSymbol == null)
            {
                _logger.LogWarning("Failed to get method symbol for {MethodName}", methodDeclaration.Identifier);
                return null;
            }

            _logger.LogDebug("Extracting operation from method {MethodName}", methodDeclaration.Identifier);

            // Get the method body (either block body or expression body)
            var bodyNode = methodDeclaration.Body ?? (SyntaxNode?)methodDeclaration.ExpressionBody;
            if (bodyNode == null)
            {
                _logger.LogDebug("Method {MethodName} has no body, cannot extract operation", 
                    methodDeclaration.Identifier);
                return null;
            }

            return await ExtractFromBodyAsync(compilation, bodyNode, methodDeclaration.Identifier.ValueText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract operation from method {MethodName}", 
                methodDeclaration.Identifier);
            return null;
        }
    }

    /// <summary>
    /// Extract IBlockOperation from a constructor declaration  
    /// </summary>
    public async Task<IBlockOperation?> ExtractFromConstructorAsync(
        Compilation compilation, 
        ConstructorDeclarationSyntax constructorDeclaration)
    {
        await Task.CompletedTask; // Maintain async signature

        try
        {
            // Get semantic model for the syntax tree
            var semanticModel = compilation.GetSemanticModel(constructorDeclaration.SyntaxTree);

            // Get the constructor symbol for validation
            var constructorSymbol = semanticModel.GetDeclaredSymbol(constructorDeclaration);
            if (constructorSymbol == null)
            {
                _logger.LogWarning("Failed to get constructor symbol");
                return null;
            }

            _logger.LogDebug("Extracting operation from constructor");

            // Get the constructor body (either block body or expression body)
            var bodyNode = constructorDeclaration.Body ?? (SyntaxNode?)constructorDeclaration.ExpressionBody;
            if (bodyNode == null)
            {
                _logger.LogDebug("Constructor has no body, cannot extract operation");
                return null;
            }

            return await ExtractFromBodyAsync(compilation, bodyNode, ".ctor");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract operation from constructor");
            return null;
        }
    }

    /// <summary>
    /// Extract IBlockOperation from any syntax node representing a method body
    /// </summary>
    public async Task<IBlockOperation?> ExtractFromBodyAsync(
        Compilation compilation, 
        SyntaxNode bodyNode,
        string memberName)
    {
        await Task.CompletedTask; // Maintain async signature

        try
        {
            // Get semantic model from the body node's syntax tree
            var semanticModel = compilation.GetSemanticModel(bodyNode.SyntaxTree);

            // Get the operation from the body node
            var operation = semanticModel.GetOperation(bodyNode);
            if (operation == null)
            {
                _logger.LogWarning("Failed to get operation for member {MemberName}", memberName);
                return null;
            }

            _logger.LogDebug("Got operation type {OperationType} for member {MemberName}", 
                operation.GetType().Name, memberName);

            // ControlFlowGraph.Create requires IBlockOperation specifically
            // Handle the various operation types that can represent method bodies
            var blockOperation = operation switch
            {
                // Direct block operation (method with { } body)
                IBlockOperation directBlock => directBlock,
                
                // Method body operation containing a block
                IMethodBodyOperation { BlockBody: not null } methodBodyOp => methodBodyOp.BlockBody,
                
                // Cannot extract IBlockOperation from other types
                _ => null
            };

            if (blockOperation == null)
            {
                _logger.LogWarning("No valid IBlockOperation found for member {MemberName}. " +
                    "Operation type: {OperationType}", memberName, operation.GetType().Name);
                return null;
            }

            _logger.LogDebug("Successfully extracted IBlockOperation for member {MemberName}", memberName);
            return blockOperation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract IBlockOperation from body for member {MemberName}", memberName);
            return null;
        }
    }
}