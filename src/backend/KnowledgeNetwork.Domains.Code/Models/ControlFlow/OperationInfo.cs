using KnowledgeNetwork.Domains.Code.Models.Common;

namespace KnowledgeNetwork.Domains.Code.Models.ControlFlow;

/// <summary>
/// Information about an operation within a basic block
/// </summary>
public class KnOperationInfo
{
    /// <summary>
    /// Type of operation (assignment, method call, etc.)
    /// </summary>
    public string OperationKind { get; set; } = string.Empty;

    /// <summary>
    /// Syntax representation of the operation
    /// </summary>
    public string Syntax { get; set; } = string.Empty;

    /// <summary>
    /// Simplified human-readable description
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Source location of this operation
    /// </summary>
    public KnLocationInfo? Location { get; set; }

    /// <summary>
    /// Whether this operation might throw an exception
    /// </summary>
    public bool MayThrow { get; set; }
}