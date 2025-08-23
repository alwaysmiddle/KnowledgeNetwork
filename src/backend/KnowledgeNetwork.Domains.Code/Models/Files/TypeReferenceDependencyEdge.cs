using KnowledgeNetwork.Domains.Code.Models.Common;

namespace KnowledgeNetwork.Domains.Code.Models.Files;

/// <summary>
/// Represents a dependency created by type references between files
/// </summary>
public class TypeReferenceDependencyEdge
{
    /// <summary>
    /// Unique identifier for this edge
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// ID of the file that references the type (source)
    /// </summary>
    public string SourceFileId { get; set; } = string.Empty;

    /// <summary>
    /// ID of the file that declares the type (target)
    /// </summary>
    public string TargetFileId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the type being referenced
    /// </summary>
    public string TypeName { get; set; } = string.Empty;

    /// <summary>
    /// Full qualified name of the type
    /// </summary>
    public string FullTypeName { get; set; } = string.Empty;

    /// <summary>
    /// Namespace of the referenced type
    /// </summary>
    public string TypeNamespace { get; set; } = string.Empty;

    /// <summary>
    /// Kind of type being referenced
    /// </summary>
    public string TypeKind { get; set; } = string.Empty;

    /// <summary>
    /// How the type is being referenced
    /// </summary>
    public TypeReferenceContext ReferenceContext { get; set; } = TypeReferenceContext.Usage;

    /// <summary>
    /// Specific usage patterns for this type reference
    /// </summary>
    public List<TypeUsagePattern> UsagePatterns { get; set; } = new();

    /// <summary>
    /// Number of times this type is referenced
    /// </summary>
    public int ReferenceCount { get; set; } = 1;

    /// <summary>
    /// Strength of the type reference dependency
    /// </summary>
    public TypeReferenceDependencyStrength Strength { get; set; } = TypeReferenceDependencyStrength.Weak;

    /// <summary>
    /// Whether this is a bidirectional dependency
    /// </summary>
    public bool IsBidirectional { get; set; }

    /// <summary>
    /// Whether this reference is polymorphic (through interfaces or base classes)
    /// </summary>
    public bool IsPolymorphic { get; set; }

    /// <summary>
    /// Whether this reference involves generics
    /// </summary>
    public bool IsGeneric { get; set; }

    /// <summary>
    /// Generic type arguments if applicable
    /// </summary>
    public List<string> GenericTypeArguments { get; set; } = new();

    /// <summary>
    /// Whether this is an optional dependency (nullable or optional parameters)
    /// </summary>
    public bool IsOptional { get; set; }

    /// <summary>
    /// Impact level if this dependency were to be removed
    /// </summary>
    public DependencyImpactLevel ImpactLevel { get; set; } = DependencyImpactLevel.Low;

    /// <summary>
    /// Source locations where the type is referenced
    /// </summary>
    public List<CSharpLocationInfo> ReferenceLocations { get; set; } = new();

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Context in which a type is referenced
/// </summary>
public enum TypeReferenceContext
{
    /// <summary>
    /// General usage of the type
    /// </summary>
    Usage,

    /// <summary>
    /// Inheritance relationship
    /// </summary>
    Inheritance,

    /// <summary>
    /// Interface implementation
    /// </summary>
    InterfaceImplementation,

    /// <summary>
    /// Composition (has-a relationship)
    /// </summary>
    Composition,

    /// <summary>
    /// Aggregation (uses-a relationship)
    /// </summary>
    Aggregation,

    /// <summary>
    /// Association (knows-about relationship)
    /// </summary>
    Association,

    /// <summary>
    /// Dependency injection
    /// </summary>
    DependencyInjection,

    /// <summary>
    /// Factory pattern usage
    /// </summary>
    Factory,

    /// <summary>
    /// Observer pattern usage
    /// </summary>
    Observer,

    /// <summary>
    /// Strategy pattern usage
    /// </summary>
    Strategy
}

/// <summary>
/// Specific patterns of how a type is used
/// </summary>
public enum TypeUsagePattern
{
    /// <summary>
    /// Field or property declaration
    /// </summary>
    FieldDeclaration,

    /// <summary>
    /// Method parameter
    /// </summary>
    MethodParameter,

    /// <summary>
    /// Method return type
    /// </summary>
    MethodReturnType,

    /// <summary>
    /// Local variable declaration
    /// </summary>
    LocalVariable,

    /// <summary>
    /// Constructor parameter
    /// </summary>
    ConstructorParameter,

    /// <summary>
    /// Generic type constraint
    /// </summary>
    GenericConstraint,

    /// <summary>
    /// Attribute application
    /// </summary>
    AttributeApplication,

    /// <summary>
    /// Exception handling (catch clause)
    /// </summary>
    ExceptionHandling,

    /// <summary>
    /// Type casting or conversion
    /// </summary>
    TypeCasting,

    /// <summary>
    /// Pattern matching (is/as operators)
    /// </summary>
    PatternMatching,

    /// <summary>
    /// Reflection usage (typeof, GetType)
    /// </summary>
    ReflectionUsage,

    /// <summary>
    /// Array or collection element type
    /// </summary>
    CollectionElement,

    /// <summary>
    /// Event declaration
    /// </summary>
    EventDeclaration,

    /// <summary>
    /// Delegate declaration
    /// </summary>
    DelegateDeclaration,

    /// <summary>
    /// Anonymous type usage
    /// </summary>
    AnonymousType
}

/// <summary>
/// Strength of type reference dependency
/// </summary>
public enum TypeReferenceDependencyStrength
{
    /// <summary>
    /// Weak dependency - loosely coupled, easily replaceable
    /// </summary>
    Weak,

    /// <summary>
    /// Moderate dependency - some coupling, requires refactoring to replace
    /// </summary>
    Moderate,

    /// <summary>
    /// Strong dependency - tightly coupled, significant refactoring to replace
    /// </summary>
    Strong,

    /// <summary>
    /// Critical dependency - fundamental to design, very difficult to replace
    /// </summary>
    Critical
}

/// <summary>
/// Impact level of removing a dependency
/// </summary>
public enum DependencyImpactLevel
{
    /// <summary>
    /// Low impact - easy to remove or replace
    /// </summary>
    Low,

    /// <summary>
    /// Medium impact - some refactoring required
    /// </summary>
    Medium,

    /// <summary>
    /// High impact - significant refactoring required
    /// </summary>
    High,

    /// <summary>
    /// Critical impact - would break core functionality
    /// </summary>
    Critical
}