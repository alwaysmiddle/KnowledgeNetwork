using KnowledgeNetwork.Domains.Code.Analyzers.Blocks.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.Extensions.Logging;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Blocks;

/// <summary>
/// Extracts ControlFlowGraph directly from C# method declarations using Roslyn APIs.
/// Combines operation extraction and CFG creation in one cohesive service.
/// </summary>
public class RoslynCfgExtractor(ILogger<RoslynCfgExtractor> logger) : IRoslynCfgExtractor
{
    /// <summary>
    /// Extract ControlFlowGraph from a method declaration
    /// </summary>
    /// <param name="compilation">Compilation context</param>
    /// <param name="methodDeclaration">Method syntax node</param>
    /// <returns>ControlFlowGraph or null if extraction fails</returns>
    public async Task<ControlFlowGraph?> ExtractControlFlowGraphAsync(Compilation compilation, MethodDeclarationSyntax methodDeclaration)
    {
        ArgumentNullException.ThrowIfNull(compilation);
        ArgumentNullException.ThrowIfNull(methodDeclaration);

        await Task.CompletedTask; // Maintain async signature

        try
        {
            logger.LogDebug("Extracting CFG from method {MethodName}", methodDeclaration.Identifier);

            // Phase 1: Get semantic model and validate method symbol
            var semanticModel = compilation.GetSemanticModel(methodDeclaration.SyntaxTree);
            var methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration);
            if (methodSymbol is null)
            {
                logger.LogWarning("Failed to get method symbol for {MethodName}", methodDeclaration.Identifier);
                return null;
            }

            // Phase 2: Get method body operation (the root operation)
            var methodBodyOperation = semanticModel.GetOperation(methodDeclaration);
            if (methodBodyOperation is not IMethodBodyOperation methodBody)
            {
                logger.LogWarning("Method {MethodName} did not produce IMethodBodyOperation. Got: {OperationType}",
                    methodDeclaration.Identifier, methodBodyOperation?.GetType().Name ?? "null");
                return null;
            }

            // Validate that we have a block body (not expression body for now)
            if (methodBody.BlockBody is null && methodDeclaration.ExpressionBody is null)
            {
                logger.LogDebug("Method {MethodName} has no body, skipping CFG creation",
                    methodDeclaration.Identifier);
                return null;
            }

            // Phase 3: Create ControlFlowGraph from the root method body operation
            logger.LogDebug("Creating CFG from IMethodBodyOperation for method {MethodName}",
                methodDeclaration.Identifier);

            var cfg = ControlFlowGraph.Create(methodBody);

            if (cfg is null) //Defensive Programming, Microsoft could return null one day for edge cases.
            {
                logger.LogWarning("ControlFlowGraph.Create returned null for method {MethodName}",
                    methodDeclaration.Identifier);
                return null;
            }

            logger.LogDebug("Successfully created CFG with {BlockCount} blocks for method {MethodName}",
                cfg.Blocks.Length, methodDeclaration.Identifier);

            return cfg;
        }
        catch (ArgumentException ex)
        {
            logger.LogError(ex, "Invalid operation provided to ControlFlowGraph.Create for method {MethodName}",
                methodDeclaration.Identifier);
            return null;
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Invalid operation structure for CFG creation in method {MethodName}",
                methodDeclaration.Identifier);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during CFG extraction for method {MethodName}",
                methodDeclaration.Identifier);
            return null;
        }
    }

    /// <summary>
    /// Extract ControlFlowGraph from a constructor declaration
    /// </summary>
    /// <param name="compilation">Compilation context</param>
    /// <param name="constructorDeclaration">Constructor syntax node</param>
    /// <returns>ControlFlowGraph or null if extraction fails</returns>
    public async Task<ControlFlowGraph?> ExtractControlFlowGraphAsync(Compilation compilation, ConstructorDeclarationSyntax constructorDeclaration)
    {
        ArgumentNullException.ThrowIfNull(compilation);
        ArgumentNullException.ThrowIfNull(constructorDeclaration);
        
        await Task.CompletedTask; // Maintain async signature

        try
        {
            logger.LogDebug("Extracting CFG from constructor {ConstructorName}", constructorDeclaration.Identifier);

            var semanticModel = compilation.GetSemanticModel(constructorDeclaration.SyntaxTree);
            var constructorSymbol = semanticModel.GetDeclaredSymbol(constructorDeclaration);
            if (constructorSymbol is null)
            {
                logger.LogWarning("Failed to get constructor symbol for {ConstructorName}", constructorDeclaration.Identifier);
                return null;
            }

            var constructorBodyOperation = semanticModel.GetOperation(constructorDeclaration);
            if (constructorBodyOperation is not IConstructorBodyOperation constructorBody)
            {
                logger.LogWarning(
                    "Constructor {ConstructorName} did not produce IConstructorBodyOperation. Got: {OperationType}",
                    constructorDeclaration.Identifier, constructorBodyOperation?.GetType().Name ?? "null");
                return null;
            }

            // Validate that we have either block body or expression body
            if (constructorBody.BlockBody is null && constructorDeclaration.ExpressionBody is null)
            {
                logger.LogDebug("Constructor {ConstructorName} has no body, skipping CFG creation",
                    constructorDeclaration.Identifier);
                return null;
            }

            logger.LogDebug("Creating CFG from IConstructorBodyOperation for constructor {ConstructorName}", 
                constructorDeclaration.Identifier);

            var cfg = ControlFlowGraph.Create(constructorBody);

            if (cfg is null)
            {
                logger.LogWarning("ControlFlowGraph.Create returned null for constructor {ConstructorName}",
                    constructorDeclaration.Identifier);
                return null;
            }

            logger.LogDebug("Successfully created CFG with {BlockCount} blocks for constructor {ConstructorName}",
                cfg.Blocks.Length, constructorDeclaration.Identifier);

            return cfg;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during CFG extraction for constructor {ConstructorName}",
                constructorDeclaration.Identifier);
            return null;
        }
    }
}