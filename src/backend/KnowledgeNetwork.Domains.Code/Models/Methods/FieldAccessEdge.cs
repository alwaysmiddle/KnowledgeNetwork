using KnowledgeNetwork.Domains.Code.Models.Common;

namespace KnowledgeNetwork.Domains.Code.Models.Methods;

/// <summary>
/// Represents a field or property access by a method
/// </summary>
public class FieldAccessEdge
{
    /// <summary>
    /// Unique identifier for this edge
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// ID of the method accessing the field/property
    /// </summary>
    public string MethodId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the field or property being accessed
    /// </summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>
    /// Type of the field or property
    /// </summary>
    public string FieldType { get; set; } = string.Empty;

    /// <summary>
    /// Type of access (read, write, both)
    /// </summary>
    public FieldAccessType AccessType { get; set; } = FieldAccessType.Read;

    /// <summary>
    /// Whether this is a field or property
    /// </summary>
    public FieldKind FieldKind { get; set; } = FieldKind.Field;

    /// <summary>
    /// Whether the field/property is static
    /// </summary>
    public bool IsStatic { get; set; }

    /// <summary>
    /// Number of times this field is accessed from the method
    /// </summary>
    public int AccessCount { get; set; } = 1;

    /// <summary>
    /// Source location where the access occurs
    /// </summary>
    public CSharpLocationInfo? AccessLocation { get; set; }

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Types of field/property access
/// </summary>
public enum FieldAccessType
{
    /// <summary>
    /// Reading from the field/property
    /// </summary>
    Read,

    /// <summary>
    /// Writing to the field/property
    /// </summary>
    Write,

    /// <summary>
    /// Both reading and writing
    /// </summary>
    ReadWrite
}

/// <summary>
/// Kind of field being accessed
/// </summary>
public enum FieldKind
{
    /// <summary>
    /// Regular field
    /// </summary>
    Field,

    /// <summary>
    /// Property with getter/setter
    /// </summary>
    Property,

    /// <summary>
    /// Auto-implemented property
    /// </summary>
    AutoProperty,

    /// <summary>
    /// Constant field
    /// </summary>
    Constant,

    /// <summary>
    /// Readonly field
    /// </summary>
    ReadOnly
}