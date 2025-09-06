using KnowledgeNetwork.Domains.Code.Models.Classes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Classes.Abstractions;

/// <summary>
/// Calculates complexity metrics for classes and methods
/// </summary>
public interface IComplexityCalculator
{
    /// <summary>
    /// Calculates complexity metrics for a class
    /// </summary>
    /// <param name="typeDeclaration">The type declaration to analyze</param>
    /// <param name="semanticModel">The semantic model for symbol analysis</param>
    /// <returns>Calculated complexity metrics</returns>
    Task<ClassComplexityMetrics> CalculateClassComplexityAsync(
        BaseTypeDeclarationSyntax typeDeclaration, 
        SemanticModel semanticModel);
}