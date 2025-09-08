using KnowledgeNetwork.Domains.Code.Analyzers.Files.Abstractions;
using KnowledgeNetwork.Domains.Code.Models.Files;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Files.DependencyAnalyzers;

/// <summary>
/// Analyzes external assembly dependencies for files
/// </summary>
public class AssemblyDependencyAnalyzer(ILogger<AssemblyDependencyAnalyzer> logger) : IAssemblyDependencyAnalyzer
{
    private readonly ILogger<AssemblyDependencyAnalyzer> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Analyzes external assembly dependencies for all files in the graph
    /// </summary>
    public async Task AnalyzeAsync(Compilation compilation, FileDependencyGraph graph)
    {
        _logger.LogDebug("Starting assembly dependency analysis for {FileCount} files", graph.Files.Count);

        try
        {
            var dependencyCount = 0;
            var processedAssemblies = new HashSet<string>();

            foreach (var sourceFile in graph.Files)
            {
                // Analyze external type references to determine assembly dependencies
                foreach (var referencedType in sourceFile.ReferencedTypes)
                {
                    // Only process external types
                    if (!referencedType.IsExternal || string.IsNullOrEmpty(referencedType.Assembly))
                        continue;

                    var assemblyKey = $"{sourceFile.Id}:{referencedType.Assembly}";
                    
                    // Skip if we've already processed this file-assembly combination
                    if (processedAssemblies.Contains(assemblyKey))
                        continue;

                    processedAssemblies.Add(assemblyKey);

                    // Try to get assembly information from compilation
                    var assemblyInfo = GetAssemblyInfo(compilation, referencedType.Assembly);
                    if (assemblyInfo == null) continue;

                    var assemblyDependency = CreateAssemblyDependency(sourceFile, referencedType, assemblyInfo);
                    
                    // Collect all types used from this assembly for this file
                    CollectAssemblyTypeUsages(assemblyDependency, sourceFile, referencedType.Assembly);

                    graph.AssemblyDependencies.Add(assemblyDependency);
                    dependencyCount++;

                    _logger.LogTrace("Found assembly dependency: {SourceFile} depends on assembly {AssemblyName}",
                        sourceFile.FileName, referencedType.Assembly);
                }

                // Also analyze explicit assembly references from the file
                foreach (var assemblyReference in sourceFile.AssemblyReferences)
                {
                    var assemblyKey = $"{sourceFile.Id}:{assemblyReference}";
                    
                    if (processedAssemblies.Contains(assemblyKey))
                        continue;

                    processedAssemblies.Add(assemblyKey);

                    var assemblyInfo = GetAssemblyInfo(compilation, assemblyReference);
                    if (assemblyInfo == null) continue;

                    var assemblyDependency = CreateAssemblyDependencyFromReference(sourceFile, assemblyReference, assemblyInfo);
                    
                    // Collect type usages from this assembly
                    CollectAssemblyTypeUsages(assemblyDependency, sourceFile, assemblyReference);

                    graph.AssemblyDependencies.Add(assemblyDependency);
                    dependencyCount++;

                    _logger.LogTrace("Found explicit assembly dependency: {SourceFile} references assembly {AssemblyName}",
                        sourceFile.FileName, assemblyReference);
                }
            }

            _logger.LogDebug("Completed assembly dependency analysis. Found {DependencyCount} assembly dependencies", dependencyCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during assembly dependency analysis");
            throw;
        }
    }

    /// <summary>
    /// Gets assembly information from the compilation
    /// </summary>
    private IAssemblySymbol? GetAssemblyInfo(Compilation compilation, string assemblyName)
    {
        try
        {
            // First try to find by simple name
            var assembly = compilation.References
                .Select(compilation.GetAssemblyOrModuleSymbol)
                .OfType<IAssemblySymbol>()
                .FirstOrDefault(a => a.Name == assemblyName);

            if (assembly != null) return assembly;

            // Try to find by partial name match (for cases with version info)
            assembly = compilation.References
                .Select(compilation.GetAssemblyOrModuleSymbol)
                .OfType<IAssemblySymbol>()
                .FirstOrDefault(a => a.Identity.Name == assemblyName || 
                                   a.Identity.GetDisplayName().StartsWith(assemblyName));

            return assembly;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get assembly info for {AssemblyName}", assemblyName);
            return null;
        }
    }

    /// <summary>
    /// Creates assembly dependency from a referenced type
    /// </summary>
    private AssemblyDependencyEdge CreateAssemblyDependency(FileNode sourceFile, ReferencedType referencedType, IAssemblySymbol assemblyInfo)
    {
        var dependency = new AssemblyDependencyEdge
        {
            SourceFileId = sourceFile.Id,
            AssemblyName = assemblyInfo.Name,
            AssemblyFullName = assemblyInfo.Identity.GetDisplayName(),
            AssemblyVersion = assemblyInfo.Identity.Version.ToString(),
            DependencyType = DetermineAssemblyDependencyType(sourceFile),
            Source = DetermineAssemblySource(assemblyInfo),
            IsDirectDependency = true, // Simplified - assume direct
            UsageCount = referencedType.ReferenceCount,
            Importance = DetermineImportance(referencedType.ReferenceCount),
            IntroductionLocation = referencedType.ReferenceLocations.FirstOrDefault()
        };

        // Add used namespaces
        if (!string.IsNullOrEmpty(referencedType.Namespace))
        {
            dependency.UsedNamespaces.Add(referencedType.Namespace);
        }

        return dependency;
    }

    /// <summary>
    /// Creates assembly dependency from an explicit assembly reference
    /// </summary>
    private AssemblyDependencyEdge CreateAssemblyDependencyFromReference(FileNode sourceFile, string assemblyReference, IAssemblySymbol assemblyInfo)
    {
        var dependency = new AssemblyDependencyEdge
        {
            SourceFileId = sourceFile.Id,
            AssemblyName = assemblyInfo.Name,
            AssemblyFullName = assemblyInfo.Identity.GetDisplayName(),
            AssemblyVersion = assemblyInfo.Identity.Version.ToString(),
            DependencyType = DetermineAssemblyDependencyType(sourceFile),
            Source = DetermineAssemblySource(assemblyInfo),
            IsDirectDependency = true, // Simplified - assume direct
            UsageCount = 0, // Will be updated by CollectAssemblyTypeUsages
            Importance = DependencyImportance.Moderate // Default for explicit references
        };

        return dependency;
    }

    /// <summary>
    /// Collects all type usages from a specific assembly for a file
    /// </summary>
    private void CollectAssemblyTypeUsages(AssemblyDependencyEdge dependency, FileNode sourceFile, string assemblyName)
    {
        var assemblyTypes = sourceFile.ReferencedTypes
            .Where(t => t.IsExternal && t.Assembly == assemblyName)
            .GroupBy(t => t.FullName)
            .ToList();

        foreach (var typeGroup in assemblyTypes)
        {
            var firstType = typeGroup.First();
            var totalUsageCount = typeGroup.Sum(t => t.ReferenceCount);
            var allLocations = typeGroup.SelectMany(t => t.ReferenceLocations).ToList();

            var typeUsage = new AssemblyTypeUsage
            {
                TypeName = firstType.Name,
                FullTypeName = firstType.FullName,
                Namespace = firstType.Namespace,
                UsageKinds = DetermineTypeUsageKinds(typeGroup.Select(t => t.ReferenceKind).ToList()),
                UsageCount = totalUsageCount,
                IsCritical = DetermineIfTypeCritical(firstType.ReferenceKind, totalUsageCount),
                UsageLocations = allLocations
            };

            dependency.UsedTypes.Add(typeUsage);
            dependency.UsageCount += totalUsageCount;

            // Add namespace if not already present
            if (!string.IsNullOrEmpty(firstType.Namespace) && !dependency.UsedNamespaces.Contains(firstType.Namespace))
            {
                dependency.UsedNamespaces.Add(firstType.Namespace);
            }
        }

        // Update importance based on total usage
        dependency.Importance = DetermineImportance(dependency.UsageCount);
    }

    /// <summary>
    /// Determines the type of assembly dependency
    /// </summary>
    private AssemblyDependencyType DetermineAssemblyDependencyType(FileNode sourceFile)
    {
        // Check if it's a test file
        if (sourceFile.FileType == FileType.Test || sourceFile.FilePath.Contains("Test", StringComparison.OrdinalIgnoreCase))
            return AssemblyDependencyType.Test;

        // Default to reference for simplicity
        return AssemblyDependencyType.Reference;
    }

    /// <summary>
    /// Determines the source of an assembly
    /// </summary>
    private AssemblySource DetermineAssemblySource(IAssemblySymbol assemblyInfo)
    {
        var assemblyLocation = assemblyInfo.Locations.FirstOrDefault()?.MetadataModule?.Name;
        
        if (string.IsNullOrEmpty(assemblyLocation))
            return AssemblySource.Unknown;

        // Check for NuGet packages
        if (assemblyLocation.Contains("packages", StringComparison.OrdinalIgnoreCase) ||
            assemblyLocation.Contains(".nuget", StringComparison.OrdinalIgnoreCase))
            return AssemblySource.NuGet;

        // Check for project references
        if (assemblyLocation.Contains("bin", StringComparison.OrdinalIgnoreCase))
            return AssemblySource.ProjectReference;

        return AssemblySource.Local;
    }


    /// <summary>
    /// Determines dependency importance based on usage count
    /// </summary>
    private DependencyImportance DetermineImportance(int usageCount)
    {
        return usageCount switch
        {
            >= 50 => DependencyImportance.Critical,
            >= 20 => DependencyImportance.High,
            >= 5 => DependencyImportance.Moderate,
            _ => DependencyImportance.Low
        };
    }


    /// <summary>
    /// Determines type usage kinds from reference kinds
    /// </summary>
    private List<TypeUsageKind> DetermineTypeUsageKinds(List<TypeReferenceKind> referenceKinds)
    {
        var usageKinds = new List<TypeUsageKind>();

        foreach (var kind in referenceKinds.Distinct())
        {
            var mappedKinds = kind switch
            {
                TypeReferenceKind.Inheritance => new[] { TypeUsageKind.Inheritance },
                TypeReferenceKind.Interface => new[] { TypeUsageKind.InterfaceImplementation },
                TypeReferenceKind.GenericParameter => new[] { TypeUsageKind.GenericArgument },
                TypeReferenceKind.Attribute => new[] { TypeUsageKind.Attribute },
                TypeReferenceKind.Reflection => new[] { TypeUsageKind.StaticMethodCall },
                _ => new[] { TypeUsageKind.Declaration }
            };

            foreach (var mappedKind in mappedKinds)
            {
                if (!usageKinds.Contains(mappedKind))
                {
                    usageKinds.Add(mappedKind);
                }
            }
        }

        return usageKinds;
    }

    /// <summary>
    /// Determines if a type usage is critical
    /// </summary>
    private bool DetermineIfTypeCritical(TypeReferenceKind referenceKind, int usageCount)
    {
        // Inheritance and interface implementation are always critical
        if (referenceKind == TypeReferenceKind.Inheritance || referenceKind == TypeReferenceKind.Interface)
            return true;

        // High usage count indicates criticality
        return usageCount >= 10;
    }
}