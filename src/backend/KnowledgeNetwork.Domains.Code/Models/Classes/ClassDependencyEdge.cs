using KnowledgeNetwork.Domains.Code.Models.Common;

namespace KnowledgeNetwork.Domains.Code.Models.Classes;

/// <summary>
/// Represents a dependency relationship between classes (usage without ownership)
/// </summary>
public class ClassDependencyEdge
{
    /// <summary>
    /// Unique identifier for this edge
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// ID of the class that has the dependency (source)
    /// </summary>
    public string SourceClassId { get; set; } = string.Empty;

    /// <summary>
    /// ID or name of the class being depended upon (target)
    /// </summary>
    public string TargetClassId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the target class/type
    /// </summary>
    public string TargetClassName { get; set; } = string.Empty;

    /// <summary>
    /// Type of dependency relationship
    /// </summary>
    public ClassDependencyType DependencyType { get; set; } = ClassDependencyType.Usage;

    /// <summary>
    /// How the dependency is used
    /// </summary>
    public List<DependencyUsage> UsageTypes { get; set; } = new();

    /// <summary>
    /// Strength of the dependency
    /// </summary>
    public DependencyStrength Strength { get; set; } = DependencyStrength.Weak;

    /// <summary>
    /// Number of times this class is referenced
    /// </summary>
    public int ReferenceCount { get; set; } = 1;

    /// <summary>
    /// Whether the dependency crosses assembly boundaries
    /// </summary>
    public bool IsCrossAssembly { get; set; }

    /// <summary>
    /// Whether the dependency crosses namespace boundaries
    /// </summary>
    public bool IsCrossNamespace { get; set; }

    /// <summary>
    /// Whether the target is a generic type
    /// </summary>
    public bool IsGenericTarget { get; set; }

    /// <summary>
    /// Generic type arguments if applicable
    /// </summary>
    public List<string> GenericTypeArguments { get; set; } = new();

    /// <summary>
    /// Source locations where the dependency is used
    /// </summary>
    public List<CSharpLocationInfo> UsageLocations { get; set; } = new();

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Types of class dependencies
/// </summary>
public enum ClassDependencyType
{
    /// <summary>
    /// General usage dependency
    /// </summary>
    Usage,

    /// <summary>
    /// Method parameter dependency
    /// </summary>
    Parameter,

    /// <summary>
    /// Return type dependency
    /// </summary>
    ReturnType,

    /// <summary>
    /// Local variable dependency
    /// </summary>
    LocalVariable,

    /// <summary>
    /// Static method call dependency
    /// </summary>
    StaticCall,

    /// <summary>
    /// Exception handling dependency
    /// </summary>
    Exception,

    /// <summary>
    /// Attribute usage dependency
    /// </summary>
    Attribute,

    /// <summary>
    /// Generic constraint dependency
    /// </summary>
    GenericConstraint,

    /// <summary>
    /// Typeof or reflection dependency
    /// </summary>
    Reflection
}

/// <summary>
/// Specific ways a dependency is used
/// </summary>
public enum DependencyUsage
{
    /// <summary>
    /// Used as method parameter
    /// </summary>
    MethodParameter,

    /// <summary>
    /// Used as return type
    /// </summary>
    ReturnType,

    /// <summary>
    /// Used in field/property type
    /// </summary>
    MemberType,

    /// <summary>
    /// Used as local variable
    /// </summary>
    LocalVariable,

    /// <summary>
    /// Used in static method call
    /// </summary>
    StaticMethodCall,

    /// <summary>
    /// Used in constructor call
    /// </summary>
    ConstructorCall,

    /// <summary>
    /// Used in exception handling
    /// </summary>
    ExceptionHandling,

    /// <summary>
    /// Used as attribute
    /// </summary>
    Attribute,

    /// <summary>
    /// Used in generic constraint
    /// </summary>
    GenericConstraint,

    /// <summary>
    /// Used in typeof expression
    /// </summary>
    TypeOf,

    /// <summary>
    /// Used in cast operation
    /// </summary>
    Cast,

    /// <summary>
    /// Used in is/as pattern
    /// </summary>
    PatternMatching
}

/// <summary>
/// Strength of dependency relationship
/// </summary>
public enum DependencyStrength
{
    /// <summary>
    /// Weak dependency - only used occasionally or in specific contexts
    /// </summary>
    Weak,

    /// <summary>
    /// Moderate dependency - used in multiple places but not core to functionality
    /// </summary>
    Moderate,

    /// <summary>
    /// Strong dependency - heavily used and core to class functionality
    /// </summary>
    Strong,

    /// <summary>
    /// Critical dependency - class cannot function without this dependency
    /// </summary>
    Critical
}