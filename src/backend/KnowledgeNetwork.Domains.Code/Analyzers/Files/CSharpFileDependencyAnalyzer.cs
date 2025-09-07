using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using KnowledgeNetwork.Domains.Code.Models.Files;
using KnowledgeNetwork.Domains.Code.Models.Common;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Files;

/// <summary>
/// Analyzes file-level dependencies within C# projects including using statements, namespace dependencies, and assembly references
/// </summary>
public class CSharpFileDependencyAnalyzer(ILogger<CSharpFileDependencyAnalyzer> logger)
{
    private readonly ILogger<CSharpFileDependencyAnalyzer> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Analyzes file dependencies within a compilation (project-level analysis)
    /// </summary>
    public async Task<FileDependencyGraph?> AnalyzeProjectAsync(Compilation compilation, string projectName = "",
        string projectPath = "")
    {
        try
        {
            _logger.LogInformation("Starting file dependency analysis for project: {ProjectName}", projectName);

            var graph = new FileDependencyGraph
            {
                ScopeName = projectName,
                ScopeType = FileDependencyScope.Project
            };

            // Get all syntax trees (files) in the compilation
            var syntaxTrees = compilation.SyntaxTrees.ToList();
            _logger.LogDebug("Found {Count} files in project", syntaxTrees.Count);

            // Create file nodes
            foreach (var syntaxTree in syntaxTrees)
            {
                var fileNode = await CreateFileNodeAsync(compilation, syntaxTree, projectName);
                if (fileNode != null)
                {
                    graph.Files.Add(fileNode);
                }
            }

            // Analyze dependencies between files
            await AnalyzeUsingDependencies(compilation, graph);
            await AnalyzeNamespaceDependencies(compilation, graph);
            await AnalyzeTypeReferenceDependencies(compilation, graph);
            await AnalyzeAssemblyDependencies(compilation, graph);

            _logger.LogInformation("Completed file dependency analysis. Found {FileCount} files, {UsingCount} using dependencies, {NamespaceCount} namespace dependencies, {TypeRefCount} type reference dependencies, {AssemblyCount} assembly dependencies",
                graph.Files.Count,
                graph.UsingDependencies.Count,
                graph.NamespaceDependencies.Count,
                graph.TypeReferenceDependencies.Count,
                graph.AssemblyDependencies.Count);

            return graph;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing file dependencies for project: {ProjectName}", projectName);
            return null;
        }
    }

    /// <summary>
    /// Creates a file node from a syntax tree
    /// </summary>
    private async Task<FileNode?> CreateFileNodeAsync(Compilation compilation, SyntaxTree syntaxTree, string projectName)
    {
        try
        {
            var filePath = syntaxTree.FilePath;
            if (string.IsNullOrEmpty(filePath)) return null;

            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var root = await syntaxTree.GetRootAsync();

            var fileNode = new FileNode
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                RelativePath = GetRelativePath(filePath),
                DirectoryPath = Path.GetDirectoryName(filePath) ?? string.Empty,
                FileExtension = Path.GetExtension(filePath),
                Language = FileLanguage.CSharp,
                ProjectName = projectName,
                Location = GetLocationInfo(root)
            };

            // Determine file type
            fileNode.FileType = DetermineFileType(fileNode.FileName, filePath);

            // Get file size and last modified time
            if (File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                fileNode.FileSize = fileInfo.Length;
                fileNode.LastModified = fileInfo.LastWriteTime;
            }

            // Extract using directives
            ExtractUsingDirectives(fileNode, root);

            // Extract declared namespaces
            ExtractDeclaredNamespaces(fileNode, root);

            // Extract declared types
            await ExtractDeclaredTypesAsync(fileNode, root, semanticModel);

            // Extract referenced types
            await ExtractReferencedTypesAsync(fileNode, root, semanticModel);

            // Calculate metrics
            CalculateFileMetrics(fileNode, root);

            return fileNode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create file node for syntax tree: {FilePath}", syntaxTree.FilePath);
            return null;
        }
    }

    /// <summary>
    /// Gets relative path for a file
    /// </summary>
    private string GetRelativePath(string filePath)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var uri = new Uri(currentDirectory + Path.DirectorySeparatorChar);
        var fileUri = new Uri(filePath);
        
        if (uri.IsBaseOf(fileUri))
        {
            return Uri.UnescapeDataString(uri.MakeRelativeUri(fileUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }
        
        return filePath;
    }

    /// <summary>
    /// Determines the type of file based on name and path
    /// </summary>
    private FileType DetermineFileType(string fileName, string filePath)
    {
        var lowerFileName = fileName.ToLowerInvariant();
        var lowerPath = filePath.ToLowerInvariant();

        if (lowerFileName.Contains("test") || lowerPath.Contains("test"))
            return FileType.Test;
        
        if (lowerFileName.EndsWith(".designer.cs"))
            return FileType.Designer;
        
        if (lowerFileName.EndsWith(".g.cs") || lowerFileName.EndsWith(".generated.cs"))
            return FileType.Generated;
        
        if (lowerFileName.Contains("config") || lowerFileName.EndsWith(".config"))
            return FileType.Configuration;
        
        if (lowerFileName.Contains("resource") || lowerFileName.EndsWith(".resx"))
            return FileType.Resource;

        return FileType.Source;
    }

    /// <summary>
    /// Extracts using directives from the syntax tree
    /// </summary>
    private void ExtractUsingDirectives(FileNode fileNode, SyntaxNode root)
    {
        var usingDirectives = root.DescendantNodes().OfType<UsingDirectiveSyntax>().ToList();
        
        foreach (var usingDirective in usingDirectives)
        {
            var nameText = usingDirective.Name?.ToString() ?? string.Empty;
            if (!string.IsNullOrEmpty(nameText))
            {
                if (usingDirective.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword))
                {
                    fileNode.GlobalUsings.Add(nameText);
                }
                else
                {
                    fileNode.UsingDirectives.Add(nameText);
                }
            }
        }
    }

    /// <summary>
    /// Extracts declared namespaces from the syntax tree
    /// </summary>
    private void ExtractDeclaredNamespaces(FileNode fileNode, SyntaxNode root)
    {
        // File-scoped namespaces (C# 10+)
        var fileScopedNamespaces = root.DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>();
        foreach (var ns in fileScopedNamespaces)
        {
            var namespaceName = ns.Name.ToString();
            if (!fileNode.DeclaredNamespaces.Contains(namespaceName))
            {
                fileNode.DeclaredNamespaces.Add(namespaceName);
            }
        }

        // Traditional block-scoped namespaces
        var blockScopedNamespaces = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>();
        foreach (var ns in blockScopedNamespaces)
        {
            var namespaceName = ns.Name.ToString();
            if (!fileNode.DeclaredNamespaces.Contains(namespaceName))
            {
                fileNode.DeclaredNamespaces.Add(namespaceName);
            }
        }
    }

    /// <summary>
    /// Extracts declared types from the syntax tree
    /// </summary>
    private async Task ExtractDeclaredTypesAsync(FileNode fileNode, SyntaxNode root, SemanticModel semanticModel)
    {
        var typeDeclarations = root.DescendantNodes().OfType<BaseTypeDeclarationSyntax>();
        
        foreach (var typeDeclaration in typeDeclarations)
        {
            var typeSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration);
            if (typeSymbol != null)
            {
                var declaredType = new DeclaredType
                {
                    Name = typeSymbol.Name,
                    FullName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    Namespace = typeSymbol.ContainingNamespace?.ToDisplayString() ?? string.Empty,
                    TypeKind = typeSymbol.TypeKind.ToString(),
                    Visibility = typeSymbol.DeclaredAccessibility.ToString(),
                    IsStatic = typeSymbol.IsStatic,
                    IsGeneric = typeSymbol is INamedTypeSymbol namedType && namedType.IsGenericType,
                    Location = GetLocationInfo(typeDeclaration)
                };

                fileNode.DeclaredTypes.Add(declaredType);
            }
        }
    }

    /// <summary>
    /// Extracts referenced types from the syntax tree
    /// </summary>
    private async Task ExtractReferencedTypesAsync(FileNode fileNode, SyntaxNode root, SemanticModel semanticModel)
    {
        var referencedTypes = new Dictionary<string, ReferencedType>();

        // Find all identifier names (type references)
        var identifierNodes = root.DescendantNodes().OfType<IdentifierNameSyntax>();
        
        foreach (var identifier in identifierNodes)
        {
            var symbolInfo = semanticModel.GetSymbolInfo(identifier);
            if (symbolInfo.Symbol is ITypeSymbol typeSymbol)
            {
                var fullName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                
                // Skip primitive types and declared types in the same file
                if (typeSymbol.SpecialType != SpecialType.None || 
                    fileNode.DeclaredTypes.Any(dt => dt.FullName == fullName))
                    continue;

                if (!referencedTypes.ContainsKey(fullName))
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
                        ReferenceLocations = new List<CSharpLocationInfo> { GetLocationInfo(identifier) }
                    };

                    referencedTypes.Add(fullName, referencedType);
                }
                else
                {
                    referencedTypes[fullName].ReferenceCount++;
                    referencedTypes[fullName].ReferenceLocations.Add(GetLocationInfo(identifier));
                }
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

    /// <summary>
    /// Calculates metrics for the file
    /// </summary>
    private void CalculateFileMetrics(FileNode fileNode, SyntaxNode root)
    {
        var text = root.SyntaxTree.GetText();
        var lines = text.Lines;

        var metrics = new FileMetrics
        {
            TotalLines = lines.Count,
            UsingDirectiveCount = fileNode.UsingDirectives.Count + fileNode.GlobalUsings.Count,
            DeclaredTypeCount = fileNode.DeclaredTypes.Count,
            ReferencedTypeCount = fileNode.ReferencedTypes.Count
        };

        // Calculate lines of code, comments, and blank lines
        foreach (var line in lines)
        {
            var lineText = line.ToString().Trim();
            
            if (string.IsNullOrEmpty(lineText))
            {
                metrics.BlankLines++;
            }
            else if (lineText.StartsWith("//") || lineText.StartsWith("/*") || lineText.StartsWith("*"))
            {
                metrics.CommentLines++;
            }
            else
            {
                metrics.LinesOfCode++;
            }
        }

        // Simple complexity calculation based on control flow statements
        var complexityNodes = root.DescendantNodes().Where(n => 
            n.IsKind(SyntaxKind.IfStatement) ||
            n.IsKind(SyntaxKind.WhileStatement) ||
            n.IsKind(SyntaxKind.ForStatement) ||
            n.IsKind(SyntaxKind.ForEachStatement) ||
            n.IsKind(SyntaxKind.SwitchStatement) ||
            n.IsKind(SyntaxKind.TryStatement));

        metrics.CyclomaticComplexity = complexityNodes.Count() + 1; // +1 for base complexity

        // Calculate maintainability index (simplified)
        if (metrics.LinesOfCode > 0)
        {
            var commentRatio = (double)metrics.CommentLines / metrics.TotalLines;
            var complexityRatio = (double)metrics.CyclomaticComplexity / metrics.LinesOfCode;
            
            metrics.MaintainabilityIndex = Math.Max(0, 
                100 - (metrics.LinesOfCode / 10.0) - (metrics.CyclomaticComplexity * 2) + (commentRatio * 20));
            
            metrics.TechnicalDebtRatio = Math.Min(1.0, complexityRatio * 0.5 + (1.0 - commentRatio) * 0.3);
        }

        fileNode.Metrics = metrics;
    }

    /// <summary>
    /// Analyzes using directive dependencies between files
    /// </summary>
    private async Task AnalyzeUsingDependencies(Compilation compilation, FileDependencyGraph graph)
    {
        foreach (var sourceFile in graph.Files)
        {
            var syntaxTree = compilation.SyntaxTrees.FirstOrDefault(st => st.FilePath == sourceFile.FilePath);
            if (syntaxTree == null) continue;

            var root = await syntaxTree.GetRootAsync();
            var usingDirectives = root.DescendantNodes().OfType<UsingDirectiveSyntax>().ToList();

            foreach (var usingDirective in usingDirectives)
            {
                var namespaceName = usingDirective.Name?.ToString();
                if (string.IsNullOrEmpty(namespaceName)) continue;

                // Find target file that declares this namespace
                var targetFile = graph.Files.FirstOrDefault(f => 
                    f.DeclaredNamespaces.Contains(namespaceName) && f.Id != sourceFile.Id);

                var usingDependency = new UsingDependencyEdge
                {
                    SourceFileId = sourceFile.Id,
                    TargetFileId = targetFile?.Id ?? string.Empty,
                    NamespaceName = namespaceName,
                    UsingDirective = usingDirective.ToString(),
                    DirectiveType = DetermineUsingDirectiveType(usingDirective),
                    IsGlobal = usingDirective.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword),
                    IsStatic = usingDirective.StaticKeyword.IsKind(SyntaxKind.StaticKeyword),
                    IsAlias = usingDirective.Alias != null,
                    AliasName = usingDirective.Alias?.Name.ToString(),
                    IsExternalAssembly = targetFile == null,
                    UsingLocation = GetLocationInfo(usingDirective)
                };

                // Check if the using is actually utilized
                AnalyzeUsingUtilization(usingDependency, sourceFile, compilation);

                graph.UsingDependencies.Add(usingDependency);
            }
        }
    }

    /// <summary>
    /// Determines the type of using directive
    /// </summary>
    private UsingDirectiveType DetermineUsingDirectiveType(UsingDirectiveSyntax usingDirective)
    {
        var isGlobal = usingDirective.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword);
        var isStatic = usingDirective.StaticKeyword.IsKind(SyntaxKind.StaticKeyword);
        var isAlias = usingDirective.Alias != null;

        return (isGlobal, isStatic, isAlias) switch
        {
            (true, true, false) => UsingDirectiveType.GlobalStatic,
            (true, false, true) => UsingDirectiveType.GlobalAlias,
            (true, false, false) => UsingDirectiveType.Global,
            (false, true, false) => UsingDirectiveType.Static,
            (false, false, true) => UsingDirectiveType.Alias,
            _ => UsingDirectiveType.Namespace
        };
    }

    /// <summary>
    /// Analyzes whether a using directive is actually utilized
    /// </summary>
    private void AnalyzeUsingUtilization(UsingDependencyEdge usingDependency, FileNode sourceFile, Compilation compilation)
    {
        var namespaceName = usingDependency.NamespaceName;
        
        // Check if any referenced types belong to this namespace
        var utilizedTypes = sourceFile.ReferencedTypes
            .Where(rt => rt.Namespace == namespaceName)
            .ToList();

        usingDependency.IsUtilized = utilizedTypes.Count > 0;
        usingDependency.UtilizationCount = utilizedTypes.Sum(ut => ut.ReferenceCount);
        usingDependency.UtilizedTypes = utilizedTypes.Select(ut => ut.Name).ToList();
    }

    /// <summary>
    /// Analyzes namespace dependencies between files
    /// </summary>
    private async Task AnalyzeNamespaceDependencies(Compilation compilation, FileDependencyGraph graph)
    {
        foreach (var sourceFile in graph.Files)
        {
            foreach (var referencedType in sourceFile.ReferencedTypes)
            {
                if (string.IsNullOrEmpty(referencedType.Namespace) || referencedType.IsExternal)
                    continue;

                // Find target file that declares types in this namespace
                var targetFile = graph.Files.FirstOrDefault(f =>
                    f.DeclaredNamespaces.Contains(referencedType.Namespace) && f.Id != sourceFile.Id);

                if (targetFile == null) continue;

                var sourceNamespace = sourceFile.GetPrimaryNamespace();
                
                var namespaceDependency = new NamespaceDependencyEdge
                {
                    SourceFileId = sourceFile.Id,
                    TargetFileId = targetFile.Id,
                    SourceNamespace = sourceNamespace,
                    NamespaceName = referencedType.Namespace,
                    DependencyType = MapToNamespaceDependencyType(referencedType.ReferenceKind),
                    Strength = DetermineNamespaceDependencyStrength(referencedType.ReferenceCount),
                    TypeUsageCount = referencedType.ReferenceCount,
                    NamespaceDistance = CalculateNamespaceDistance(sourceNamespace, referencedType.Namespace),
                    DependencyLocation = referencedType.ReferenceLocations.FirstOrDefault()
                };

                // Add type usage information
                namespaceDependency.TypeUsages.Add(new NamespaceTypeUsage
                {
                    TypeName = referencedType.Name,
                    FullTypeName = referencedType.FullName,
                    UsageKinds = MapToTypeUsageKinds(referencedType.ReferenceKind),
                    UsageCount = referencedType.ReferenceCount,
                    UsageLocations = referencedType.ReferenceLocations
                });

                graph.NamespaceDependencies.Add(namespaceDependency);
            }
        }
    }

    /// <summary>
    /// Maps type reference kind to namespace dependency type
    /// </summary>
    private NamespaceDependencyType MapToNamespaceDependencyType(TypeReferenceKind referenceKind)
    {
        return referenceKind switch
        {
            TypeReferenceKind.Inheritance => NamespaceDependencyType.Inheritance,
            TypeReferenceKind.Interface => NamespaceDependencyType.InterfaceImplementation,
            TypeReferenceKind.GenericParameter => NamespaceDependencyType.GenericConstraint,
            TypeReferenceKind.Attribute => NamespaceDependencyType.AttributeUsage,
            TypeReferenceKind.Reflection => NamespaceDependencyType.TypeReference,
            _ => NamespaceDependencyType.TypeReference
        };
    }

    /// <summary>
    /// Determines namespace dependency strength
    /// </summary>
    private NamespaceDependencyStrength DetermineNamespaceDependencyStrength(int usageCount)
    {
        return usageCount switch
        {
            >= 10 => NamespaceDependencyStrength.Critical,
            >= 5 => NamespaceDependencyStrength.Strong,
            >= 2 => NamespaceDependencyStrength.Moderate,
            _ => NamespaceDependencyStrength.Weak
        };
    }

    /// <summary>
    /// Calculates the distance between two namespaces
    /// </summary>
    private int CalculateNamespaceDistance(string sourceNamespace, string targetNamespace)
    {
        if (string.IsNullOrEmpty(sourceNamespace) || string.IsNullOrEmpty(targetNamespace))
            return int.MaxValue;

        var sourceParts = sourceNamespace.Split('.');
        var targetParts = targetNamespace.Split('.');

        // Find common prefix length
        int commonLength = 0;
        int minLength = Math.Min(sourceParts.Length, targetParts.Length);
        
        for (int i = 0; i < minLength; i++)
        {
            if (sourceParts[i] == targetParts[i])
                commonLength++;
            else
                break;
        }

        // Distance is the sum of unique parts
        return (sourceParts.Length - commonLength) + (targetParts.Length - commonLength);
    }

    /// <summary>
    /// Maps type reference kind to type usage kinds
    /// </summary>
    private List<TypeUsageKind> MapToTypeUsageKinds(TypeReferenceKind referenceKind)
    {
        return referenceKind switch
        {
            TypeReferenceKind.Inheritance => [TypeUsageKind.Inheritance],
            TypeReferenceKind.Interface => [TypeUsageKind.InterfaceImplementation],
            TypeReferenceKind.GenericParameter => [TypeUsageKind.GenericArgument],
            TypeReferenceKind.Attribute => [TypeUsageKind.Attribute],
            _ => [TypeUsageKind.Declaration]
        };
    }

    /// <summary>
    /// Analyzes type reference dependencies between files
    /// </summary>
    private async Task AnalyzeTypeReferenceDependencies(Compilation compilation, FileDependencyGraph graph)
    {
        foreach (var sourceFile in graph.Files)
        {
            foreach (var referencedType in sourceFile.ReferencedTypes.Where(rt => !rt.IsExternal))
            {
                // Find target file that declares this type
                var targetFile = graph.Files.FirstOrDefault(f =>
                    f.DeclaredTypes.Any(dt => dt.FullName == referencedType.FullName) && f.Id != sourceFile.Id);

                if (targetFile == null) continue;

                var typeReferenceDependency = new TypeReferenceDependencyEdge
                {
                    SourceFileId = sourceFile.Id,
                    TargetFileId = targetFile.Id,
                    TypeName = referencedType.Name,
                    FullTypeName = referencedType.FullName,
                    TypeNamespace = referencedType.Namespace,
                    TypeKind = GetTypeKind(referencedType.FullName, targetFile),
                    ReferenceContext = MapToTypeReferenceContext(referencedType.ReferenceKind),
                    UsagePatterns = MapToTypeUsagePatterns(referencedType.ReferenceKind),
                    ReferenceCount = referencedType.ReferenceCount,
                    Strength = DetermineTypeReferenceDependencyStrength(referencedType.ReferenceCount),
                    ImpactLevel = DetermineImpactLevel(referencedType.ReferenceKind, referencedType.ReferenceCount),
                    ReferenceLocations = referencedType.ReferenceLocations
                };

                graph.TypeReferenceDependencies.Add(typeReferenceDependency);
            }
        }
    }

    /// <summary>
    /// Gets the kind of type from the target file
    /// </summary>
    private string GetTypeKind(string fullTypeName, FileNode targetFile)
    {
        var declaredType = targetFile.DeclaredTypes.FirstOrDefault(dt => dt.FullName == fullTypeName);
        return declaredType?.TypeKind ?? "Unknown";
    }

    /// <summary>
    /// Maps type reference kind to type reference context
    /// </summary>
    private TypeReferenceContext MapToTypeReferenceContext(TypeReferenceKind referenceKind)
    {
        return referenceKind switch
        {
            TypeReferenceKind.Inheritance => TypeReferenceContext.Inheritance,
            TypeReferenceKind.Interface => TypeReferenceContext.InterfaceImplementation,
            _ => TypeReferenceContext.Usage
        };
    }

    /// <summary>
    /// Maps type reference kind to usage patterns
    /// </summary>
    private List<TypeUsagePattern> MapToTypeUsagePatterns(TypeReferenceKind referenceKind)
    {
        return referenceKind switch
        {
            TypeReferenceKind.Inheritance => [TypeUsagePattern.FieldDeclaration],
            TypeReferenceKind.Attribute => [TypeUsagePattern.AttributeApplication],
            TypeReferenceKind.GenericParameter => [TypeUsagePattern.GenericConstraint],
            TypeReferenceKind.Reflection => [TypeUsagePattern.ReflectionUsage],
            _ => [TypeUsagePattern.LocalVariable]
        };
    }

    /// <summary>
    /// Determines type reference dependency strength
    /// </summary>
    private TypeReferenceDependencyStrength DetermineTypeReferenceDependencyStrength(int referenceCount)
    {
        return referenceCount switch
        {
            >= 20 => TypeReferenceDependencyStrength.Critical,
            >= 10 => TypeReferenceDependencyStrength.Strong,
            >= 3 => TypeReferenceDependencyStrength.Moderate,
            _ => TypeReferenceDependencyStrength.Weak
        };
    }

    /// <summary>
    /// Determines impact level of a dependency
    /// </summary>
    private DependencyImpactLevel DetermineImpactLevel(TypeReferenceKind referenceKind, int referenceCount)
    {
        if (referenceKind == TypeReferenceKind.Inheritance)
            return DependencyImpactLevel.Critical;
        
        if (referenceKind == TypeReferenceKind.Interface)
            return DependencyImpactLevel.High;

        return referenceCount switch
        {
            >= 10 => DependencyImpactLevel.High,
            >= 5 => DependencyImpactLevel.Medium,
            _ => DependencyImpactLevel.Low
        };
    }

    /// <summary>
    /// Analyzes external assembly dependencies
    /// </summary>
    private async Task AnalyzeAssemblyDependencies(Compilation compilation, FileDependencyGraph graph)
    {
        var assemblyDependencies = new Dictionary<string, AssemblyDependencyEdge>();

        foreach (var sourceFile in graph.Files)
        {
            foreach (var referencedType in sourceFile.ReferencedTypes.Where(rt => rt.IsExternal))
            {
                if (string.IsNullOrEmpty(referencedType.Assembly)) continue;

                var assemblyName = referencedType.Assembly;
                var assemblyKey = $"{sourceFile.Id}_{assemblyName}";

                if (!assemblyDependencies.ContainsKey(assemblyKey))
                {
                    var assemblyDependency = new AssemblyDependencyEdge
                    {
                        SourceFileId = sourceFile.Id,
                        AssemblyName = assemblyName,
                        AssemblyFullName = assemblyName, // Simplified - could get full name from metadata
                        DependencyType = AssemblyDependencyType.Reference,
                        Source = DetermineAssemblySource(assemblyName),
                        Importance = DetermineAssemblyImportance(assemblyName),
                        IsBaseClassLibrary = IsBaseClassLibrary(assemblyName),
                        IsMicrosoftAssembly = IsMicrosoftAssembly(assemblyName),
                        SecurityRisk = DetermineSecurityRisk(assemblyName)
                    };

                    assemblyDependencies.Add(assemblyKey, assemblyDependency);
                }

                var dependency = assemblyDependencies[assemblyKey];
                dependency.UsageCount += referencedType.ReferenceCount;
                
                if (!dependency.UsedNamespaces.Contains(referencedType.Namespace))
                {
                    dependency.UsedNamespaces.Add(referencedType.Namespace);
                }

                dependency.UsedTypes.Add(new AssemblyTypeUsage
                {
                    TypeName = referencedType.Name,
                    FullTypeName = referencedType.FullName,
                    Namespace = referencedType.Namespace,
                    UsageKinds = MapToTypeUsageKinds(referencedType.ReferenceKind),
                    UsageCount = referencedType.ReferenceCount,
                    UsageLocations = referencedType.ReferenceLocations
                });
            }
        }

        graph.AssemblyDependencies = assemblyDependencies.Values.ToList();
    }

    /// <summary>
    /// Determines the source of an assembly
    /// </summary>
    private AssemblySource DetermineAssemblySource(string assemblyName)
    {
        if (assemblyName.StartsWith("System.") || assemblyName == "mscorlib" || assemblyName == "netstandard")
            return AssemblySource.Framework;
        
        // This is simplified - in practice, you'd check package references, GAC, etc.
        return AssemblySource.NuGet;
    }

    /// <summary>
    /// Determines the importance of an assembly dependency
    /// </summary>
    private DependencyImportance DetermineAssemblyImportance(string assemblyName)
    {
        // Core framework assemblies are critical
        if (assemblyName.StartsWith("System.") || assemblyName == "mscorlib")
            return DependencyImportance.Critical;
        
        // Common Microsoft assemblies are high importance
        if (assemblyName.StartsWith("Microsoft."))
            return DependencyImportance.High;
        
        return DependencyImportance.Moderate;
    }

    /// <summary>
    /// Checks if an assembly is part of the Base Class Library
    /// </summary>
    private bool IsBaseClassLibrary(string assemblyName)
    {
        var bclAssemblies = new[]
        {
            "mscorlib", "System", "System.Core", "System.Runtime", "System.Collections",
            "System.Linq", "System.Text", "System.Threading", "System.IO", "netstandard"
        };
        
        return bclAssemblies.Any(bcl => assemblyName.StartsWith(bcl));
    }

    /// <summary>
    /// Checks if an assembly is owned by Microsoft
    /// </summary>
    private bool IsMicrosoftAssembly(string assemblyName)
    {
        return assemblyName.StartsWith("System.") || 
               assemblyName.StartsWith("Microsoft.") ||
               assemblyName == "mscorlib" || 
               assemblyName == "netstandard";
    }

    /// <summary>
    /// Determines the security risk level of an assembly
    /// </summary>
    private SecurityRiskLevel DetermineSecurityRisk(string assemblyName)
    {
        // Microsoft and .NET framework assemblies are very low risk
        if (IsMicrosoftAssembly(assemblyName))
            return SecurityRiskLevel.VeryLow;
        
        // Well-known third-party libraries (this would be expanded in practice)
        var trustedAssemblies = new[] { "Newtonsoft.Json", "AutoMapper", "Serilog" };
        if (trustedAssemblies.Any(assemblyName.StartsWith))
            return SecurityRiskLevel.Low;
        
        return SecurityRiskLevel.Unknown;
    }

    /// <summary>
    /// Gets location information from a syntax node
    /// </summary>
    private CSharpLocationInfo GetLocationInfo(SyntaxNode? node)
    {
        if (node == null) return new CSharpLocationInfo();

        var location = node.GetLocation();
        var span = location.GetLineSpan();

        return new CSharpLocationInfo
        {
            FilePath = span.Path ?? "",
            StartLine = span.StartLinePosition.Line + 1,
            StartColumn = span.StartLinePosition.Character + 1,
            EndLine = span.EndLinePosition.Line + 1,
            EndColumn = span.EndLinePosition.Character + 1
        };
    }
}