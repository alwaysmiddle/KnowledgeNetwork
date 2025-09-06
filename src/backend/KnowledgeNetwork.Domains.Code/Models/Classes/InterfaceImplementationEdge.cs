using KnowledgeNetwork.Domains.Code.Models.Classes.ClassEnums;
using KnowledgeNetwork.Domains.Code.Models.Common;

namespace KnowledgeNetwork.Domains.Code.Models.Classes;

/// <summary>
/// Represents an interface implementation relationship
/// </summary>
public class InterfaceImplementationEdge
{
    /// <summary>
    /// Unique identifier for this edge
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// ID of the class implementing the interface
    /// </summary>
    public string ClassId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the interface being implemented
    /// </summary>
    public string InterfaceName { get; set; } = string.Empty;

    /// <summary>
    /// Full qualified name of the interface
    /// </summary>
    public string InterfaceFullName { get; set; } = string.Empty;

    /// <summary>
    /// Type of interface implementation
    /// </summary>
    public InterfaceImplementationType ImplementationType { get; set; } = InterfaceImplementationType.Direct;

    /// <summary>
    /// Whether this is an explicit interface implementation
    /// </summary>
    public bool IsExplicitImplementation { get; set; }

    /// <summary>
    /// Whether the interface is generic
    /// </summary>
    public bool IsGenericInterface { get; set; }

    /// <summary>
    /// Generic type arguments if the interface is generic
    /// </summary>
    public List<string> GenericTypeArguments { get; set; } = new();

    /// <summary>
    /// Whether the interface crosses assembly boundaries
    /// </summary>
    public bool IsCrossAssembly { get; set; }

    /// <summary>
    /// Whether the interface crosses namespace boundaries
    /// </summary>
    public bool IsCrossNamespace { get; set; }

    /// <summary>
    /// Methods from this interface that are implemented
    /// </summary>
    public List<InterfaceMethodImplementation> ImplementedMethods { get; set; } = new();

    /// <summary>
    /// Source location where the interface implementation is declared
    /// </summary>
    public CSharpLocationInfo? ImplementationLocation { get; set; }

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}