using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using KnowledgeNetwork.Domains.Code.Models.Common;
using KnowledgeNetwork.Domains.Code.Models.Methods;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Methods;

/// <summary>
/// Analyzer for extracting method-level relationships within a C# class.
/// This analyzer identifies how methods call each other, access fields/properties, and form constructor chains.
/// </summary>
public class CSharpMethodRelationshipAnalyzer
{
    private readonly ILogger<CSharpMethodRelationshipAnalyzer> _logger;

    /// <summary>
    /// Initializes a new instance of the CSharpMethodRelationshipAnalyzer
    /// </summary>
    /// <param name="logger">Logger instance for diagnostic output</param>
    public CSharpMethodRelationshipAnalyzer(ILogger<CSharpMethodRelationshipAnalyzer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Analyze method relationships within a class declaration
    /// </summary>
    /// <param name="compilation">Compilation context</param>
    /// <param name="classDeclaration">Class syntax node</param>
    /// <returns>Method relationship graph or null if analysis fails</returns>
    public async Task<MethodRelationshipGraph?> AnalyzeClassAsync(
        Compilation compilation,
        ClassDeclarationSyntax classDeclaration)
    {
        await Task.CompletedTask; // To maintain async signature

        try
        {
            var semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration);
            
            if (classSymbol == null)
            {
                _logger.LogWarning("Failed to get class symbol for {ClassName}", classDeclaration.Identifier);
                return null;
            }

            _logger.LogDebug("Analyzing method relationships for class {ClassName}", classDeclaration.Identifier);

            var graph = new MethodRelationshipGraph
            {
                ClassName = classSymbol.Name,
                FullyQualifiedTypeName = classSymbol.ToDisplayString(),
                Namespace = classSymbol.ContainingNamespace?.ToDisplayString() ?? "",
                Location = CreateLocationInfo(classDeclaration)
            };

            // Extract all methods
            var methods = ExtractMethods(classDeclaration, semanticModel);
            graph.Methods.AddRange(methods);
            _logger.LogDebug("Found {MethodCount} methods in class {ClassName}", methods.Count, classDeclaration.Identifier);

            // Extract method call relationships
            var callEdges = ExtractMethodCalls(classDeclaration, semanticModel, methods);
            graph.CallEdges.AddRange(callEdges);
            _logger.LogDebug("Found {CallEdgeCount} method call relationships", callEdges.Count);

            // Extract field/property access relationships
            var fieldAccesses = ExtractFieldAccesses(classDeclaration, semanticModel, methods);
            graph.FieldAccesses.AddRange(fieldAccesses);
            _logger.LogDebug("Found {FieldAccessCount} field access relationships", fieldAccesses.Count);

            // Extract constructor chains
            var constructorChains = ExtractConstructorChains(classDeclaration, semanticModel, methods);
            graph.ConstructorChains.AddRange(constructorChains);
            _logger.LogDebug("Found {ConstructorChainCount} constructor chains", constructorChains.Count);

            // Add metadata
            graph.Metadata = new Dictionary<string, object>
            {
                ["totalMethods"] = methods.Count,
                ["totalCallEdges"] = callEdges.Count,
                ["totalFieldAccesses"] = fieldAccesses.Count,
                ["totalConstructorChains"] = constructorChains.Count,
                ["analysisTimestamp"] = DateTime.UtcNow,
                ["hasConstructors"] = methods.Any(m => m.IsConstructor),
                ["hasStaticMethods"] = methods.Any(m => m.IsStatic),
                ["hasVirtualMethods"] = methods.Any(m => m.IsVirtual)
            };

            return graph;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Method relationship analysis failed for class {ClassName}", classDeclaration.Identifier);
            return null;
        }
    }

    /// <summary>
    /// Analyze method relationships for all classes in a syntax tree
    /// </summary>
    /// <param name="compilation">Compilation context</param>
    /// <param name="syntaxTree">Syntax tree to analyze</param>
    /// <returns>List of method relationship graphs</returns>
    public async Task<List<MethodRelationshipGraph>> AnalyzeAllClassesAsync(
        Compilation compilation,
        SyntaxTree syntaxTree)
    {
        var graphs = new List<MethodRelationshipGraph>();

        try
        {
            _logger.LogDebug("Starting method relationship analysis for syntax tree");
            var root = await syntaxTree.GetRootAsync();

            // Find all class declarations
            var classes = root.DescendantNodes()
                .OfType<ClassDeclarationSyntax>();

            var classCount = classes.Count();
            _logger.LogDebug("Found {ClassCount} classes to analyze", classCount);

            // Analyze each class
            foreach (var classDeclaration in classes)
            {
                var graph = await AnalyzeClassAsync(compilation, classDeclaration);
                if (graph != null)
                {
                    graphs.Add(graph);
                }
            }

            _logger.LogDebug("Completed method relationship analysis for {GraphCount}/{ClassCount} classes", graphs.Count, classCount);
            return graphs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Method relationship analysis failed for syntax tree");
            return graphs; // Return partial results
        }
    }

    #region Private Helper Methods

    /// <summary>
    /// Extract all methods from a class declaration
    /// </summary>
    private List<MethodNode> ExtractMethods(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel)
    {
        var methods = new List<MethodNode>();

        // Extract regular methods
        foreach (var methodDeclaration in classDeclaration.Members.OfType<MethodDeclarationSyntax>())
        {
            var methodNode = CreateMethodNode(methodDeclaration, semanticModel);
            if (methodNode != null)
            {
                methods.Add(methodNode);
            }
        }

        // Extract constructors
        foreach (var constructorDeclaration in classDeclaration.Members.OfType<ConstructorDeclarationSyntax>())
        {
            var constructorNode = CreateConstructorNode(constructorDeclaration, semanticModel);
            if (constructorNode != null)
            {
                methods.Add(constructorNode);
            }
        }

        return methods;
    }

    /// <summary>
    /// Create a method node from method declaration
    /// </summary>
    private MethodNode? CreateMethodNode(MethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel)
    {
        try
        {
            var methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration);
            if (methodSymbol == null) return null;

            return new MethodNode
            {
                Name = methodSymbol.Name,
                Signature = methodSymbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat),
                ReturnType = methodSymbol.ReturnType.ToDisplayString(),
                Visibility = GetMethodVisibility(methodSymbol),
                IsStatic = methodSymbol.IsStatic,
                IsAbstract = methodSymbol.IsAbstract,
                IsVirtual = methodSymbol.IsVirtual,
                IsOverride = methodSymbol.IsOverride,
                IsAsync = methodSymbol.IsAsync,
                IsConstructor = false,
                Parameters = ExtractParameters(methodSymbol),
                Location = CreateLocationInfo(methodDeclaration),
                Metrics = CalculateMethodMetrics(methodDeclaration, semanticModel)
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create method node for {MethodName}", methodDeclaration.Identifier);
            return null;
        }
    }

    /// <summary>
    /// Create a constructor node from constructor declaration
    /// </summary>
    private MethodNode? CreateConstructorNode(ConstructorDeclarationSyntax constructorDeclaration, SemanticModel semanticModel)
    {
        try
        {
            var constructorSymbol = semanticModel.GetDeclaredSymbol(constructorDeclaration);
            if (constructorSymbol == null) return null;

            return new MethodNode
            {
                Name = ".ctor",
                Signature = constructorSymbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat),
                ReturnType = "void",
                Visibility = GetMethodVisibility(constructorSymbol),
                IsStatic = constructorSymbol.IsStatic,
                IsAbstract = false,
                IsVirtual = false,
                IsOverride = false,
                IsAsync = false,
                IsConstructor = true,
                Parameters = ExtractParameters(constructorSymbol),
                Location = CreateLocationInfo(constructorDeclaration),
                Metrics = CalculateConstructorMetrics(constructorDeclaration, semanticModel)
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create constructor node");
            return null;
        }
    }

    /// <summary>
    /// Extract method call relationships
    /// </summary>
    private List<MethodCallEdge> ExtractMethodCalls(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel, List<MethodNode> methods)
    {
        var callEdges = new List<MethodCallEdge>();
        var methodLookup = methods.ToDictionary(m => m.Signature, m => m.Id);

        foreach (var methodDeclaration in classDeclaration.Members.OfType<MethodDeclarationSyntax>())
        {
            var sourceMethodId = GetMethodId(methodDeclaration, semanticModel, methodLookup);
            if (sourceMethodId == null) continue;

            var invocations = methodDeclaration.DescendantNodes().OfType<InvocationExpressionSyntax>();
            
            foreach (var invocation in invocations)
            {
                var targetMethodId = GetInvocationTargetMethodId(invocation, semanticModel, methodLookup);
                if (targetMethodId != null)
                {
                    var callEdge = new MethodCallEdge
                    {
                        SourceMethodId = sourceMethodId,
                        TargetMethodId = targetMethodId,
                        CallType = DetermineCallType(invocation, semanticModel),
                        CallLocation = CreateLocationInfo(invocation),
                        Arguments = ExtractArgumentStrings(invocation)
                    };
                    
                    callEdges.Add(callEdge);
                }
            }
        }

        return callEdges;
    }

    /// <summary>
    /// Extract field/property access relationships
    /// </summary>
    private List<FieldAccessEdge> ExtractFieldAccesses(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel, List<MethodNode> methods)
    {
        var fieldAccesses = new List<FieldAccessEdge>();
        var methodLookup = methods.ToDictionary(m => m.Signature, m => m.Id);

        foreach (var methodDeclaration in classDeclaration.Members.OfType<MethodDeclarationSyntax>())
        {
            var methodId = GetMethodId(methodDeclaration, semanticModel, methodLookup);
            if (methodId == null) continue;

            // Find field and property accesses
            var memberAccesses = methodDeclaration.DescendantNodes().OfType<MemberAccessExpressionSyntax>();
            var identifierNames = methodDeclaration.DescendantNodes().OfType<IdentifierNameSyntax>();

            foreach (var memberAccess in memberAccesses)
            {
                var fieldAccess = CreateFieldAccessEdge(memberAccess, semanticModel, methodId);
                if (fieldAccess != null) fieldAccesses.Add(fieldAccess);
            }

            foreach (var identifier in identifierNames)
            {
                var fieldAccess = CreateFieldAccessFromIdentifier(identifier, semanticModel, methodId);
                if (fieldAccess != null) fieldAccesses.Add(fieldAccess);
            }
        }

        return fieldAccesses;
    }

    /// <summary>
    /// Extract constructor chain relationships
    /// </summary>
    private List<ConstructorChainEdge> ExtractConstructorChains(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel, List<MethodNode> methods)
    {
        var chains = new List<ConstructorChainEdge>();
        var constructorLookup = methods.Where(m => m.IsConstructor).ToDictionary(m => m.Signature, m => m.Id);

        foreach (var constructorDeclaration in classDeclaration.Members.OfType<ConstructorDeclarationSyntax>())
        {
            var sourceId = GetMethodId(constructorDeclaration, semanticModel, constructorLookup);
            if (sourceId == null) continue;

            var initializer = constructorDeclaration.Initializer;
            if (initializer != null)
            {
                var chainType = initializer.ThisOrBaseKeyword.IsKind(SyntaxKind.ThisKeyword) 
                    ? ConstructorChainType.This 
                    : ConstructorChainType.Base;

                var chain = new ConstructorChainEdge
                {
                    SourceConstructorId = sourceId,
                    ChainType = chainType,
                    Arguments = ExtractArgumentStrings(initializer.ArgumentList),
                    ChainLocation = CreateLocationInfo(initializer)
                };

                chains.Add(chain);
            }
        }

        return chains;
    }

    /// <summary>
    /// Get method visibility from symbol
    /// </summary>
    private MethodVisibility GetMethodVisibility(IMethodSymbol methodSymbol)
    {
        return methodSymbol.DeclaredAccessibility switch
        {
            Accessibility.Public => MethodVisibility.Public,
            Accessibility.Protected => MethodVisibility.Protected,
            Accessibility.Internal => MethodVisibility.Internal,
            Accessibility.ProtectedOrInternal => MethodVisibility.ProtectedInternal,
            Accessibility.ProtectedAndInternal => MethodVisibility.PrivateProtected,
            _ => MethodVisibility.Private
        };
    }

    /// <summary>
    /// Extract method parameters
    /// </summary>
    private List<MethodParameter> ExtractParameters(IMethodSymbol methodSymbol)
    {
        return methodSymbol.Parameters.Select(p => new MethodParameter
        {
            Name = p.Name,
            Type = p.Type.ToDisplayString(),
            HasDefaultValue = p.HasExplicitDefaultValue,
            DefaultValue = p.HasExplicitDefaultValue ? p.ExplicitDefaultValue?.ToString() : null,
            IsRef = p.RefKind == RefKind.Ref,
            IsOut = p.RefKind == RefKind.Out,
            IsParams = p.IsParams
        }).ToList();
    }

    /// <summary>
    /// Calculate basic method complexity metrics
    /// </summary>
    private MethodComplexityMetrics CalculateMethodMetrics(MethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel)
    {
        var body = methodDeclaration.Body;
        if (body == null) return new MethodComplexityMetrics();

        return new MethodComplexityMetrics
        {
            LineCount = body.GetLocation().GetLineSpan().EndLinePosition.Line - 
                       body.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
            ParameterCount = methodDeclaration.ParameterList.Parameters.Count,
            LocalVariableCount = body.DescendantNodes().OfType<VariableDeclaratorSyntax>().Count(),
            MethodCallCount = body.DescendantNodes().OfType<InvocationExpressionSyntax>().Count(),
            FieldAccessCount = body.DescendantNodes().OfType<MemberAccessExpressionSyntax>().Count(),
            HasExceptionHandling = body.DescendantNodes().OfType<TryStatementSyntax>().Any()
        };
    }

    /// <summary>
    /// Calculate constructor complexity metrics
    /// </summary>
    private MethodComplexityMetrics CalculateConstructorMetrics(ConstructorDeclarationSyntax constructorDeclaration, SemanticModel semanticModel)
    {
        var body = constructorDeclaration.Body;
        if (body == null) return new MethodComplexityMetrics();

        return new MethodComplexityMetrics
        {
            LineCount = body.GetLocation().GetLineSpan().EndLinePosition.Line - 
                       body.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
            ParameterCount = constructorDeclaration.ParameterList.Parameters.Count,
            LocalVariableCount = body.DescendantNodes().OfType<VariableDeclaratorSyntax>().Count(),
            MethodCallCount = body.DescendantNodes().OfType<InvocationExpressionSyntax>().Count(),
            FieldAccessCount = body.DescendantNodes().OfType<MemberAccessExpressionSyntax>().Count(),
            HasExceptionHandling = body.DescendantNodes().OfType<TryStatementSyntax>().Any()
        };
    }

    /// <summary>
    /// Create location info from syntax node
    /// </summary>
    private CSharpLocationInfo CreateLocationInfo(SyntaxNode node)
    {
        var location = node.GetLocation();
        var lineSpan = location.GetLineSpan();

        return new CSharpLocationInfo
        {
            StartLine = lineSpan.StartLinePosition.Line + 1,
            StartColumn = lineSpan.StartLinePosition.Character + 1,
            EndLine = lineSpan.EndLinePosition.Line + 1,
            EndColumn = lineSpan.EndLinePosition.Character + 1
        };
    }

    /// <summary>
    /// Get method ID from method lookup
    /// </summary>
    private string? GetMethodId(SyntaxNode declaration, SemanticModel semanticModel, Dictionary<string, string> methodLookup)
    {
        var symbol = semanticModel.GetDeclaredSymbol(declaration);
        if (symbol == null) return null;

        var signature = symbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
        return methodLookup.TryGetValue(signature, out var id) ? id : null;
    }

    /// <summary>
    /// Get target method ID from invocation
    /// </summary>
    private string? GetInvocationTargetMethodId(InvocationExpressionSyntax invocation, SemanticModel semanticModel, Dictionary<string, string> methodLookup)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(invocation);
        var targetSymbol = symbolInfo.Symbol;
        
        if (targetSymbol is IMethodSymbol methodSymbol)
        {
            var signature = methodSymbol.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
            return methodLookup.TryGetValue(signature, out var id) ? id : null;
        }

        return null;
    }

    /// <summary>
    /// Determine method call type
    /// </summary>
    private MethodCallType DetermineCallType(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(invocation);
        var targetSymbol = symbolInfo.Symbol;

        if (targetSymbol is IMethodSymbol methodSymbol)
        {
            if (methodSymbol.IsStatic) return MethodCallType.Static;
            if (methodSymbol.MethodKind == MethodKind.Constructor) return MethodCallType.Constructor;
            if (methodSymbol.IsVirtual) return MethodCallType.Virtual;
        }

        return MethodCallType.Direct;
    }

    /// <summary>
    /// Extract argument strings from invocation
    /// </summary>
    private List<string> ExtractArgumentStrings(InvocationExpressionSyntax invocation)
    {
        return invocation.ArgumentList.Arguments
            .Select(arg => arg.Expression.ToString())
            .ToList();
    }

    /// <summary>
    /// Extract argument strings from argument list
    /// </summary>
    private List<string> ExtractArgumentStrings(ArgumentListSyntax? argumentList)
    {
        return argumentList?.Arguments
            .Select(arg => arg.Expression.ToString())
            .ToList() ?? new List<string>();
    }

    /// <summary>
    /// Create field access edge from member access expression
    /// </summary>
    private FieldAccessEdge? CreateFieldAccessEdge(MemberAccessExpressionSyntax memberAccess, SemanticModel semanticModel, string methodId)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(memberAccess);
        var symbol = symbolInfo.Symbol;

        if (symbol is IFieldSymbol fieldSymbol)
        {
            return new FieldAccessEdge
            {
                MethodId = methodId,
                FieldName = fieldSymbol.Name,
                FieldType = fieldSymbol.Type.ToDisplayString(),
                FieldKind = fieldSymbol.IsConst ? FieldKind.Constant : 
                           fieldSymbol.IsReadOnly ? FieldKind.ReadOnly : FieldKind.Field,
                IsStatic = fieldSymbol.IsStatic,
                AccessLocation = CreateLocationInfo(memberAccess)
            };
        }
        else if (symbol is IPropertySymbol propertySymbol)
        {
            return new FieldAccessEdge
            {
                MethodId = methodId,
                FieldName = propertySymbol.Name,
                FieldType = propertySymbol.Type.ToDisplayString(),
                FieldKind = propertySymbol.IsIndexer ? FieldKind.Property : FieldKind.AutoProperty,
                IsStatic = propertySymbol.IsStatic,
                AccessLocation = CreateLocationInfo(memberAccess)
            };
        }

        return null;
    }

    /// <summary>
    /// Create field access edge from identifier
    /// </summary>
    private FieldAccessEdge? CreateFieldAccessFromIdentifier(IdentifierNameSyntax identifier, SemanticModel semanticModel, string methodId)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(identifier);
        var symbol = symbolInfo.Symbol;

        if (symbol is IFieldSymbol fieldSymbol && fieldSymbol.ContainingType != null)
        {
            return new FieldAccessEdge
            {
                MethodId = methodId,
                FieldName = fieldSymbol.Name,
                FieldType = fieldSymbol.Type.ToDisplayString(),
                FieldKind = fieldSymbol.IsConst ? FieldKind.Constant : 
                           fieldSymbol.IsReadOnly ? FieldKind.ReadOnly : FieldKind.Field,
                IsStatic = fieldSymbol.IsStatic,
                AccessLocation = CreateLocationInfo(identifier)
            };
        }
        else if (symbol is IPropertySymbol propertySymbol && propertySymbol.ContainingType != null)
        {
            return new FieldAccessEdge
            {
                MethodId = methodId,
                FieldName = propertySymbol.Name,
                FieldType = propertySymbol.Type.ToDisplayString(),
                FieldKind = propertySymbol.IsIndexer ? FieldKind.Property : FieldKind.AutoProperty,
                IsStatic = propertySymbol.IsStatic,
                AccessLocation = CreateLocationInfo(identifier)
            };
        }

        return null;
    }

    #endregion
}