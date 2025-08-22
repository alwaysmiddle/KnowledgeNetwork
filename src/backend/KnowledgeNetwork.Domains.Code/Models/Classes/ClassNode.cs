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

/// <summary>
/// Visibility levels for classes
/// </summary>
public enum ClassVisibility
{
    Private,
    Protected,
    Internal,
    Public,
    ProtectedInternal,
    PrivateProtected
}

/// <summary>
/// Types of classes/types
/// </summary>
public enum ClassType
{
    Class,
    Interface,
    Struct,
    Enum,
    Record,
    RecordStruct,
    Delegate
}

/// <summary>
/// Summary of class members
/// </summary>
public class ClassMemberSummary
{
    /// <summary>
    /// Number of fields in the class
    /// </summary>
    public int FieldCount { get; set; }

    /// <summary>
    /// Number of properties in the class
    /// </summary>
    public int PropertyCount { get; set; }

    /// <summary>
    /// Number of methods in the class (excluding constructors)
    /// </summary>
    public int MethodCount { get; set; }

    /// <summary>
    /// Number of constructors
    /// </summary>
    public int ConstructorCount { get; set; }

    /// <summary>
    /// Number of nested types
    /// </summary>
    public int NestedTypeCount { get; set; }

    /// <summary>
    /// Number of events
    /// </summary>
    public int EventCount { get; set; }

    /// <summary>
    /// Number of indexers
    /// </summary>
    public int IndexerCount { get; set; }

    /// <summary>
    /// Number of operators
    /// </summary>
    public int OperatorCount { get; set; }
}

/// <summary>
/// Complexity metrics for a class
/// </summary>
public class ClassComplexityMetrics
{
    /// <summary>
    /// Total lines of code in the class
    /// </summary>
    public int TotalLineCount { get; set; }

    /// <summary>
    /// Number of public members
    /// </summary>
    public int PublicMemberCount { get; set; }

    /// <summary>
    /// Number of dependencies (other classes this class uses)
    /// </summary>
    public int DependencyCount { get; set; }

    /// <summary>
    /// Number of classes that depend on this class
    /// </summary>
    public int DependentCount { get; set; }

    /// <summary>
    /// Inheritance depth (how many levels deep in inheritance hierarchy)
    /// </summary>
    public int InheritanceDepth { get; set; }

    /// <summary>
    /// Number of interfaces implemented
    /// </summary>
    public int InterfaceCount { get; set; }

    /// <summary>
    /// Weighted Methods per Class (WMC) - sum of method complexities
    /// </summary>
    public int WeightedMethodsPerClass { get; set; }

    /// <summary>
    /// Response for Class (RFC) - number of methods that can be invoked
    /// </summary>
    public int ResponseForClass { get; set; }

    /// <summary>
    /// Lack of Cohesion of Methods (LCOM) - measure of class cohesion
    /// </summary>
    public double LackOfCohesion { get; set; }

    /// <summary>
    /// Coupling Between Objects (CBO) - number of classes coupled to this class
    /// </summary>
    public int CouplingBetweenObjects { get; set; }
}