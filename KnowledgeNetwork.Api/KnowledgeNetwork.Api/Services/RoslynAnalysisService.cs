using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using KnowledgeNetwork.Api.Models;

namespace KnowledgeNetwork.Api.Services;

/// <summary>
/// Service for analyzing C# code using Roslyn compiler APIs to extract structural information
/// </summary>
public class RoslynAnalysisService : IRoslynAnalysisService
{
    /// <summary>
    /// Analyzes C# code and extracts structural information including classes, methods, and their relationships
    /// </summary>
    public async Task<CodeAnalysisResponse> AnalyzeCodeAsync(CodeAnalysisRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return new CodeAnalysisResponse
            {
                Success = false,
                ErrorMessage = "Source code cannot be null or empty"
            };
        }

        try
        {
            // Parse the source code into a syntax tree
            var syntaxTree = CSharpSyntaxTree.ParseText(request.Code, cancellationToken: cancellationToken);
            var root = await syntaxTree.GetRootAsync(cancellationToken);

            // Check for syntax errors
            var diagnostics = syntaxTree.GetDiagnostics();
            var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            
            if (errors.Any())
            {
                return new CodeAnalysisResponse
                {
                    Success = false,
                    ErrorMessage = $"Syntax errors found: {string.Join("; ", errors.Select(e => e.GetMessage()))}",
                    Diagnostics = errors.Select(e => e.GetMessage()).ToList()
                };
            }

            // Extract code elements
            var classes = ExtractClasses(root);
            var methods = ExtractMethods(root);
            var properties = ExtractProperties(root);
            var fields = ExtractFields(root);

            return new CodeAnalysisResponse
            {
                Success = true,
                Details = new CodeAnalysisDetails
                {
                    Classes = classes,
                    Methods = methods,
                    Properties = properties,
                    Fields = fields
                },
                Summary = new CodeAnalysisSummary
                {
                    ClassCount = classes.Count,
                    MethodCount = methods.Count,
                    PropertyCount = properties.Count,
                    FieldCount = fields.Count,
                    LinesOfCode = request.Code.Split('\n').Length
                }
            };
        }
        catch (Exception ex)
        {
            return new CodeAnalysisResponse
            {
                Success = false,
                ErrorMessage = $"Analysis failed: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Validates that the provided source code is syntactically correct C# code
    /// </summary>
    public bool ValidateSourceCode(string sourceCode)
    {
        if (string.IsNullOrWhiteSpace(sourceCode))
            return false;

        try
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            var diagnostics = syntaxTree.GetDiagnostics();
            return !diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Extracts class information from the syntax tree
    /// </summary>
    private List<ClassElement> ExtractClasses(SyntaxNode root)
    {
        var classes = new List<ClassElement>();
        
        var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
        
        foreach (var classDecl in classDeclarations)
        {
            var classElement = new ClassElement
            {
                Name = classDecl.Identifier.ValueText,
                FullName = GetFullTypeName(classDecl),
                LineNumber = classDecl.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                AccessModifier = GetAccessModifier(classDecl.Modifiers),
                IsStatic = classDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)),
                IsAbstract = classDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword)),
                IsSealed = classDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.SealedKeyword)),
                Namespace = GetNamespace(classDecl)
            };

            // Extract base class and interfaces
            if (classDecl.BaseList != null)
            {
                foreach (var baseType in classDecl.BaseList.Types)
                {
                    var typeName = baseType.Type.ToString();
                    if (baseType == classDecl.BaseList.Types.First() && !typeName.StartsWith("I"))
                    {
                        classElement.BaseClass = typeName;
                    }
                    else
                    {
                        classElement.Interfaces.Add(typeName);
                    }
                }
            }

            classes.Add(classElement);
        }
        
        return classes;
    }

    /// <summary>
    /// Extracts method information from the syntax tree
    /// </summary>
    private List<MethodElement> ExtractMethods(SyntaxNode root)
    {
        var methods = new List<MethodElement>();
        
        var methodDeclarations = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
        
        foreach (var methodDecl in methodDeclarations)
        {
            var methodElement = new MethodElement
            {
                Name = methodDecl.Identifier.ValueText,
                FullName = GetFullMethodName(methodDecl),
                LineNumber = methodDecl.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                AccessModifier = GetAccessModifier(methodDecl.Modifiers),
                ReturnType = methodDecl.ReturnType.ToString(),
                IsStatic = methodDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)),
                IsVirtual = methodDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.VirtualKeyword)),
                IsOverride = methodDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword)),
                IsAsync = methodDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword)),
                ContainingClass = GetContainingClassName(methodDecl),
                Parameters = ExtractParameters(methodDecl.ParameterList)
            };

            methods.Add(methodElement);
        }
        
        return methods;
    }

    /// <summary>
    /// Extracts property information from the syntax tree
    /// </summary>
    private List<PropertyElement> ExtractProperties(SyntaxNode root)
    {
        var properties = new List<PropertyElement>();
        
        var propertyDeclarations = root.DescendantNodes().OfType<PropertyDeclarationSyntax>();
        
        foreach (var propDecl in propertyDeclarations)
        {
            var propertyElement = new PropertyElement
            {
                Name = propDecl.Identifier.ValueText,
                FullName = GetFullPropertyName(propDecl),
                LineNumber = propDecl.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                AccessModifier = GetAccessModifier(propDecl.Modifiers),
                Type = propDecl.Type.ToString(),
                HasGetter = propDecl.AccessorList?.Accessors.Any(a => a.IsKind(SyntaxKind.GetAccessorDeclaration)) ?? false,
                HasSetter = propDecl.AccessorList?.Accessors.Any(a => a.IsKind(SyntaxKind.SetAccessorDeclaration)) ?? false,
                IsStatic = propDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)),
                IsVirtual = propDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.VirtualKeyword)),
                IsOverride = propDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword)),
                ContainingClass = GetContainingClassName(propDecl)
            };

            properties.Add(propertyElement);
        }
        
        return properties;
    }

    /// <summary>
    /// Extracts field information from the syntax tree
    /// </summary>
    private List<FieldElement> ExtractFields(SyntaxNode root)
    {
        var fields = new List<FieldElement>();
        
        var fieldDeclarations = root.DescendantNodes().OfType<FieldDeclarationSyntax>();
        
        foreach (var fieldDecl in fieldDeclarations)
        {
            foreach (var variable in fieldDecl.Declaration.Variables)
            {
                var fieldElement = new FieldElement
                {
                    Name = variable.Identifier.ValueText,
                    FullName = GetFullFieldName(fieldDecl, variable),
                    LineNumber = fieldDecl.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                    AccessModifier = GetAccessModifier(fieldDecl.Modifiers),
                    Type = fieldDecl.Declaration.Type.ToString(),
                    IsStatic = fieldDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword)),
                    IsReadOnly = fieldDecl.Modifiers.Any(m => m.IsKind(SyntaxKind.ReadOnlyKeyword)),
                    ContainingClass = GetContainingClassName(fieldDecl)
                };

                fields.Add(fieldElement);
            }
        }
        
        return fields;
    }

    /// <summary>
    /// Extracts parameter information from a parameter list
    /// </summary>
    private List<ParameterElement> ExtractParameters(ParameterListSyntax? parameterList)
    {
        var parameters = new List<ParameterElement>();
        
        if (parameterList?.Parameters != null)
        {
            foreach (var param in parameterList.Parameters)
            {
                parameters.Add(new ParameterElement
                {
                    Name = param.Identifier.ValueText,
                    Type = param.Type?.ToString() ?? "var",
                    HasDefaultValue = param.Default != null,
                    DefaultValue = param.Default?.Value.ToString(),
                    Modifier = string.Join(" ", param.Modifiers.Select(m => m.ValueText))
                });
            }
        }
        
        return parameters;
    }

    private string GetAccessModifier(SyntaxTokenList modifiers)
    {
        if (modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword))) return "public";
        if (modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword))) return "private";
        if (modifiers.Any(m => m.IsKind(SyntaxKind.ProtectedKeyword))) return "protected";
        if (modifiers.Any(m => m.IsKind(SyntaxKind.InternalKeyword))) return "internal";
        return "private"; // default
    }

    private string GetNamespace(SyntaxNode node)
    {
        var namespaceDecl = node.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
        return namespaceDecl?.Name.ToString() ?? string.Empty;
    }

    private string GetFullTypeName(ClassDeclarationSyntax classDecl)
    {
        var namespaceName = GetNamespace(classDecl);
        return string.IsNullOrEmpty(namespaceName) ? classDecl.Identifier.ValueText : $"{namespaceName}.{classDecl.Identifier.ValueText}";
    }

    private string GetContainingClassName(SyntaxNode node)
    {
        var classDecl = node.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        return classDecl?.Identifier.ValueText ?? string.Empty;
    }

    private string GetFullMethodName(MethodDeclarationSyntax methodDecl)
    {
        var className = GetContainingClassName(methodDecl);
        var namespaceName = GetNamespace(methodDecl);
        var fullClassName = string.IsNullOrEmpty(namespaceName) ? className : $"{namespaceName}.{className}";
        return string.IsNullOrEmpty(fullClassName) ? methodDecl.Identifier.ValueText : $"{fullClassName}.{methodDecl.Identifier.ValueText}";
    }

    private string GetFullPropertyName(PropertyDeclarationSyntax propDecl)
    {
        var className = GetContainingClassName(propDecl);
        var namespaceName = GetNamespace(propDecl);
        var fullClassName = string.IsNullOrEmpty(namespaceName) ? className : $"{namespaceName}.{className}";
        return string.IsNullOrEmpty(fullClassName) ? propDecl.Identifier.ValueText : $"{fullClassName}.{propDecl.Identifier.ValueText}";
    }

    private string GetFullFieldName(FieldDeclarationSyntax fieldDecl, VariableDeclaratorSyntax variable)
    {
        var className = GetContainingClassName(fieldDecl);
        var namespaceName = GetNamespace(fieldDecl);
        var fullClassName = string.IsNullOrEmpty(namespaceName) ? className : $"{namespaceName}.{className}";
        return string.IsNullOrEmpty(fullClassName) ? variable.Identifier.ValueText : $"{fullClassName}.{variable.Identifier.ValueText}";
    }
}