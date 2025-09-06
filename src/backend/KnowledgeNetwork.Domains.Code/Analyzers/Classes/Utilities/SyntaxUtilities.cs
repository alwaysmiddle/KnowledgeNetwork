using KnowledgeNetwork.Domains.Code.Analyzers.Classes.Abstractions;
using KnowledgeNetwork.Domains.Code.Models.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Classes.Utilities;

/// <summary>
/// Common syntax helper utilities
/// </summary>
public class SyntaxUtilities : ISyntaxUtilities
{
    /// <summary>
    /// Gets location information from a syntax node
    /// </summary>
    public CSharpLocationInfo GetLocationInfo(SyntaxNode? node, string? fallbackFilePath = null)
    {
        if (node == null) return new CSharpLocationInfo { FilePath = fallbackFilePath ?? "" };

        var location = node.GetLocation();
        var span = location.GetLineSpan();

        // Use fallback file path if syntax tree doesn't have one
        var effectiveFilePath = !string.IsNullOrWhiteSpace(span.Path) 
            ? span.Path 
            : fallbackFilePath ?? "";

        return new CSharpLocationInfo
        {
            FilePath = effectiveFilePath,
            StartLine = span.StartLinePosition.Line + 1,
            StartColumn = span.StartLinePosition.Character + 1,
            EndLine = span.EndLinePosition.Line + 1,
            EndColumn = span.EndLinePosition.Character + 1
        };
    }

    /// <summary>
    /// Gets all methods from a type declaration
    /// </summary>
    public IEnumerable<MethodDeclarationSyntax> GetMethods(BaseTypeDeclarationSyntax typeDeclaration)
    {
        return typeDeclaration switch
        {
            ClassDeclarationSyntax classDecl => classDecl.Members.OfType<MethodDeclarationSyntax>(),
            StructDeclarationSyntax structDecl => structDecl.Members.OfType<MethodDeclarationSyntax>(),
            InterfaceDeclarationSyntax interfaceDecl => interfaceDecl.Members.OfType<MethodDeclarationSyntax>(),
            RecordDeclarationSyntax recordDecl => recordDecl.Members.OfType<MethodDeclarationSyntax>(),
            _ => []
        };
    }

    /// <summary>
    /// Gets all members from a type declaration
    /// </summary>
    public IEnumerable<MemberDeclarationSyntax> GetAllMembers(BaseTypeDeclarationSyntax typeDeclaration)
    {
        return typeDeclaration switch
        {
            ClassDeclarationSyntax classDecl => classDecl.Members,
            StructDeclarationSyntax structDecl => structDecl.Members,
            InterfaceDeclarationSyntax interfaceDecl => interfaceDecl.Members,
            RecordDeclarationSyntax recordDecl => recordDecl.Members,
            _ => Enumerable.Empty<MemberDeclarationSyntax>()
        };
    }
}