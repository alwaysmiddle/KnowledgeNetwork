using Microsoft.CodeAnalysis;

namespace KnowledgeNetwork.Domains.Code.Models;

/// <summary>
/// Represents a basic block in a control flow graph.
/// A basic block is a sequence of operations with single entry and exit points.
/// </summary>
public class BasicBlock
{
    /// <summary>
    /// Unique identifier for this basic block
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Ordinal position in the control flow graph
    /// </summary>
    public int Ordinal { get; set; }

    /// <summary>
    /// Type of basic block (Entry, Exit, Regular, etc.)
    /// </summary>
    public BasicBlockKind Kind { get; set; }

    /// <summary>
    /// Operations contained in this basic block
    /// </summary>
    public List<OperationInfo> Operations { get; set; } = new();

    /// <summary>
    /// IDs of basic blocks that can execute before this one
    /// </summary>
    public List<int> Predecessors { get; set; } = new();

    /// <summary>
    /// IDs of basic blocks that can execute after this one
    /// </summary>
    public List<int> Successors { get; set; } = new();

    /// <summary>
    /// Whether this block is reachable from the entry block
    /// </summary>
    public bool IsReachable { get; set; }

    /// <summary>
    /// Conditional branch information if this block ends with a condition
    /// </summary>
    public BranchInfo? BranchInfo { get; set; }

    /// <summary>
    /// Source location information for this block
    /// </summary>
    public LocationInfo? Location { get; set; }

    /// <summary>
    /// Additional metadata for visualization
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Types of basic blocks in control flow
/// </summary>
public enum BasicBlockKind
{
    /// <summary>
    /// Entry point of the method
    /// </summary>
    Entry,

    /// <summary>
    /// Exit point of the method
    /// </summary>
    Exit,

    /// <summary>
    /// Regular block with operations
    /// </summary>
    Block,

    /// <summary>
    /// Block that handles exceptions
    /// </summary>
    ExceptionHandler
}

/// <summary>
/// Information about an operation within a basic block
/// </summary>
public class OperationInfo
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
    public LocationInfo? Location { get; set; }

    /// <summary>
    /// Whether this operation might throw an exception
    /// </summary>
    public bool MayThrow { get; set; }
}

/// <summary>
/// Information about conditional branches
/// </summary>
public class BranchInfo
{
    /// <summary>
    /// Condition expression for the branch
    /// </summary>
    public string Condition { get; set; } = string.Empty;

    /// <summary>
    /// Block to execute if condition is true
    /// </summary>
    public int? TrueTarget { get; set; }

    /// <summary>
    /// Block to execute if condition is false
    /// </summary>
    public int? FalseTarget { get; set; }

    /// <summary>
    /// Type of branch (if, while, for, switch, etc.)
    /// </summary>
    public BranchType BranchType { get; set; }
}

/// <summary>
/// Types of control flow branches
/// </summary>
public enum BranchType
{
    /// <summary>
    /// Simple if statement
    /// </summary>
    Conditional,

    /// <summary>
    /// Loop condition (while, for)
    /// </summary>
    Loop,

    /// <summary>
    /// Switch statement
    /// </summary>
    Switch,

    /// <summary>
    /// Exception handling
    /// </summary>
    Exception,

    /// <summary>
    /// Unconditional jump
    /// </summary>
    Unconditional
}

/// <summary>
/// Source location information
/// </summary>
public class LocationInfo
{
    /// <summary>
    /// Starting line number (1-based)
    /// </summary>
    public int StartLine { get; set; }

    /// <summary>
    /// Ending line number (1-based)
    /// </summary>
    public int EndLine { get; set; }

    /// <summary>
    /// Starting column (0-based)
    /// </summary>
    public int StartColumn { get; set; }

    /// <summary>
    /// Ending column (0-based)
    /// </summary>
    public int EndColumn { get; set; }

    /// <summary>
    /// File path (if available)
    /// </summary>
    public string? FilePath { get; set; }
}