using KnowledgeNetwork.Domains.Code.Models.Common;

namespace KnowledgeNetwork.Domains.Code.Models.Files;

/// <summary>
/// Represents a namespace-level dependency between files
/// </summary>
public class NamespaceDependencyEdge
{
    /// <summary>
    /// Unique identifier for this edge
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// ID of the file that uses types from another namespace (source)
    /// </summary>
    public string SourceFileId { get; set; } = string.Empty;

    /// <summary>
    /// ID of the file that declares the target namespace (target)
    /// </summary>
    public string TargetFileId { get; set; } = string.Empty;

    /// <summary>
    /// The source namespace (where the dependency originates)
    /// </summary>
    public string SourceNamespace { get; set; } = string.Empty;

    /// <summary>
    /// The target namespace being depended upon
    /// </summary>
    public string NamespaceName { get; set; } = string.Empty;

    /// <summary>
    /// Type of namespace dependency
    /// </summary>
    public NamespaceDependencyType DependencyType { get; set; } = NamespaceDependencyType.TypeReference;

    /// <summary>
    /// Strength of the dependency
    /// </summary>
    public NamespaceDependencyStrength Strength { get; set; } = NamespaceDependencyStrength.Weak;

    /// <summary>
    /// Whether this is a cross-project dependency
    /// </summary>
    public bool IsCrossProject { get; set; }

    /// <summary>
    /// Whether this is an external assembly dependency
    /// </summary>
    public bool IsExternalAssembly { get; set; }

    /// <summary>
    /// External assembly name if applicable
    /// </summary>
    public string ExternalAssemblyName { get; set; } = string.Empty;

    /// <summary>
    /// Number of types from the target namespace that are used
    /// </summary>
    public int TypeUsageCount { get; set; } = 1;

    /// <summary>
    /// Specific types from the target namespace that are used
    /// </summary>
    public List<NamespaceTypeUsage> TypeUsages { get; set; } = new();

    /// <summary>
    /// Relationship depth (how many namespace levels apart)
    /// </summary>
    public int NamespaceDistance { get; set; }

    /// <summary>
    /// Whether this dependency creates a potential circular reference
    /// </summary>
    public bool IsCircular { get; set; }

    /// <summary>
    /// Source location where the dependency occurs
    /// </summary>
    public CSharpLocationInfo? DependencyLocation { get; set; }

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Types of namespace dependencies
/// </summary>
public enum NamespaceDependencyType
{
    /// <summary>
    /// Direct type reference
    /// </summary>
    TypeReference,

    /// <summary>
    /// Inheritance relationship
    /// </summary>
    Inheritance,

    /// <summary>
    /// Interface implementation
    /// </summary>
    InterfaceImplementation,

    /// <summary>
    /// Generic type constraint
    /// </summary>
    GenericConstraint,

    /// <summary>
    /// Attribute usage
    /// </summary>
    AttributeUsage,

    /// <summary>
    /// Event declaration
    /// </summary>
    EventDeclaration,

    /// <summary>
    /// Exception handling
    /// </summary>
    ExceptionHandling
}

/// <summary>
/// Strength of namespace dependency
/// </summary>
public enum NamespaceDependencyStrength
{
    /// <summary>
    /// Weak dependency - occasionally used
    /// </summary>
    Weak,

    /// <summary>
    /// Moderate dependency - regularly used
    /// </summary>
    Moderate,

    /// <summary>
    /// Strong dependency - heavily used
    /// </summary>
    Strong,

    /// <summary>
    /// Critical dependency - essential for functionality
    /// </summary>
    Critical
}

/// <summary>
/// Information about how a specific type from a namespace is used
/// </summary>
public class NamespaceTypeUsage
{
    /// <summary>
    /// Name of the type being used
    /// </summary>
    public string TypeName { get; set; } = string.Empty;

    /// <summary>
    /// Full qualified name of the type
    /// </summary>
    public string FullTypeName { get; set; } = string.Empty;

    /// <summary>
    /// How the type is being used
    /// </summary>
    public List<TypeUsageKind> UsageKinds { get; set; } = new();

    /// <summary>
    /// Number of times this type is used
    /// </summary>
    public int UsageCount { get; set; } = 1;

    /// <summary>
    /// Source locations where this type is used
    /// </summary>
    public List<CSharpLocationInfo> UsageLocations { get; set; } = new();
}

/// <summary>
/// Different ways a type can be used
/// </summary>
public enum TypeUsageKind
{
    /// <summary>
    /// Variable or field declaration
    /// </summary>
    Declaration,

    /// <summary>
    /// Method parameter
    /// </summary>
    Parameter,

    /// <summary>
    /// Method return type
    /// </summary>
    ReturnType,

    /// <summary>
    /// Base class inheritance
    /// </summary>
    Inheritance,

    /// <summary>
    /// Interface implementation
    /// </summary>
    InterfaceImplementation,

    /// <summary>
    /// Generic type argument
    /// </summary>
    GenericArgument,

    /// <summary>
    /// Attribute application
    /// </summary>
    Attribute,

    /// <summary>
    /// Constructor invocation
    /// </summary>
    Constructor,

    /// <summary>
    /// Static method call
    /// </summary>
    StaticMethodCall,

    /// <summary>
    /// Type casting
    /// </summary>
    TypeCast,

    /// <summary>
    /// Pattern matching
    /// </summary>
    PatternMatching,

    /// <summary>
    /// Exception handling
    /// </summary>
    ExceptionHandling
}