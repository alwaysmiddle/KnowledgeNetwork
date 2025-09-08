using KnowledgeNetwork.Domains.Code.Analyzers.Files.Abstractions;
using KnowledgeNetwork.Domains.Code.Models.Files;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Files.DependencyAnalyzers;

/// <summary>
/// Analyzes type reference dependencies between files
/// </summary>
public class TypeReferenceDependencyAnalyzer(ILogger<TypeReferenceDependencyAnalyzer> logger) : ITypeReferenceDependencyAnalyzer
{
    private readonly ILogger<TypeReferenceDependencyAnalyzer> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Analyzes type reference dependencies between files in the graph
    /// </summary>
    public async Task AnalyzeAsync(Compilation compilation, FileDependencyGraph graph)
    {
        _logger.LogDebug("Starting type reference dependency analysis for {FileCount} files", graph.Files.Count);

        try
        {
            var dependencyCount = 0;

            foreach (var sourceFile in graph.Files)
            {
                foreach (var referencedType in sourceFile.ReferencedTypes)
                {
                    // Skip external types - they don't create internal dependencies
                    if (referencedType.IsExternal || string.IsNullOrEmpty(referencedType.FullName))
                        continue;

                    // Find target file that declares this type
                    var targetFile = FindFileDeclaringType(graph, referencedType.FullName, sourceFile.Id);
                    if (targetFile == null) continue;

                    var typeReferenceDependency = CreateTypeReferenceDependency(sourceFile, targetFile, referencedType);
                    
                    // Check if this dependency already exists and merge if so
                    var existingDependency = graph.TypeReferenceDependencies
                        .FirstOrDefault(d => d.SourceFileId == sourceFile.Id && 
                                           d.TargetFileId == targetFile.Id && 
                                           d.FullTypeName == referencedType.FullName);

                    if (existingDependency != null)
                    {
                        MergeTypeReferenceDependencies(existingDependency, typeReferenceDependency);
                    }
                    else
                    {
                        graph.TypeReferenceDependencies.Add(typeReferenceDependency);
                        dependencyCount++;
                    }

                    _logger.LogTrace("Found type reference dependency: {SourceFile} depends on type {TypeName} in {TargetFile}",
                        sourceFile.FileName, referencedType.Name, targetFile.FileName);
                }
            }

            _logger.LogDebug("Completed type reference dependency analysis. Found {DependencyCount} type dependencies", dependencyCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during type reference dependency analysis");
            throw;
        }
    }

    /// <summary>
    /// Finds the file that declares a specific type
    /// </summary>
    private FileNode? FindFileDeclaringType(FileDependencyGraph graph, string fullTypeName, string sourceFileId)
    {
        return graph.Files.FirstOrDefault(f => 
            f.Id != sourceFileId && 
            f.DeclaredTypes.Any(dt => dt.FullName == fullTypeName));
    }

    /// <summary>
    /// Creates a type reference dependency edge from the given information
    /// </summary>
    private TypeReferenceDependencyEdge CreateTypeReferenceDependency(FileNode sourceFile, FileNode targetFile, ReferencedType referencedType)
    {
        var dependency = new TypeReferenceDependencyEdge
        {
            SourceFileId = sourceFile.Id,
            TargetFileId = targetFile.Id,
            TypeName = referencedType.Name,
            FullTypeName = referencedType.FullName,
            TypeNamespace = referencedType.Namespace,
            TypeKind = GetTypeKindFromReferencedType(targetFile, referencedType.FullName),
            ReferenceContext = MapToTypeReferenceContext(referencedType.ReferenceKind),
            ReferenceCount = referencedType.ReferenceCount,
            Strength = DetermineTypeReferenceDependencyStrength(referencedType.ReferenceKind, referencedType.ReferenceCount),
            ReferenceLocations = referencedType.ReferenceLocations,
            ImpactLevel = DetermineImpactLevel(referencedType.ReferenceKind, referencedType.ReferenceCount)
        };

        // Determine usage patterns
        dependency.UsagePatterns = DetermineUsagePatterns(referencedType.ReferenceKind);

        // Check for generics
        dependency.IsGeneric = referencedType.FullName.Contains('<') && referencedType.FullName.Contains('>');
        if (dependency.IsGeneric)
        {
            dependency.GenericTypeArguments = ExtractGenericTypeArguments(referencedType.FullName);
        }

        // Check for polymorphism (inheritance or interface implementation)
        dependency.IsPolymorphic = referencedType.ReferenceKind == TypeReferenceKind.Inheritance || 
                                  referencedType.ReferenceKind == TypeReferenceKind.Interface;

        return dependency;
    }

    /// <summary>
    /// Merges two type reference dependencies by combining their properties
    /// </summary>
    private void MergeTypeReferenceDependencies(TypeReferenceDependencyEdge existing, TypeReferenceDependencyEdge newDependency)
    {
        // Combine reference counts
        existing.ReferenceCount += newDependency.ReferenceCount;

        // Take the stronger reference context
        if (newDependency.ReferenceContext > existing.ReferenceContext)
        {
            existing.ReferenceContext = newDependency.ReferenceContext;
        }

        // Merge usage patterns
        foreach (var pattern in newDependency.UsagePatterns)
        {
            if (!existing.UsagePatterns.Contains(pattern))
            {
                existing.UsagePatterns.Add(pattern);
            }
        }

        // Merge reference locations
        existing.ReferenceLocations.AddRange(newDependency.ReferenceLocations);

        // Update strength based on new counts
        existing.Strength = DetermineTypeReferenceDependencyStrength(existing.ReferenceContext, existing.ReferenceCount);
        existing.ImpactLevel = DetermineImpactLevel(existing.ReferenceContext, existing.ReferenceCount);

        // Update flags
        existing.IsGeneric = existing.IsGeneric || newDependency.IsGeneric;
        existing.IsPolymorphic = existing.IsPolymorphic || newDependency.IsPolymorphic;

        if (newDependency.IsGeneric && newDependency.GenericTypeArguments.Any())
        {
            foreach (var arg in newDependency.GenericTypeArguments)
            {
                if (!existing.GenericTypeArguments.Contains(arg))
                {
                    existing.GenericTypeArguments.Add(arg);
                }
            }
        }
    }

    /// <summary>
    /// Gets the TypeKind from a declared type in the target file
    /// </summary>
    private string GetTypeKindFromReferencedType(FileNode targetFile, string fullTypeName)
    {
        var declaredType = targetFile.DeclaredTypes.FirstOrDefault(dt => dt.FullName == fullTypeName);
        return declaredType?.TypeKind ?? "Unknown";
    }

    /// <summary>
    /// Maps TypeReferenceKind to TypeReferenceContext
    /// </summary>
    private TypeReferenceContext MapToTypeReferenceContext(TypeReferenceKind referenceKind)
    {
        return referenceKind switch
        {
            TypeReferenceKind.Inheritance => TypeReferenceContext.Inheritance,
            TypeReferenceKind.Interface => TypeReferenceContext.InterfaceImplementation,
            TypeReferenceKind.Direct => TypeReferenceContext.Usage,
            TypeReferenceKind.GenericParameter => TypeReferenceContext.Usage,
            TypeReferenceKind.Attribute => TypeReferenceContext.Usage,
            TypeReferenceKind.Reflection => TypeReferenceContext.Usage,
            _ => TypeReferenceContext.Usage
        };
    }

    /// <summary>
    /// Determines type reference dependency strength based on reference kind and count
    /// </summary>
    private TypeReferenceDependencyStrength DetermineTypeReferenceDependencyStrength(TypeReferenceKind referenceKind, int referenceCount)
    {
        // Inheritance and interface implementation are always strong
        if (referenceKind == TypeReferenceKind.Inheritance || referenceKind == TypeReferenceKind.Interface)
            return TypeReferenceDependencyStrength.Critical;

        // For general usage, base on reference count
        return referenceCount switch
        {
            >= 10 => TypeReferenceDependencyStrength.Critical,
            >= 5 => TypeReferenceDependencyStrength.Strong,
            >= 2 => TypeReferenceDependencyStrength.Moderate,
            _ => TypeReferenceDependencyStrength.Weak
        };
    }

    /// <summary>
    /// Determines type reference dependency strength based on context and count
    /// </summary>
    private TypeReferenceDependencyStrength DetermineTypeReferenceDependencyStrength(TypeReferenceContext context, int referenceCount)
    {
        // Inheritance and interface implementation are always critical
        if (context == TypeReferenceContext.Inheritance || context == TypeReferenceContext.InterfaceImplementation)
            return TypeReferenceDependencyStrength.Critical;

        // Composition creates strong coupling
        if (context == TypeReferenceContext.Composition)
            return TypeReferenceDependencyStrength.Strong;

        // Dependency injection and design patterns create moderate coupling
        if (context == TypeReferenceContext.DependencyInjection || 
            context == TypeReferenceContext.Factory ||
            context == TypeReferenceContext.Observer ||
            context == TypeReferenceContext.Strategy)
            return TypeReferenceDependencyStrength.Moderate;

        // For general usage, base on reference count
        return referenceCount switch
        {
            >= 10 => TypeReferenceDependencyStrength.Critical,
            >= 5 => TypeReferenceDependencyStrength.Strong,
            >= 2 => TypeReferenceDependencyStrength.Moderate,
            _ => TypeReferenceDependencyStrength.Weak
        };
    }

    /// <summary>
    /// Determines the impact level of removing a dependency
    /// </summary>
    private DependencyImpactLevel DetermineImpactLevel(TypeReferenceKind referenceKind, int referenceCount)
    {
        // Inheritance and interface implementation have critical impact
        if (referenceKind == TypeReferenceKind.Inheritance || referenceKind == TypeReferenceKind.Interface)
            return DependencyImpactLevel.Critical;

        // For general usage, base on reference count
        return referenceCount switch
        {
            >= 10 => DependencyImpactLevel.Critical,
            >= 5 => DependencyImpactLevel.High,
            >= 2 => DependencyImpactLevel.Medium,
            _ => DependencyImpactLevel.Low
        };
    }

    /// <summary>
    /// Determines the impact level based on context and count
    /// </summary>
    private DependencyImpactLevel DetermineImpactLevel(TypeReferenceContext context, int referenceCount)
    {
        // Inheritance and interface implementation have critical impact
        if (context == TypeReferenceContext.Inheritance || context == TypeReferenceContext.InterfaceImplementation)
            return DependencyImpactLevel.Critical;

        // Composition has high impact
        if (context == TypeReferenceContext.Composition)
            return DependencyImpactLevel.High;

        // Design patterns have medium to high impact
        if (context == TypeReferenceContext.DependencyInjection ||
            context == TypeReferenceContext.Factory ||
            context == TypeReferenceContext.Observer ||
            context == TypeReferenceContext.Strategy)
            return DependencyImpactLevel.Medium;

        // For general usage, base on reference count
        return referenceCount switch
        {
            >= 10 => DependencyImpactLevel.Critical,
            >= 5 => DependencyImpactLevel.High,
            >= 2 => DependencyImpactLevel.Medium,
            _ => DependencyImpactLevel.Low
        };
    }

    /// <summary>
    /// Determines usage patterns based on reference kind
    /// </summary>
    private List<TypeUsagePattern> DetermineUsagePatterns(TypeReferenceKind referenceKind)
    {
        return referenceKind switch
        {
            TypeReferenceKind.Inheritance => [TypeUsagePattern.FieldDeclaration],
            TypeReferenceKind.Interface => [TypeUsagePattern.FieldDeclaration],
            TypeReferenceKind.Direct => [TypeUsagePattern.LocalVariable],
            TypeReferenceKind.GenericParameter => [TypeUsagePattern.GenericConstraint],
            TypeReferenceKind.Attribute => [TypeUsagePattern.AttributeApplication],
            TypeReferenceKind.Reflection => [TypeUsagePattern.ReflectionUsage],
            _ => [TypeUsagePattern.LocalVariable]
        };
    }

    /// <summary>
    /// Extracts generic type arguments from a generic type name
    /// </summary>
    private List<string> ExtractGenericTypeArguments(string fullTypeName)
    {
        var arguments = new List<string>();
        
        if (!fullTypeName.Contains('<') || !fullTypeName.Contains('>'))
            return arguments;

        var start = fullTypeName.IndexOf('<') + 1;
        var end = fullTypeName.LastIndexOf('>');
        
        if (end <= start) return arguments;

        var genericPart = fullTypeName.Substring(start, end - start);
        var parts = genericPart.Split(',', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var part in parts)
        {
            arguments.Add(part.Trim());
        }

        return arguments;
    }
}