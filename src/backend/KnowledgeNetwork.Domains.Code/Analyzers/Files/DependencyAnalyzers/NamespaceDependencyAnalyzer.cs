using KnowledgeNetwork.Domains.Code.Analyzers.Files.Abstractions;
using KnowledgeNetwork.Domains.Code.Models.Files;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Files.DependencyAnalyzers;

/// <summary>
/// Analyzes namespace dependencies between files
/// </summary>
public class NamespaceDependencyAnalyzer(ILogger<NamespaceDependencyAnalyzer> logger) : INamespaceDependencyAnalyzer
{
    private readonly ILogger<NamespaceDependencyAnalyzer> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Analyzes namespace dependencies between files in the graph
    /// </summary>
    public async Task AnalyzeAsync(Compilation compilation, FileDependencyGraph graph)
    {
        _logger.LogDebug("Starting namespace dependency analysis for {FileCount} files", graph.Files.Count);

        try
        {
            var dependencyCount = 0;

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
                    dependencyCount++;
                    
                    _logger.LogTrace("Found namespace dependency: {SourceFile} depends on namespace {TargetNamespace} in {TargetFile}",
                        sourceFile.FileName, referencedType.Namespace, targetFile.FileName);
                }
            }

            _logger.LogDebug("Completed namespace dependency analysis. Found {DependencyCount} namespace dependencies", dependencyCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during namespace dependency analysis");
            throw;
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
    /// Determines namespace dependency strength based on usage count
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
    /// Calculates the distance between two namespaces based on their hierarchical structure
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
}