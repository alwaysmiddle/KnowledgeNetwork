using KnowledgeNetwork.Domains.Code.Models.Common;

namespace KnowledgeNetwork.Domains.Code.Models.Classes;

/// <summary>
/// Represents a nested class relationship
/// </summary>
public class NestedClassEdge
{
    /// <summary>
    /// Unique identifier for this edge
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// ID of the containing (outer) class
    /// </summary>
    public string ContainerClassId { get; set; } = string.Empty;

    /// <summary>
    /// ID of the nested (inner) class
    /// </summary>
    public string NestedClassId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the nested class
    /// </summary>
    public string NestedClassName { get; set; } = string.Empty;

    /// <summary>
    /// Nesting level (1 = direct child, 2 = nested in nested, etc.)
    /// </summary>
    public int NestingLevel { get; set; } = 1;

    /// <summary>
    /// Visibility of the nested class
    /// </summary>
    public string NestedClassVisibility { get; set; } = string.Empty;

    /// <summary>
    /// Whether the nested class has access to the container's private members
    /// </summary>
    public bool HasPrivateAccess { get; set; } = true;

    /// <summary>
    /// Whether the nested class is static
    /// </summary>
    public bool IsStaticNested { get; set; }

    /// <summary>
    /// Type of the nested class (class, interface, struct, enum, etc.)
    /// </summary>
    public string NestedClassType { get; set; } = string.Empty;

    /// <summary>
    /// Source location where the nested class is declared
    /// </summary>
    public CSharpLocationInfo? NestingLocation { get; set; }

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}