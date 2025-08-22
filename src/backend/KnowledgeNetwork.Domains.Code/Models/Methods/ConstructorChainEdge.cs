using KnowledgeNetwork.Domains.Code.Models.Common;

namespace KnowledgeNetwork.Domains.Code.Models.Methods;

/// <summary>
/// Represents a constructor chaining relationship (this(...) or base(...))
/// </summary>
public class ConstructorChainEdge
{
    /// <summary>
    /// Unique identifier for this edge
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// ID of the constructor making the call (source)
    /// </summary>
    public string SourceConstructorId { get; set; } = string.Empty;

    /// <summary>
    /// ID of the constructor being called (target)
    /// </summary>
    public string TargetConstructorId { get; set; } = string.Empty;

    /// <summary>
    /// Type of constructor chain
    /// </summary>
    public ConstructorChainType ChainType { get; set; } = ConstructorChainType.This;

    /// <summary>
    /// Arguments passed to the chained constructor
    /// </summary>
    public List<string> Arguments { get; set; } = new();

    /// <summary>
    /// Source location where the chain occurs
    /// </summary>
    public CSharpLocationInfo? ChainLocation { get; set; }

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Types of constructor chaining
/// </summary>
public enum ConstructorChainType
{
    /// <summary>
    /// Calls another constructor in the same class (this(...))
    /// </summary>
    This,

    /// <summary>
    /// Calls a constructor in the base class (base(...))
    /// </summary>
    Base
}