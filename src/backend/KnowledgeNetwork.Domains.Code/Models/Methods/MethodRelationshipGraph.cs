using KnowledgeNetwork.Domains.Code.Models.Common;

namespace KnowledgeNetwork.Domains.Code.Models.Methods;

/// <summary>
/// Represents method-level relationships within a single class.
/// This graph shows how methods call each other, access fields/properties, and interact.
/// </summary>
public class MethodRelationshipGraph
{
    /// <summary>
    /// Unique identifier for this method relationship graph
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Name of the class this graph represents
    /// </summary>
    public string ClassName { get; set; } = string.Empty;

    /// <summary>
    /// Fully qualified name of the containing type
    /// </summary>
    public string FullyQualifiedTypeName { get; set; } = string.Empty;

    /// <summary>
    /// Namespace containing this class
    /// </summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// All methods in this class
    /// </summary>
    public List<MethodNode> Methods { get; set; } = new();

    /// <summary>
    /// Method call relationships (method A calls method B)
    /// </summary>
    public List<MethodCallEdge> CallEdges { get; set; } = new();

    /// <summary>
    /// Field access relationships (method accesses field/property)
    /// </summary>
    public List<FieldAccessEdge> FieldAccesses { get; set; } = new();

    /// <summary>
    /// Constructor chain relationships
    /// </summary>
    public List<ConstructorChainEdge> ConstructorChains { get; set; } = new();

    /// <summary>
    /// Source location information for this class
    /// </summary>
    public CSharpLocationInfo? Location { get; set; }

    /// <summary>
    /// Additional metadata for analysis and visualization
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Get a method by its signature
    /// </summary>
    /// <param name="signature">Method signature</param>
    /// <returns>Method node or null if not found</returns>
    public MethodNode? GetMethod(string signature)
    {
        return Methods.FirstOrDefault(m => m.Signature.Equals(signature, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Get all methods that call the specified method
    /// </summary>
    /// <param name="targetMethodId">ID of the method being called</param>
    /// <returns>List of methods that call the target method</returns>
    public List<MethodNode> GetCallers(string targetMethodId)
    {
        var callerIds = CallEdges
            .Where(e => e.TargetMethodId == targetMethodId)
            .Select(e => e.SourceMethodId)
            .ToHashSet();

        return Methods.Where(m => callerIds.Contains(m.Id)).ToList();
    }

    /// <summary>
    /// Get all methods called by the specified method
    /// </summary>
    /// <param name="sourceMethodId">ID of the calling method</param>
    /// <returns>List of methods called by the source method</returns>
    public List<MethodNode> GetCallees(string sourceMethodId)
    {
        var calleeIds = CallEdges
            .Where(e => e.SourceMethodId == sourceMethodId)
            .Select(e => e.TargetMethodId)
            .ToHashSet();

        return Methods.Where(m => calleeIds.Contains(m.Id)).ToList();
    }

    /// <summary>
    /// Get all fields/properties accessed by the specified method
    /// </summary>
    /// <param name="methodId">ID of the method</param>
    /// <returns>List of field access edges for the method</returns>
    public List<FieldAccessEdge> GetFieldAccesses(string methodId)
    {
        return FieldAccesses.Where(f => f.MethodId == methodId).ToList();
    }
}