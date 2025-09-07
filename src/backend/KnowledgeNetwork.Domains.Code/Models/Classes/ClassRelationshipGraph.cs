using KnowledgeNetwork.Domains.Code.Models.Classes.ClassEnums;
using KnowledgeNetwork.Domains.Code.Models.Common;

namespace KnowledgeNetwork.Domains.Code.Models.Classes;

/// <summary>
/// Represents relationships between classes within a file or namespace
/// </summary>
public class ClassRelationshipGraph
{
    /// <summary>
    /// Unique identifier for this graph
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Name/identifier of the analyzed scope (file name, namespace, etc.)
    /// </summary>
    public string ScopeName { get; set; } = string.Empty;

    /// <summary>
    /// Type of scope this graph represents
    /// </summary>
    public ClassAnalysisScope ScopeType { get; set; } = ClassAnalysisScope.File;

    /// <summary>
    /// All classes found in the analyzed scope
    /// </summary>
    public List<ClassNode> Classes { get; set; } = [];

    /// <summary>
    /// Inheritance relationships between classes
    /// </summary>
    public List<InheritanceEdge> InheritanceRelationships { get; set; } = [];

    /// <summary>
    /// Interface implementation relationships
    /// </summary>
    public List<InterfaceImplementationEdge> InterfaceImplementations { get; set; } = [];

    /// <summary>
    /// Composition and aggregation relationships
    /// </summary>
    public List<CompositionEdge> CompositionRelationships { get; set; } = [];

    /// <summary>
    /// Dependency relationships (using other classes)
    /// </summary>
    public List<ClassDependencyEdge> DependencyRelationships { get; set; } = [];

    /// <summary>
    /// Nested class relationships
    /// </summary>
    public List<NestedClassEdge> NestedClassRelationships { get; set; } = [];

    /// <summary>
    /// Source location information
    /// </summary>
    public CSharpLocationInfo? Location { get; set; }

    /// <summary>
    /// Analysis timestamp
    /// </summary>
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Get all classes that the specified class depends on
    /// </summary>
    public List<ClassNode> GetDependenciesFor(string classId)
    {
        var dependencyIds = DependencyRelationships
            .Where(d => d.SourceClassId == classId)
            .Select(d => d.TargetClassId)
            .ToList();

        return Classes.Where(c => dependencyIds.Contains(c.Id)).ToList();
    }

    /// <summary>
    /// Get all classes that depend on the specified class
    /// </summary>
    public List<ClassNode> GetDependentsFor(string classId)
    {
        var dependentIds = DependencyRelationships
            .Where(d => d.TargetClassId == classId)
            .Select(d => d.SourceClassId)
            .ToList();

        return Classes.Where(c => dependentIds.Contains(c.Id)).ToList();
    }

    /// <summary>
    /// Get inheritance hierarchy for a class (all ancestors)
    /// </summary>
    public List<ClassNode> GetInheritanceHierarchyFor(string classId)
    {
        var hierarchy = new List<ClassNode>();
        var currentClassId = classId;

        while (currentClassId != null)
        {
            var inheritance = InheritanceRelationships
                .FirstOrDefault(i => i.ChildClassId == currentClassId);

            if (inheritance == null) break;

            var parentClass = Classes.FirstOrDefault(c => c.Id == inheritance.ParentClassId);
            if (parentClass != null)
            {
                hierarchy.Add(parentClass);
                currentClassId = parentClass.Id;
            }
            else
            {
                break;
            }
        }

        return hierarchy;
    }

    /// <summary>
    /// Get all interfaces implemented by a class (directly and through inheritance)
    /// </summary>
    public List<string> GetAllImplementedInterfaces(string classId)
    {
        var interfaces = new HashSet<string>();
        
        // Direct implementations
        var directInterfaces = InterfaceImplementations
            .Where(i => i.ClassId == classId)
            .Select(i => i.InterfaceName);
        
        foreach (var interfaceName in directInterfaces)
        {
            interfaces.Add(interfaceName);
        }

        // Inherited implementations
        var hierarchy = GetInheritanceHierarchyFor(classId);
        foreach (var interfaceName in hierarchy.Select(ancestorClass => InterfaceImplementations
                     .Where(i => i.ClassId == ancestorClass.Id)
                     .Select(i => i.InterfaceName)).SelectMany(inheritedInterfaces => inheritedInterfaces))
        {
            interfaces.Add(interfaceName);
        }

        return interfaces.ToList();
    }
}