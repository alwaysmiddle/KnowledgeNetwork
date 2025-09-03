using KnowledgeNetwork.Domains.Code.Models.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Classes.Utilities;

/// <summary>
/// Common syntax helper utilities
/// </summary>
public interface ISyntaxUtilities
{
    /// <summary>
    /// Gets location information from a syntax node
    /// </summary>
    /// <param name="node">The syntax node to get location for</param>
    /// <param name="fallbackFilePath">Fallback file path if syntax tree doesn't have one</param>
    /// <returns>Location information</returns>
    CSharpLocationInfo GetLocationInfo(SyntaxNode? node, string? fallbackFilePath = null);

    /// <summary>
    /// Gets all methods from a type declaration
    /// </summary>
    /// <param name="typeDeclaration">The type declaration to extract methods from</param>
    /// <returns>Enumerable of method declarations</returns>
    IEnumerable<MethodDeclarationSyntax> GetMethods(BaseTypeDeclarationSyntax typeDeclaration);

    /// <summary>
    /// Gets all members from a type declaration
    /// </summary>
    /// <param name="typeDeclaration">The type declaration to extract members from</param>
    /// <returns>Enumerable of member declarations</returns>
    IEnumerable<MemberDeclarationSyntax> GetAllMembers(BaseTypeDeclarationSyntax typeDeclaration);
}