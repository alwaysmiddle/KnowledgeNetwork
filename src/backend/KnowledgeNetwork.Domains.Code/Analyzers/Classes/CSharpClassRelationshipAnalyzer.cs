using KnowledgeNetwork.Domains.Code.Analyzers.Classes.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using KnowledgeNetwork.Domains.Code.Models.Classes;
using KnowledgeNetwork.Domains.Code.Models.Common;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Classes;

/// <summary>
/// Analyzes class-level relationships within C# code including inheritance, interfaces, composition, and dependencies
/// </summary>
public class CSharpClassRelationshipAnalyzer(ILogger<CSharpClassRelationshipAnalyzer> logger) : ICSharpClassRelationshipAnalyzer
{
    private readonly ILogger<CSharpClassRelationshipAnalyzer> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Resolves the effective file name using multiple fallback strategies
    /// </summary>
    private string ResolveEffectiveFileName(CompilationUnitSyntax compilationUnit, string providedFileName)
    {
        // 1. Use provided filename if not empty
        if (!string.IsNullOrWhiteSpace(providedFileName))
        {
            _logger.LogDebug("Using provided filename: {FileName}", providedFileName);
            return providedFileName;
        }

        // 2. Try to get filename from syntax tree
        var syntaxTreePath = compilationUnit.SyntaxTree?.FilePath;
        if (!string.IsNullOrWhiteSpace(syntaxTreePath))
        {
            _logger.LogDebug("Using syntax tree filename: {FileName}", syntaxTreePath);
            return syntaxTreePath;
        }

        // 3. Generate identifier based on content hash for in-memory code
        var contentHash = GenerateContentBasedIdentifier(compilationUnit);
        var syntheticName = $"<in-memory-{contentHash}>";
        _logger.LogDebug("Generated synthetic filename: {FileName}", syntheticName);
        return syntheticName;
    }

    /// <summary>
    /// Generates a content-based identifier for in-memory compilation units
    /// </summary>
    private string GenerateContentBasedIdentifier(CompilationUnitSyntax compilationUnit)
    {
        // Use a simple hash of the first type name + member count for identification
        var firstType = compilationUnit.DescendantNodes()
            .OfType<BaseTypeDeclarationSyntax>()
            .FirstOrDefault();

        if (firstType != null)
        {
            var typeName = firstType switch
            {
                ClassDeclarationSyntax cls => cls.Identifier.ValueText,
                InterfaceDeclarationSyntax intf => intf.Identifier.ValueText,
                StructDeclarationSyntax str => str.Identifier.ValueText,
                RecordDeclarationSyntax rec => rec.Identifier.ValueText,
                EnumDeclarationSyntax enm => enm.Identifier.ValueText,
                _ => "UnknownType"
            };

            var memberCount = compilationUnit.DescendantNodes().OfType<MemberDeclarationSyntax>().Count();
            var hashCode = (typeName + memberCount).GetHashCode();
            return $"{typeName}-{Math.Abs(hashCode):X6}";
        }

        // Fallback to simple hash of the full text
        var textHash = compilationUnit.GetText().ToString().GetHashCode();
        return $"code-{Math.Abs(textHash):X6}";
    }

    /// <summary>
    /// Analyzes class relationships within a compilation unit (file)
    /// </summary>
    public async Task<ClassRelationshipGraph?> AnalyzeFileAsync(
        Compilation compilation,
        CompilationUnitSyntax compilationUnit,
        string fileName = "")
    {
        try
        {
            var effectiveFileName = ResolveEffectiveFileName(compilationUnit, fileName);
            _logger.LogInformation("Starting class relationship analysis for file: {FileName}", effectiveFileName);

            var semanticModel = compilation.GetSemanticModel(compilationUnit.SyntaxTree);
            var graph = new ClassRelationshipGraph
            {
                ScopeName = effectiveFileName,
                ScopeType = ClassAnalysisScope.File,
                Location = GetLocationInfo(compilationUnit, effectiveFileName)
            };

            // Extract all class/type declarations
            var typeDeclarations = compilationUnit.DescendantNodes()
                .OfType<BaseTypeDeclarationSyntax>()
                .ToList();

            _logger.LogDebug("Found {Count} type declarations in file", typeDeclarations.Count);

            // Create class nodes
            foreach (var typeDeclaration in typeDeclarations)
            {
                var classNode = await CreateClassNodeAsync(semanticModel, typeDeclaration, effectiveFileName);
                if (classNode != null)
                {
                    graph.Classes.Add(classNode);
                }
            }

            // Analyze relationships between classes
            await AnalyzeInheritanceRelationships(semanticModel, graph, typeDeclarations);
            await AnalyzeInterfaceImplementations(semanticModel, graph, typeDeclarations);
            await AnalyzeCompositionRelationships(semanticModel, graph, typeDeclarations);
            await AnalyzeDependencyRelationships(semanticModel, graph, typeDeclarations);
            await AnalyzeNestedClassRelationships(semanticModel, graph, typeDeclarations);

            _logger.LogInformation("Completed class relationship analysis. Found {ClassCount} classes, {InheritanceCount} inheritance relationships, {InterfaceCount} interface implementations, {CompositionCount} composition relationships, {DependencyCount} dependency relationships",
                graph.Classes.Count,
                graph.InheritanceRelationships.Count,
                graph.InterfaceImplementations.Count,
                graph.CompositionRelationships.Count,
                graph.DependencyRelationships.Count);

            return graph;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing class relationships for file: {FileName}", fileName);
            return null;
        }
    }

    /// <summary>
    /// Creates a class node from a type declaration
    /// </summary>
    private async Task<ClassNode?> CreateClassNodeAsync(SemanticModel semanticModel, BaseTypeDeclarationSyntax typeDeclaration, string effectiveFileName)
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
                Location = GetLocationInfo(typeDeclaration, effectiveFileName)
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
        var metrics = new ClassComplexityMetrics();

        // Calculate line count
        var span = typeDeclaration.Span;
        var text = typeDeclaration.SyntaxTree.GetText();
        var startLine = text.Lines.GetLineFromPosition(span.Start).LineNumber;
        var endLine = text.Lines.GetLineFromPosition(span.End).LineNumber;
        metrics.TotalLineCount = endLine - startLine + 1;

        // Get members for analysis
        var methods = GetMethods(typeDeclaration);
        var allMembers = GetAllMembers(typeDeclaration);

        // Count public members
        metrics.PublicMemberCount = allMembers
            .Count(m => m.Modifiers.Any(SyntaxKind.PublicKeyword));

        // Calculate WMC (sum of method complexities) - simplified version
        foreach (var method in methods)
        {
            // Simple complexity based on control flow statements
            var complexity = CalculateMethodComplexity(method);
            metrics.WeightedMethodsPerClass += complexity;
        }

        // Calculate RFC (response for class) - methods + methods called
        metrics.ResponseForClass = methods.Count();
        foreach (var method in methods)
        {
            var invocations = method.DescendantNodes().OfType<InvocationExpressionSyntax>().Count();
            metrics.ResponseForClass += invocations;
        }

        classNode.Metrics = metrics;
    }

    /// <summary>
    /// Simple method complexity calculation
    /// </summary>
    private int CalculateMethodComplexity(MethodDeclarationSyntax method)
    {
        var complexity = 1; // Base complexity

        // Add complexity for control flow statements
        complexity += method.DescendantNodes().OfType<IfStatementSyntax>().Count();
        complexity += method.DescendantNodes().OfType<WhileStatementSyntax>().Count();
        complexity += method.DescendantNodes().OfType<ForStatementSyntax>().Count();
        complexity += method.DescendantNodes().OfType<ForEachStatementSyntax>().Count();
        complexity += method.DescendantNodes().OfType<SwitchStatementSyntax>().Count();
        complexity += method.DescendantNodes().OfType<CatchClauseSyntax>().Count();

        return complexity;
    }

    /// <summary>
    /// Analyzes inheritance relationships
    /// </summary>
    private async Task AnalyzeInheritanceRelationships(SemanticModel semanticModel, ClassRelationshipGraph graph, List<BaseTypeDeclarationSyntax> typeDeclarations)
    {
        foreach (var typeDeclaration in typeDeclarations)
        {
            if (typeDeclaration.BaseList?.Types.Count > 0)
            {
                var childSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration);
                if (childSymbol == null) continue;

                var childClass = graph.Classes.FirstOrDefault(c => c.FullName == childSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                if (childClass == null) continue;

                // Find base class (first non-interface type in base list)
                foreach (var baseType in typeDeclaration.BaseList.Types)
                {
                    var baseSymbol = semanticModel.GetSymbolInfo(baseType.Type).Symbol as INamedTypeSymbol;
                    if (baseSymbol?.TypeKind == TypeKind.Class)
                    {
                        var inheritanceEdge = new InheritanceEdge
                        {
                            ChildClassId = childClass.Id,
                            ParentClassId = baseSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                            ParentClassName = baseSymbol.ToDisplayString(),
                            InheritanceType = baseSymbol.IsAbstract ? InheritanceType.AbstractClass : InheritanceType.Class,
                            IsCrossNamespace = childSymbol.ContainingNamespace?.ToDisplayString() != baseSymbol.ContainingNamespace?.ToDisplayString(),
                            IsCrossAssembly = !SymbolEqualityComparer.Default.Equals(childSymbol.ContainingAssembly, baseSymbol.ContainingAssembly),
                            HierarchyLevel = 0,
                            InheritanceLocation = GetLocationInfo(baseType)
                        };

                        graph.InheritanceRelationships.Add(inheritanceEdge);
                        break; // Only one base class in C#
                    }
                }
            }
        }
    }

    /// <summary>
    /// Analyzes interface implementations
    /// </summary>
    private async Task AnalyzeInterfaceImplementations(SemanticModel semanticModel, ClassRelationshipGraph graph, List<BaseTypeDeclarationSyntax> typeDeclarations)
    {
        foreach (var typeDeclaration in typeDeclarations)
        {
            if (typeDeclaration.BaseList?.Types.Count > 0)
            {
                var classSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration);
                if (classSymbol == null) continue;

                var classNode = graph.Classes.FirstOrDefault(c => c.FullName == classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                if (classNode == null) continue;

                // Find interfaces in base list
                foreach (var baseType in typeDeclaration.BaseList.Types)
                {
                    var baseSymbol = semanticModel.GetSymbolInfo(baseType.Type).Symbol as INamedTypeSymbol;
                    if (baseSymbol?.TypeKind == TypeKind.Interface)
                    {
                        var implementationEdge = new InterfaceImplementationEdge
                        {
                            ClassId = classNode.Id,
                            InterfaceName = baseSymbol.Name,
                            InterfaceFullName = baseSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                            ImplementationType = InterfaceImplementationType.Direct,
                            IsGenericInterface = baseSymbol.IsGenericType,
                            IsCrossNamespace = classSymbol.ContainingNamespace?.ToDisplayString() != baseSymbol.ContainingNamespace?.ToDisplayString(),
                            IsCrossAssembly = !SymbolEqualityComparer.Default.Equals(classSymbol.ContainingAssembly, baseSymbol.ContainingAssembly),
                            ImplementationLocation = GetLocationInfo(baseType)
                        };

                        if (baseSymbol.IsGenericType)
                        {
                            implementationEdge.GenericTypeArguments = baseSymbol.TypeArguments
                                .Select(ta => ta.ToDisplayString())
                                .ToList();
                        }

                        // Extract implemented methods
                        ExtractInterfaceMethodImplementations(implementationEdge, baseSymbol, typeDeclaration, semanticModel);

                        graph.InterfaceImplementations.Add(implementationEdge);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Extracts interface method implementations
    /// </summary>
    private void ExtractInterfaceMethodImplementations(InterfaceImplementationEdge edge, INamedTypeSymbol interfaceSymbol, BaseTypeDeclarationSyntax classDeclaration, SemanticModel semanticModel)
    {
        var interfaceMethods = interfaceSymbol.GetMembers().OfType<IMethodSymbol>().ToList();
        var classMethods = GetMethods(classDeclaration).ToList();

        foreach (var interfaceMethod in interfaceMethods)
        {
            var implementation = new InterfaceMethodImplementation
            {
                MethodName = interfaceMethod.Name,
                MethodSignature = interfaceMethod.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)
            };

            // Find implementing method in class
            var implementingMethod = classMethods.FirstOrDefault(m => 
                m.Identifier.ValueText == interfaceMethod.Name);

            if (implementingMethod != null)
            {
                implementation.ImplementingMethodName = implementingMethod.Identifier.ValueText;
                implementation.ImplementationLocation = GetLocationInfo(implementingMethod);
                implementation.IsExplicit = implementingMethod.ExplicitInterfaceSpecifier != null;
            }

            edge.ImplementedMethods.Add(implementation);
        }
    }

    /// <summary>
    /// Analyzes composition relationships (has-a relationships through fields/properties)
    /// </summary>
    private async Task AnalyzeCompositionRelationships(SemanticModel semanticModel, ClassRelationshipGraph graph, List<BaseTypeDeclarationSyntax> typeDeclarations)
    {
        foreach (var typeDeclaration in typeDeclarations)
        {
            var classSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration);
            if (classSymbol == null) continue;

            var classNode = graph.Classes.FirstOrDefault(c => c.FullName == classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
            if (classNode == null) continue;

            var allMembers = GetAllMembers(typeDeclaration);

            // Analyze fields
            var fields = allMembers.OfType<FieldDeclarationSyntax>();
            foreach (var field in fields)
            {
                AnalyzeCompositionFromMember(field.Declaration.Type, field.Declaration.Variables.FirstOrDefault()?.Identifier.ValueText ?? "", semanticModel, graph, classNode, field, CompositionAccessType.Field);
            }

            // Analyze properties
            var properties = allMembers.OfType<PropertyDeclarationSyntax>();
            foreach (var property in properties)
            {
                AnalyzeCompositionFromMember(property.Type, property.Identifier.ValueText, semanticModel, graph, classNode, property, CompositionAccessType.Property);
            }
        }
    }

    /// <summary>
    /// Analyzes composition from a field or property
    /// </summary>
    private void AnalyzeCompositionFromMember(TypeSyntax typeSyntax, string memberName, SemanticModel semanticModel, ClassRelationshipGraph graph, ClassNode containerClass, SyntaxNode memberNode, CompositionAccessType accessType)
    {
        var typeSymbol = semanticModel.GetSymbolInfo(typeSyntax).Symbol as ITypeSymbol;
        if (typeSymbol == null) return;

        // Skip primitive types and system types
        if (typeSymbol.SpecialType != SpecialType.None) return;
        if (typeSymbol.ContainingNamespace?.ToDisplayString().StartsWith("System") == true) return;

        var compositionEdge = new CompositionEdge
        {
            ContainerClassId = containerClass.Id,
            ContainedClassId = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            ContainedClassName = typeSymbol.ToDisplayString(),
            MemberName = memberName,
            AccessType = accessType,
            CompositionType = DetermineCompositionType(typeSymbol),
            Multiplicity = DetermineMultiplicity(typeSymbol),
            IsNullable = typeSymbol.CanBeReferencedByName && typeSymbol.NullableAnnotation == NullableAnnotation.Annotated,
            IsGenericType = typeSymbol is INamedTypeSymbol namedType && namedType.IsGenericType,
            CompositionLocation = GetLocationInfo(memberNode)
        };

        if (compositionEdge.IsGenericType && typeSymbol is INamedTypeSymbol genericType)
        {
            compositionEdge.GenericTypeArguments = genericType.TypeArguments
                .Select(ta => ta.ToDisplayString())
                .ToList();
        }

        graph.CompositionRelationships.Add(compositionEdge);
    }

    /// <summary>
    /// Determines the type of composition relationship
    /// </summary>
    private CompositionType DetermineCompositionType(ITypeSymbol typeSymbol)
    {
        // This is a simplified heuristic - in practice, this would be more sophisticated
        var typeName = typeSymbol.ToDisplayString();
        
        if (typeName.Contains("List") || typeName.Contains("Collection") || typeName.Contains("Array"))
        {
            return CompositionType.Aggregation;
        }
        
        return CompositionType.Composition;
    }

    /// <summary>
    /// Determines the multiplicity of the relationship
    /// </summary>
    private CompositionMultiplicity DetermineMultiplicity(ITypeSymbol typeSymbol)
    {
        var typeName = typeSymbol.ToDisplayString();
        
        if (typeName.Contains("List") || typeName.Contains("Collection") || typeName.Contains("Array") || typeName.Contains("IEnumerable"))
        {
            return CompositionMultiplicity.Many;
        }
        
        if (typeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
        {
            return CompositionMultiplicity.ZeroOrOne;
        }
        
        return CompositionMultiplicity.One;
    }

    /// <summary>
    /// Analyzes dependency relationships (usage without ownership)
    /// </summary>
    private async Task AnalyzeDependencyRelationships(SemanticModel semanticModel, ClassRelationshipGraph graph, List<BaseTypeDeclarationSyntax> typeDeclarations)
    {
        foreach (var typeDeclaration in typeDeclarations)
        {
            var classSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration);
            if (classSymbol == null) continue;

            var classNode = graph.Classes.FirstOrDefault(c => c.FullName == classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
            if (classNode == null) continue;

            // Analyze method parameters and return types
            var methods = GetMethods(typeDeclaration);
            foreach (var method in methods)
            {
                AnalyzeDependenciesInMethod(method, semanticModel, graph, classNode);
            }

            // Analyze local variables and method calls
            var allNodes = typeDeclaration.DescendantNodes();
            AnalyzeDependenciesInNodes(allNodes, semanticModel, graph, classNode);
        }
    }

    /// <summary>
    /// Analyzes dependencies in a method
    /// </summary>
    private void AnalyzeDependenciesInMethod(MethodDeclarationSyntax method, SemanticModel semanticModel, ClassRelationshipGraph graph, ClassNode classNode)
    {
        // Return type dependency
        if (method.ReturnType != null)
        {
            AnalyzeDependencyFromType(method.ReturnType, semanticModel, graph, classNode, ClassDependencyType.ReturnType, method);
        }

        // Parameter dependencies
        foreach (var parameter in method.ParameterList.Parameters)
        {
            if (parameter.Type != null)
            {
                AnalyzeDependencyFromType(parameter.Type, semanticModel, graph, classNode, ClassDependencyType.Parameter, parameter);
            }
        }
    }

    /// <summary>
    /// Analyzes dependencies in syntax nodes
    /// </summary>
    private void AnalyzeDependenciesInNodes(IEnumerable<SyntaxNode> nodes, SemanticModel semanticModel, ClassRelationshipGraph graph, ClassNode classNode)
    {
        foreach (var node in nodes)
        {
            switch (node)
            {
                case VariableDeclarationSyntax variable:
                    AnalyzeDependencyFromType(variable.Type, semanticModel, graph, classNode, ClassDependencyType.LocalVariable, variable);
                    break;

                case InvocationExpressionSyntax invocation:
                    AnalyzeDependencyFromInvocation(invocation, semanticModel, graph, classNode);
                    break;

                case CastExpressionSyntax cast:
                    AnalyzeDependencyFromType(cast.Type, semanticModel, graph, classNode, ClassDependencyType.Usage, cast);
                    break;
            }
        }
    }

    /// <summary>
    /// Analyzes dependency from a type reference
    /// </summary>
    private void AnalyzeDependencyFromType(TypeSyntax typeSyntax, SemanticModel semanticModel, ClassRelationshipGraph graph, ClassNode classNode, ClassDependencyType dependencyType, SyntaxNode location)
    {
        var typeSymbol = semanticModel.GetSymbolInfo(typeSyntax).Symbol as ITypeSymbol;
        if (typeSymbol == null) return;

        // Skip primitive types, system types, and types already in composition relationships
        if (typeSymbol.SpecialType != SpecialType.None) return;
        if (typeSymbol.ContainingNamespace?.ToDisplayString().StartsWith("System") == true) return;

        var typeFullName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        
        // Check if this is already a composition relationship
        var hasComposition = graph.CompositionRelationships.Any(c => 
            c.ContainerClassId == classNode.Id && 
            c.ContainedClassId == typeFullName);
        
        if (hasComposition) return;

        // Find or create dependency edge
        var existingDependency = graph.DependencyRelationships.FirstOrDefault(d => 
            d.SourceClassId == classNode.Id && 
            d.TargetClassId == typeFullName);

        if (existingDependency != null)
        {
            existingDependency.ReferenceCount++;
            existingDependency.UsageLocations.Add(GetLocationInfo(location));
            
            if (!existingDependency.UsageTypes.Contains(GetDependencyUsage(dependencyType)))
            {
                existingDependency.UsageTypes.Add(GetDependencyUsage(dependencyType));
            }
        }
        else
        {
            var dependencyEdge = new ClassDependencyEdge
            {
                SourceClassId = classNode.Id,
                TargetClassId = typeFullName,
                TargetClassName = typeSymbol.ToDisplayString(),
                DependencyType = dependencyType,
                UsageTypes = new List<DependencyUsage> { GetDependencyUsage(dependencyType) },
                Strength = DetermineDependencyStrength(dependencyType),
                ReferenceCount = 1,
                IsCrossNamespace = classNode.Namespace != typeSymbol.ContainingNamespace?.ToDisplayString(),
                IsGenericTarget = typeSymbol is INamedTypeSymbol namedType && namedType.IsGenericType,
                UsageLocations = new List<CSharpLocationInfo> { GetLocationInfo(location) }
            };

            if (dependencyEdge.IsGenericTarget && typeSymbol is INamedTypeSymbol genericType)
            {
                dependencyEdge.GenericTypeArguments = genericType.TypeArguments
                    .Select(ta => ta.ToDisplayString())
                    .ToList();
            }

            graph.DependencyRelationships.Add(dependencyEdge);
        }
    }

    /// <summary>
    /// Analyzes dependency from method invocation
    /// </summary>
    private void AnalyzeDependencyFromInvocation(InvocationExpressionSyntax invocation, SemanticModel semanticModel, ClassRelationshipGraph graph, ClassNode classNode)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(invocation);
        if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
        {
            var containingType = methodSymbol.ContainingType;
            if (containingType != null && containingType.SpecialType == SpecialType.None)
            {
                var typeFullName = containingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                
                // Skip if this is the same class
                if (typeFullName == classNode.FullName) return;

                AnalyzeDependencyFromType(SyntaxFactory.IdentifierName(containingType.Name), semanticModel, graph, classNode, methodSymbol.IsStatic ? ClassDependencyType.StaticCall : ClassDependencyType.Usage, invocation);
            }
        }
    }

    /// <summary>
    /// Maps dependency type to usage type
    /// </summary>
    private DependencyUsage GetDependencyUsage(ClassDependencyType dependencyType)
    {
        return dependencyType switch
        {
            ClassDependencyType.Parameter => DependencyUsage.MethodParameter,
            ClassDependencyType.ReturnType => DependencyUsage.ReturnType,
            ClassDependencyType.LocalVariable => DependencyUsage.LocalVariable,
            ClassDependencyType.StaticCall => DependencyUsage.StaticMethodCall,
            _ => DependencyUsage.LocalVariable
        };
    }

    /// <summary>
    /// Determines dependency strength
    /// </summary>
    private DependencyStrength DetermineDependencyStrength(ClassDependencyType dependencyType)
    {
        return dependencyType switch
        {
            ClassDependencyType.Parameter => DependencyStrength.Moderate,
            ClassDependencyType.ReturnType => DependencyStrength.Strong,
            ClassDependencyType.LocalVariable => DependencyStrength.Weak,
            ClassDependencyType.StaticCall => DependencyStrength.Weak,
            _ => DependencyStrength.Weak
        };
    }

    /// <summary>
    /// Analyzes nested class relationships
    /// </summary>
    private async Task AnalyzeNestedClassRelationships(SemanticModel semanticModel, ClassRelationshipGraph graph, List<BaseTypeDeclarationSyntax> typeDeclarations)
    {
        foreach (var typeDeclaration in typeDeclarations)
        {
            var classSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration);
            if (classSymbol == null) continue;

            var containerClass = graph.Classes.FirstOrDefault(c => c.FullName == classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
            if (containerClass == null) continue;

            // Find nested types
            var allMembers = GetAllMembers(typeDeclaration);
            var nestedTypes = allMembers.OfType<BaseTypeDeclarationSyntax>();
            foreach (var nestedType in nestedTypes)
            {
                var nestedSymbol = semanticModel.GetDeclaredSymbol(nestedType);
                if (nestedSymbol == null) continue;

                var nestedClass = graph.Classes.FirstOrDefault(c => c.FullName == nestedSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                if (nestedClass == null) continue;

                var nestedEdge = new NestedClassEdge
                {
                    ContainerClassId = containerClass.Id,
                    NestedClassId = nestedClass.Id,
                    NestedClassName = nestedSymbol.Name,
                    NestingLevel = CalculateNestingLevel(nestedSymbol),
                    NestedClassVisibility = nestedSymbol.DeclaredAccessibility.ToString(),
                    IsStaticNested = nestedType.Modifiers.Any(SyntaxKind.StaticKeyword),
                    NestedClassType = nestedType.GetType().Name,
                    NestingLocation = GetLocationInfo(nestedType)
                };

                graph.NestedClassRelationships.Add(nestedEdge);
            }
        }
    }

    /// <summary>
    /// Calculates the nesting level of a type
    /// </summary>
    private int CalculateNestingLevel(ISymbol symbol)
    {
        var level = 0;
        var containingType = symbol.ContainingType;
        
        while (containingType != null)
        {
            level++;
            containingType = containingType.ContainingType;
        }
        
        return level;
    }

    /// <summary>
    /// Gets location information from a syntax node
    /// </summary>
    private CSharpLocationInfo GetLocationInfo(SyntaxNode? node, string? fallbackFilePath = null)
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
    private IEnumerable<MethodDeclarationSyntax> GetMethods(BaseTypeDeclarationSyntax typeDeclaration)
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
    private IEnumerable<MemberDeclarationSyntax> GetAllMembers(BaseTypeDeclarationSyntax typeDeclaration)
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