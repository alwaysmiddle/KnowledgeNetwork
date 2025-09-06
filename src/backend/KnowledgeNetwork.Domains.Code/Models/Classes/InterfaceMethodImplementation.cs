using KnowledgeNetwork.Domains.Code.Models.Common;

namespace KnowledgeNetwork.Domains.Code.Models.Classes;

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