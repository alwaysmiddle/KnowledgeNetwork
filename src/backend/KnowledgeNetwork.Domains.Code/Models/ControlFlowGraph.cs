using KnowledgeNetwork.Domains.Code.Models.Common;
using KnowledgeNetwork.Domains.Code.Models.Enums;

namespace KnowledgeNetwork.Domains.Code.Models;

/// <summary>
/// Represents a complete control flow graph for a method or code block
/// </summary>
public class KnControlFlowGraph
{
    /// <summary>
    /// Unique identifier for this CFG
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Name of the method this CFG represents
    /// </summary>
    public string MethodName { get; set; } = string.Empty;

    /// <summary>
    /// Fully qualified name of the containing type
    /// </summary>
    public string TypeName { get; set; } = string.Empty;

    /// <summary>
    /// All basic blocks in this control flow graph
    /// </summary>
    public List<KnBasicBlock> BasicBlocks { get; set; } = new();

    /// <summary>
    /// Entry block (where execution begins)
    /// </summary>
    public KnBasicBlock? EntryBlock { get; set; }

    /// <summary>
    /// Exit block (where execution ends)
    /// </summary>
    public KnBasicBlock? ExitBlock { get; set; }

    /// <summary>
    /// Control flow edges between basic blocks
    /// </summary>
    public List<KnControlFlowEdge> Edges { get; set; } = new();

    /// <summary>
    /// Complexity metrics for this CFG
    /// </summary>
    public KnComplexityMetrics Metrics { get; set; } = new();

    /// <summary>
    /// Source location of the method
    /// </summary>
    public KnLocationInfo? Location { get; set; }

    /// <summary>
    /// Additional metadata for analysis and visualization
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Get a basic block by its ID
    /// </summary>
    /// <param name="id">Block ID</param>
    /// <returns>Basic block or null if not found</returns>
    public KnBasicBlock? GetBlock(int id)
    {
        return BasicBlocks.FirstOrDefault(b => b.Id == id);
    }

    /// <summary>
    /// Get all blocks that are reachable from the entry block
    /// </summary>
    /// <returns>List of reachable blocks</returns>
    public List<KnBasicBlock> GetReachableBlocks()
    {
        return BasicBlocks.Where(b => b.IsReachable).ToList();
    }

    /// <summary>
    /// Get all blocks that contain loops (have back-edges)
    /// </summary>
    /// <returns>List of blocks involved in loops</returns>
    public List<KnBasicBlock> GetLoopBlocks()
    {
        var loopBlocks = new HashSet<int>();
        
        foreach (var edge in Edges.Where(e => e.Kind == KnEdgeKind.BackEdge))
        {
            loopBlocks.Add(edge.Source);
            loopBlocks.Add(edge.Target);
        }
        
        return BasicBlocks.Where(b => loopBlocks.Contains(b.Id)).ToList();
    }

    /// <summary>
    /// Validate the structural integrity of the CFG
    /// </summary>
    /// <returns>List of validation errors</returns>
    public List<string> Validate()
    {
        var errors = new List<string>();

        // Check for entry block
        if (EntryBlock == null)
        {
            errors.Add("CFG must have an entry block");
        }

        // Check for exit block
        if (ExitBlock == null)
        {
            errors.Add("CFG must have an exit block");
        }

        // Validate edges reference valid blocks
        foreach (var edge in Edges)
        {
            if (GetBlock(edge.Source) == null)
            {
                errors.Add($"Edge references invalid source block: {edge.Source}");
            }
            if (GetBlock(edge.Target) == null)
            {
                errors.Add($"Edge references invalid target block: {edge.Target}");
            }
        }

        // Check for orphaned blocks (except entry)
        foreach (var block in BasicBlocks.Where(b => b.Kind != KnBasicBlockKind.Entry))
        {
            if (!Edges.Any(e => e.Target == block.Id))
            {
                errors.Add($"Block {block.Id} has no incoming edges");
            }
        }

        return errors;
    }
}