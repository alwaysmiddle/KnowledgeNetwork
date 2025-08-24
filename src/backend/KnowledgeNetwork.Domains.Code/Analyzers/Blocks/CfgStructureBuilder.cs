using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.Extensions.Logging;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Blocks;

/// <summary>
/// Implementation of ICfgStructureBuilder that creates Roslyn ControlFlowGraph structures.
/// This service provides a clean wrapper around Roslyn's ControlFlowGraph.Create API
/// with consistent error handling and logging.
/// </summary>
public class CfgStructureBuilder(ILogger<CfgStructureBuilder> logger) : ICfgStructureBuilder
{
    private readonly ILogger<CfgStructureBuilder> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Build a ControlFlowGraph from the provided IBlockOperation
    /// </summary>
    public async Task<ControlFlowGraph?> BuildStructureAsync(IBlockOperation blockOperation, string memberName)
    {
        await Task.CompletedTask; // Maintain async signature

        if (blockOperation == null)
        {
            _logger.LogWarning("Cannot build CFG: IBlockOperation is null for member {MemberName}", memberName);
            return null;
        }

        try
        {
            _logger.LogDebug("Building CFG structure for member {MemberName}", memberName);

            // Create the control flow graph using Roslyn's API
            var cfg = ControlFlowGraph.Create(blockOperation);
            
            if (cfg == null)
            {
                _logger.LogWarning("ControlFlowGraph.Create returned null for member {MemberName}", memberName);
                return null;
            }

            _logger.LogDebug("Successfully created CFG with {BlockCount} blocks for member {MemberName}", 
                cfg.Blocks.Length, memberName);

            return cfg;
        }
        catch (ArgumentException ex)
        {
            // This can happen if the IBlockOperation is invalid for CFG creation
            _logger.LogError(ex, "Invalid IBlockOperation provided for CFG creation in member {MemberName}", memberName);
            return null;
        }
        catch (InvalidOperationException ex)
        {
            // This can happen with malformed or unsupported operation structures
            _logger.LogError(ex, "Invalid operation structure for CFG creation in member {MemberName}", memberName);
            return null;
        }
        catch (Exception ex)
        {
            // Catch any other unexpected errors from Roslyn
            _logger.LogError(ex, "Unexpected error during CFG creation for member {MemberName}", memberName);
            return null;
        }
    }
}