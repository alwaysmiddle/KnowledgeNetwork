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

/// <summary>
/// Types of interface implementation
/// </summary>
public enum InterfaceImplementationType
{
    /// <summary>
    /// Direct implementation by the class
    /// </summary>
    Direct,

    /// <summary>
    /// Inherited from base class
    /// </summary>
    Inherited,

    /// <summary>
    /// Both direct and inherited
    /// </summary>
    DirectAndInherited
}

/// <summary>
/// Information about an interface method implementation
/// </summary>
public class InterfaceMethodImplementation
{
    /// <summary>
    /// Name of the interface method
    /// </summary>
    public string MethodName { get; set; } = string.Empty;

    /// <summary>
    /// Signature of the interface method
    /// </summary>
    public string MethodSignature { get; set; } = string.Empty;

    /// <summary>
    /// Name of the implementing method in the class
    /// </summary>
    public string ImplementingMethodName { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is an explicit implementation
    /// </summary>
    public bool IsExplicit { get; set; }

    /// <summary>
    /// Whether the implementation is in the current class or inherited
    /// </summary>
    public bool IsInherited { get; set; }

    /// <summary>
    /// Source location of the implementation
    /// </summary>
    public CSharpLocationInfo? ImplementationLocation { get; set; }
}