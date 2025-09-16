using KnowledgeNetwork.Domains.Code.Analyzers.Files.Abstractions;
using KnowledgeNetwork.Domains.Code.Models.Common;
using KnowledgeNetwork.Domains.Code.Models.Files;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Files.Extractors;

/// <summary>
/// Extracts declared and referenced types from C# syntax trees
/// </summary>
public class TypeExtractor(IFileSyntaxUtilities syntaxUtilities) : ITypeExtractor
{
    /// <summary>
    /// Extracts declared types from a syntax tree and populates the file node
    /// </summary>
    public async Task ExtractDeclaredTypesAsync(FileNode fileNode, SyntaxNode root, SemanticModel semanticModel)
    {
        var typeDeclarations = root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>();
        
        foreach (var typeDeclaration in typeDeclarations)
        {
            if (semanticModel.GetDeclaredSymbol(typeDeclaration) is not INamedTypeSymbol typeSymbol) continue;
            
            var declaredType = new DeclaredType
            {
                Name = typeSymbol.Name,
                FullName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                Namespace = typeSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty,
                TypeKind = typeSymbol.TypeKind.ToString(),
                Visibility = typeSymbol.DeclaredAccessibility.ToString(),
                IsStatic = typeSymbol.IsStatic,
                IsGeneric = typeSymbol.IsGenericType,
                Location = syntaxUtilities.GetLocationInfo(typeDeclaration)
            };

            fileNode.DeclaredTypes.Add(declaredType);
        }
    }

    /// <summary>
    /// Extracts referenced types from a syntax tree and populates the file node
    /// </summary>
    public async Task ExtractReferencedTypesAsync(FileNode fileNode, SyntaxNode root, SemanticModel semanticModel)
    {
        var referencedTypes = new Dictionary<string, ReferencedType>();

        // Find all identifier names (type references)
        var identifierNodes = root.DescendantNodes().OfType<IdentifierNameSyntax>();
        
        foreach (var identifier in identifierNodes)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(identifier);
            if (symbolInfo.Symbol is not ITypeSymbol typeSymbol) continue;
            
            var fullName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                
            // Skip primitive types and declared types in the same file
            if (typeSymbol.SpecialType != SpecialType.None || 
                fileNode.DeclaredTypes.Any(dt => dt.FullName == fullName))
                continue;

            if (!referencedTypes.TryGetValue(fullName, out var value))
            {
                var referencedType = new ReferencedType
                {
                    Name = typeSymbol.Name,
                    FullName = fullName,
                    Namespace = typeSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty,
                    Assembly = typeSymbol.ContainingAssembly?.Name ?? string.Empty,
                    ReferenceKind = DetermineReferenceKind(identifier, semanticModel),
                    IsExternal = typeSymbol.ContainingAssembly != semanticModel.Compilation.Assembly,
                    ReferenceCount = 1,
                    ReferenceLocations = new List<CSharpLocationInfo> { syntaxUtilities.GetLocationInfo(identifier) }
                };

                referencedTypes.Add(fullName, referencedType);
            }
            else
            {
                value.ReferenceCount++;
                value.ReferenceLocations.Add(syntaxUtilities.GetLocationInfo(identifier));
            }
        }

        fileNode.ReferencedTypes = referencedTypes.Values.ToList();
    }

    /// <summary>
    /// Determines how a type is being referenced
    /// </summary>
    private TypeReferenceKind DetermineReferenceKind(SyntaxNode node, SemanticModel semanticModel)
    {
        var parent = node.Parent;
        
        return parent switch
        {
            BaseListSyntax => TypeReferenceKind.Inheritance,
            AttributeSyntax => TypeReferenceKind.Attribute,
            GenericNameSyntax => TypeReferenceKind.GenericParameter,
            TypeOfExpressionSyntax => TypeReferenceKind.Reflection,
            _ => TypeReferenceKind.Direct
        };
    }
}