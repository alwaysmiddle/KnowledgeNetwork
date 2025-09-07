using KnowledgeNetwork.Domains.Code.Models.Classes.ClassEnums;
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
    public List<string> GenericTypeArguments { get; set; } = [];

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