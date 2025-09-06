using KnowledgeNetwork.Domains.Code.Models.Classes.ClassEnums;
using KnowledgeNetwork.Domains.Code.Models.Common;
using KnowledgeNetwork.Domains.Code.Models.Enums;

namespace KnowledgeNetwork.Domains.Code.Models.Classes;

/// <summary>
/// Represents a class node in a class relationship graph
/// </summary>
public class ClassNode
{
    /// <summary>
    /// Unique identifier for this class
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Class name (without namespace)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Full qualified name including namespace
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Namespace the class belongs to
    /// </summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// Class visibility (public, internal, private, etc.)
    /// </summary>
    public ClassVisibility Visibility { get; set; } = ClassVisibility.Internal;

    /// <summary>
    /// Type of class (class, interface, struct, enum, record, delegate)
    /// </summary>
    public ClassType ClassType { get; set; } = ClassType.Class;

    /// <summary>
    /// Whether the class is static
    /// </summary>
    public bool IsStatic { get; set; }

    /// <summary>
    /// Whether the class is abstract
    /// </summary>
    public bool IsAbstract { get; set; }

    /// <summary>
    /// Whether the class is sealed
    /// </summary>
    public bool IsSealed { get; set; }

    /// <summary>
    /// Whether the class is partial
    /// </summary>
    public bool IsPartial { get; set; }

    /// <summary>
    /// Whether this is a nested class
    /// </summary>
    public bool IsNested { get; set; }

    /// <summary>
    /// Whether this is a generic class
    /// </summary>
    public bool IsGeneric { get; set; }

    /// <summary>
    /// Generic type parameters if any
    /// </summary>
    public List<string> GenericTypeParameters { get; set; } = new();

    /// <summary>
    /// Direct base class name (if any)
    /// </summary>
    public string? BaseClassName { get; set; }

    /// <summary>
    /// Interfaces implemented directly by this class
    /// </summary>
    public List<string> ImplementedInterfaces { get; set; } = new();

    /// <summary>
    /// Summary information about class members
    /// </summary>
    public ClassMemberSummary MemberSummary { get; set; } = new();

    /// <summary>
    /// Source location information
    /// </summary>
    public CSharpLocationInfo? Location { get; set; }

    /// <summary>
    /// Complexity metrics for this class
    /// </summary>
    public ClassComplexityMetrics Metrics { get; set; } = new();

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}