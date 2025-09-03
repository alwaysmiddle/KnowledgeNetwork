using KnowledgeNetwork.Domains.Code.Models.Classes;
using KnowledgeNetwork.Domains.Code.Models.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Classes.Utilities;

/// <summary>
/// Factory for creating ClassNode instances from syntax declarations
/// </summary>
public class ClassNodeFactory(
    ILogger<ClassNodeFactory> logger,
    IComplexityCalculator complexityCalculator,
    ISyntaxUtilities syntaxUtilities) : IClassNodeFactory
{
    private readonly ILogger<ClassNodeFactory> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IComplexityCalculator _complexityCalculator = complexityCalculator ?? throw new ArgumentNullException(nameof(complexityCalculator));
    private readonly ISyntaxUtilities _syntaxUtilities = syntaxUtilities ?? throw new ArgumentNullException(nameof(syntaxUtilities));

    /// <summary>
    /// Creates a class node from a type declaration
    /// </summary>
    public async Task<ClassNode?> CreateClassNodeAsync(
        SemanticModel semanticModel, 
        BaseTypeDeclarationSyntax typeDeclaration, 
        string effectiveFileName)
    {
        try
        {
            var symbol = semanticModel.GetDeclaredSymbol(typeDeclaration);
            if (symbol == null) return null;

            var classNode = new ClassNode
            {
                Name = symbol.Name,
                FullName = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                Namespace = symbol.ContainingNamespace?.ToDisplayString() ?? string.Empty,
                Location = _syntaxUtilities.GetLocationInfo(typeDeclaration, effectiveFileName)
            };

            // Set class type and modifiers
            SetClassTypeAndModifiers(classNode, typeDeclaration, symbol);

            // Extract base class and interfaces
            ExtractBaseClassAndInterfaces(classNode, typeDeclaration, symbol);

            // Calculate member summary
            CalculateMemberSummary(classNode, typeDeclaration);

            // Calculate complexity metrics
            await CalculateComplexityMetricsAsync(classNode, typeDeclaration, semanticModel);

            return classNode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create class node for type declaration");
            return null;
        }
    }

    /// <summary>
    /// Sets the class type and modifiers
    /// </summary>
    private void SetClassTypeAndModifiers(ClassNode classNode, BaseTypeDeclarationSyntax typeDeclaration, ISymbol symbol)
    {
        // Set visibility
        classNode.Visibility = symbol.DeclaredAccessibility switch
        {
            Accessibility.Public => ClassVisibility.Public,
            Accessibility.Internal => ClassVisibility.Internal,
            Accessibility.Protected => ClassVisibility.Protected,
            Accessibility.Private => ClassVisibility.Private,
            Accessibility.ProtectedOrInternal => ClassVisibility.ProtectedInternal,
            Accessibility.ProtectedAndInternal => ClassVisibility.PrivateProtected,
            _ => ClassVisibility.Internal
        };

        // Set class type
        classNode.ClassType = typeDeclaration switch
        {
            ClassDeclarationSyntax => ClassType.Class,
            InterfaceDeclarationSyntax => ClassType.Interface,
            StructDeclarationSyntax => ClassType.Struct,
            EnumDeclarationSyntax => ClassType.Enum,
            RecordDeclarationSyntax record when record.ClassOrStructKeyword.IsKind(SyntaxKind.StructKeyword) => ClassType.RecordStruct,
            RecordDeclarationSyntax => ClassType.Record,
            _ => ClassType.Class
        };

        // Set modifiers
        var modifiers = typeDeclaration.Modifiers;
        classNode.IsStatic = modifiers.Any(SyntaxKind.StaticKeyword);
        classNode.IsAbstract = modifiers.Any(SyntaxKind.AbstractKeyword);
        classNode.IsSealed = modifiers.Any(SyntaxKind.SealedKeyword);
        classNode.IsPartial = modifiers.Any(SyntaxKind.PartialKeyword);

        // Check if nested
        classNode.IsNested = symbol.ContainingType != null;

        // Check if generic
        if (symbol is INamedTypeSymbol namedType)
        {
            classNode.IsGeneric = namedType.IsGenericType;
            if (classNode.IsGeneric)
            {
                classNode.GenericTypeParameters = namedType.TypeParameters
                    .Select(tp => tp.Name)
                    .ToList();
            }
        }
    }

    /// <summary>
    /// Extracts base class and interface information
    /// </summary>
    private void ExtractBaseClassAndInterfaces(ClassNode classNode, BaseTypeDeclarationSyntax typeDeclaration, ISymbol symbol)
    {
        if (symbol is INamedTypeSymbol namedType)
        {
            // Base class
            if (namedType.BaseType != null && namedType.BaseType.SpecialType != SpecialType.System_Object)
            {
                classNode.BaseClassName = namedType.BaseType.ToDisplayString();
            }

            // Interfaces
            classNode.ImplementedInterfaces = namedType.Interfaces
                .Select(i => i.ToDisplayString())
                .ToList();
        }
    }

    /// <summary>
    /// Calculates member summary
    /// </summary>
    private void CalculateMemberSummary(ClassNode classNode, BaseTypeDeclarationSyntax typeDeclaration)
    {
        SyntaxList<MemberDeclarationSyntax> members = default;
        
        // Get members based on the specific type
        switch (typeDeclaration)
        {
            case ClassDeclarationSyntax classDecl:
                members = classDecl.Members;
                break;
            case StructDeclarationSyntax structDecl:
                members = structDecl.Members;
                break;
            case InterfaceDeclarationSyntax interfaceDecl:
                members = interfaceDecl.Members;
                break;
            case RecordDeclarationSyntax recordDecl:
                members = recordDecl.Members;
                break;
            case EnumDeclarationSyntax enumDecl:
                // Enums don't have regular members, just enum members
                classNode.MemberSummary = new ClassMemberSummary
                {
                    FieldCount = enumDecl.Members.Count // Enum values are technically fields
                };
                return;
        }
        
        classNode.MemberSummary = new ClassMemberSummary
        {
            FieldCount = members.OfType<FieldDeclarationSyntax>().Count(),
            PropertyCount = members.OfType<PropertyDeclarationSyntax>().Count(),
            MethodCount = members.OfType<MethodDeclarationSyntax>().Count(),
            ConstructorCount = members.OfType<ConstructorDeclarationSyntax>().Count(),
            NestedTypeCount = members.OfType<BaseTypeDeclarationSyntax>().Count(),
            EventCount = members.OfType<EventDeclarationSyntax>().Count() + members.OfType<EventFieldDeclarationSyntax>().Count(),
            IndexerCount = members.OfType<IndexerDeclarationSyntax>().Count(),
            OperatorCount = members.OfType<OperatorDeclarationSyntax>().Count() + members.OfType<ConversionOperatorDeclarationSyntax>().Count()
        };
    }

    /// <summary>
    /// Calculates complexity metrics for the class
    /// </summary>
    private async Task CalculateComplexityMetricsAsync(ClassNode classNode, BaseTypeDeclarationSyntax typeDeclaration, SemanticModel semanticModel)
    {
        var metrics = await _complexityCalculator.CalculateClassComplexityAsync(typeDeclaration, semanticModel);
        classNode.Metrics = metrics;
    }
}