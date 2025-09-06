using KnowledgeNetwork.Domains.Code.Models.Classes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Classes.Abstractions;

/// <summary>
/// Factory for creating ClassNode instances from syntax declarations
/// </summary>
public interface IClassNodeFactory
{
    /// <summary>
    /// Creates a class node from a type declaration
    /// </summary>
    /// <param name="semanticModel">The semantic model for symbol analysis</param>
    /// <param name="typeDeclaration">The type declaration syntax</param>
    /// <param name="effectiveFileName">The effective file name for location info</param>
    /// <returns>A populated ClassNode instance, or null if creation fails</returns>
    Task<ClassNode?> CreateClassNodeAsync(
        SemanticModel semanticModel, 
        BaseTypeDeclarationSyntax typeDeclaration, 
        string effectiveFileName);
}