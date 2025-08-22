using KnowledgeNetwork.Domains.Code.Models.Common;

namespace KnowledgeNetwork.Domains.Code.Models.Classes;

/// <summary>
/// Represents a composition or aggregation relationship between classes
/// </summary>
public class CompositionEdge
{
    /// <summary>
    /// Unique identifier for this edge
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// ID of the containing/owning class
    /// </summary>
    public string ContainerClassId { get; set; } = string.Empty;

    /// <summary>
    /// ID or name of the contained class
    /// </summary>
    public string ContainedClassId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the contained class/type
    /// </summary>
    public string ContainedClassName { get; set; } = string.Empty;

    /// <summary>
    /// Name of the field/property that creates this relationship
    /// </summary>
    public string MemberName { get; set; } = string.Empty;

    /// <summary>
    /// Type of composition relationship
    /// </summary>
    public CompositionType CompositionType { get; set; } = CompositionType.Aggregation;

    /// <summary>
    /// How the contained object is accessed (field, property, collection, etc.)
    /// </summary>
    public CompositionAccessType AccessType { get; set; } = CompositionAccessType.Field;

    /// <summary>
    /// Multiplicity of the relationship
    /// </summary>
    public CompositionMultiplicity Multiplicity { get; set; } = CompositionMultiplicity.One;

    /// <summary>
    /// Whether the relationship is nullable
    /// </summary>
    public bool IsNullable { get; set; }

    /// <summary>
    /// Whether this is a readonly relationship
    /// </summary>
    public bool IsReadonly { get; set; }

    /// <summary>
    /// Whether the contained type is generic
    /// </summary>
    public bool IsGenericType { get; set; }

    /// <summary>
    /// Generic type arguments if applicable
    /// </summary>
    public List<string> GenericTypeArguments { get; set; } = new();

    /// <summary>
    /// Visibility of the member creating this relationship
    /// </summary>
    public string MemberVisibility { get; set; } = string.Empty;

    /// <summary>
    /// Source location where the composition is declared
    /// </summary>
    public CSharpLocationInfo? CompositionLocation { get; set; }

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Types of composition relationships
/// </summary>
public enum CompositionType
{
    /// <summary>
    /// Weak composition - contained object can exist independently
    /// </summary>
    Aggregation,

    /// <summary>
    /// Strong composition - contained object's lifetime is tied to container
    /// </summary>
    Composition,

    /// <summary>
    /// Association - loose relationship, typically through method parameters or return types
    /// </summary>
    Association,

    /// <summary>
    /// Dependency - uses the other class but doesn't store a reference
    /// </summary>
    Dependency
}

/// <summary>
/// How the composed object is accessed
/// </summary>
public enum CompositionAccessType
{
    /// <summary>
    /// Direct field access
    /// </summary>
    Field,

    /// <summary>
    /// Property access
    /// </summary>
    Property,

    /// <summary>
    /// Collection (List, Array, etc.)
    /// </summary>
    Collection,

    /// <summary>
    /// Dictionary or map
    /// </summary>
    Dictionary,

    /// <summary>
    /// Auto-property
    /// </summary>
    AutoProperty,

    /// <summary>
    /// Constructor parameter
    /// </summary>
    Constructor
}

/// <summary>
/// Multiplicity of the composition relationship
/// </summary>
public enum CompositionMultiplicity
{
    /// <summary>
    /// Single instance (1:1)
    /// </summary>
    One,

    /// <summary>
    /// Optional single instance (0:1)
    /// </summary>
    ZeroOrOne,

    /// <summary>
    /// Multiple instances (1:many)
    /// </summary>
    Many,

    /// <summary>
    /// Optional multiple instances (0:many)
    /// </summary>
    ZeroOrMany
}