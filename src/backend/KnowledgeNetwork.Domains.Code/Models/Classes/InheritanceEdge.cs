using KnowledgeNetwork.Domains.Code.Models.Classes.ClassEnums;
using KnowledgeNetwork.Domains.Code.Models.Common;

namespace KnowledgeNetwork.Domains.Code.Models.Classes;

/// <summary>
/// Represents an inheritance relationship between classes (child : parent)
/// </summary>
public class InheritanceEdge
{
    /// <summary>
    /// Unique identifier for this edge
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// ID of the child class (inheriting class)
    /// </summary>
    public string ChildClassId { get; set; } = string.Empty;

    /// <summary>
    /// ID of the parent class (base class)
    /// </summary>
    public string ParentClassId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the parent class
    /// </summary>
    public string ParentClassName { get; set; } = string.Empty;

    /// <summary>
    /// Type of inheritance relationship
    /// </summary>
    public InheritanceType InheritanceType { get; set; } = InheritanceType.Class;

    /// <summary>
    /// Whether the inheritance crosses assembly boundaries
    /// </summary>
    public bool IsCrossAssembly { get; set; }

    /// <summary>
    /// Whether the inheritance crosses namespace boundaries
    /// </summary>
    public bool IsCrossNamespace { get; set; }

    /// <summary>
    /// Depth level in the inheritance hierarchy (0 = direct parent)
    /// </summary>
    public int HierarchyLevel { get; set; }

    /// <summary>
    /// Source location where the inheritance is declared
    /// </summary>
    public CSharpLocationInfo? InheritanceLocation { get; set; }

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}