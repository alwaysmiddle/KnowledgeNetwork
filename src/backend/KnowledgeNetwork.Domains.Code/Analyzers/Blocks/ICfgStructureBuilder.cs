using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Blocks;

/// <summary>
/// Service responsible for building Roslyn ControlFlowGraph structures from IBlockOperation.
/// This service encapsulates the Roslyn CFG creation API and provides consistent error handling.
/// </summary>
public interface ICfgStructureBuilder
{
    /// <summary>
    /// Build a ControlFlowGraph from the provided IBlockOperation
    /// </summary>
    /// <param name="blockOperation">The block operation to analyze for control flow</param>
    /// <param name="memberName">Name of the member for logging purposes</param>
    /// <returns>ControlFlowGraph if creation succeeds, null otherwise</returns>
    Task<ControlFlowGraph?> BuildStructureAsync(IBlockOperation blockOperation, string memberName);
}