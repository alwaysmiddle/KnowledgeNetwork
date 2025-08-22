using KnowledgeNetwork.Domains.Code.Models.Common;

namespace KnowledgeNetwork.Domains.Code.Models.Methods;

/// <summary>
/// Represents a method call relationship between two methods
/// </summary>
public class MethodCallEdge
{
    /// <summary>
    /// Unique identifier for this edge
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// ID of the method making the call (source)
    /// </summary>
    public string SourceMethodId { get; set; } = string.Empty;

    /// <summary>
    /// ID of the method being called (target)
    /// </summary>
    public string TargetMethodId { get; set; } = string.Empty;

    /// <summary>
    /// Type of method call
    /// </summary>
    public MethodCallType CallType { get; set; } = MethodCallType.Direct;

    /// <summary>
    /// Number of times this method is called from the source method
    /// </summary>
    public int CallCount { get; set; } = 1;

    /// <summary>
    /// Whether the call is in a loop or conditional block
    /// </summary>
    public bool IsConditional { get; set; }

    /// <summary>
    /// Source location where the call is made
    /// </summary>
    public CSharpLocationInfo? CallLocation { get; set; }

    /// <summary>
    /// Arguments passed to the method call (for analysis)
    /// </summary>
    public List<string> Arguments { get; set; } = new();

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Types of method calls
/// </summary>
public enum MethodCallType
{
    /// <summary>
    /// Direct method call (method())
    /// </summary>
    Direct,

    /// <summary>
    /// Virtual method call (override/virtual)
    /// </summary>
    Virtual,

    /// <summary>
    /// Interface method call
    /// </summary>
    Interface,

    /// <summary>
    /// Static method call
    /// </summary>
    Static,

    /// <summary>
    /// Constructor call
    /// </summary>
    Constructor,

    /// <summary>
    /// Base class method call (base.method())
    /// </summary>
    Base,

    /// <summary>
    /// This method call (this.method())
    /// </summary>
    This,

    /// <summary>
    /// Delegate or lambda invocation
    /// </summary>
    Delegate
}