using Shouldly;
using KnowledgeNetwork.Domains.Code.Models.Blocks;
using KnowledgeNetwork.Domains.Code.Models.Enums;
using KnowledgeNetwork.Domains.Code.Models.Common;

namespace KnowledgeNetwork.Tests.Shared;

/// <summary>
/// Utility for validating control flow graph analysis results.
/// Provides common assertion patterns and validation logic for CFG testing.
/// </summary>
public static class AnalysisResultValidator
{
    /// <summary>
    /// Validate basic CFG structure requirements
    /// </summary>
    /// <param name="cfg">Control flow graph to validate</param>
    public static void ValidateBasicCfgStructure(MethodBlockGraph cfg)
    {
        cfg.ShouldNotBeNull("CFG should not be null");
        cfg.BasicBlocks.ShouldNotBeEmpty("CFG should have at least one basic block");
        cfg.EntryBlock.ShouldNotBeNull("CFG should have an entry block");
        cfg.ExitBlock.ShouldNotBeNull("CFG should have an exit block");
        cfg.Metrics.ShouldNotBeNull("CFG should have complexity metrics");
    }

    /// <summary>
    /// Validate entry and exit block properties
    /// </summary>
    /// <param name="cfg">Control flow graph to validate</param>
    public static void ValidateEntryExitBlocks(MethodBlockGraph cfg)
    {
        // Entry block validations
        cfg.EntryBlock.Kind.ShouldBe(CSharpBasicBlockKind.Entry, "Entry block should have Entry kind");
        cfg.EntryBlock.Predecessors.ShouldBeEmpty("Entry block should have no predecessors");
        cfg.EntryBlock.IsReachable.ShouldBeTrue("Entry block should always be reachable");

        // Exit block validations  
        cfg.ExitBlock.Kind.ShouldBe(CSharpBasicBlockKind.Exit, "Exit block should have Exit kind");
        cfg.ExitBlock.Successors.ShouldBeEmpty("Exit block should have no successors");
        cfg.ExitBlock.IsReachable.ShouldBeTrue("Exit block should be reachable in valid CFG");
    }

    /// <summary>
    /// Validate complexity metrics for expected values
    /// </summary>
    /// <param name="metrics">Complexity metrics to validate</param>
    /// <param name="expectedCyclomaticComplexity">Expected cyclomatic complexity</param>
    /// <param name="expectedDecisionPoints">Expected number of decision points</param>
    /// <param name="expectedLoopCount">Expected number of loops</param>
    public static void ValidateComplexityMetrics(
        CSharpComplexityMetrics metrics,
        int expectedCyclomaticComplexity,
        int? expectedDecisionPoints = null,
        int? expectedLoopCount = null)
    {
        metrics.ShouldNotBeNull("Metrics should not be null");
        metrics.CyclomaticComplexity.ShouldBe(expectedCyclomaticComplexity, 
            $"Cyclomatic complexity should be {expectedCyclomaticComplexity}");

        if (expectedDecisionPoints.HasValue)
        {
            metrics.DecisionPoints.ShouldBe(expectedDecisionPoints.Value,
                $"Decision points should be {expectedDecisionPoints.Value}");
        }

        if (expectedLoopCount.HasValue)
        {
            metrics.LoopCount.ShouldBe(expectedLoopCount.Value,
                $"Loop count should be {expectedLoopCount.Value}");
        }

        // Basic sanity checks
        metrics.BlockCount.ShouldBeGreaterThan(0, "Should have at least one block");
        metrics.EdgeCount.ShouldBeGreaterThanOrEqualTo(0, "Edge count should be non-negative");
        metrics.CyclomaticComplexity.ShouldBeGreaterThan(0, "Cyclomatic complexity should be positive");
    }

    /// <summary>
    /// Validate that CFG has expected number of basic blocks
    /// </summary>
    /// <param name="cfg">Control flow graph to validate</param>
    /// <param name="expectedBlockCount">Expected number of basic blocks</param>
    public static void ValidateBlockCount(MethodBlockGraph cfg, int expectedBlockCount)
    {
        cfg.BasicBlocks.Count.ShouldBe(expectedBlockCount, 
            $"CFG should have exactly {expectedBlockCount} basic blocks");
        cfg.Metrics.BlockCount.ShouldBe(expectedBlockCount,
            "Metrics block count should match actual block count");
    }

    /// <summary>
    /// Validate that CFG has expected number of edges
    /// </summary>
    /// <param name="cfg">Control flow graph to validate</param>
    /// <param name="expectedEdgeCount">Expected number of edges</param>
    public static void ValidateEdgeCount(MethodBlockGraph cfg, int expectedEdgeCount)
    {
        cfg.Edges.Count.ShouldBe(expectedEdgeCount, 
            $"CFG should have exactly {expectedEdgeCount} edges");
        cfg.Metrics.EdgeCount.ShouldBe(expectedEdgeCount,
            "Metrics edge count should match actual edge count");
    }

    /// <summary>
    /// Validate that specific edge types exist in the CFG
    /// </summary>
    /// <param name="cfg">Control flow graph to validate</param>
    /// <param name="expectedEdgeKinds">Expected edge kinds that should be present</param>
    public static void ValidateEdgeKinds(MethodBlockGraph cfg, params CSharpEdgeKind[] expectedEdgeKinds)
    {
        foreach (var expectedKind in expectedEdgeKinds)
        {
            cfg.Edges.ShouldContain(e => e.Kind == expectedKind, 
                $"CFG should contain at least one {expectedKind} edge");
        }
    }

    /// <summary>
    /// Validate that back edges are detected for loops
    /// </summary>
    /// <param name="cfg">Control flow graph to validate</param>
    /// <param name="expectedBackEdgeCount">Expected number of back edges</param>
    public static void ValidateBackEdges(MethodBlockGraph cfg, int expectedBackEdgeCount)
    {
        var backEdgeCount = cfg.Edges.Count(e => e.Kind == CSharpEdgeKind.BackEdge);
        backEdgeCount.ShouldBe(expectedBackEdgeCount,
            $"CFG should have exactly {expectedBackEdgeCount} back edges");
    }

    /// <summary>
    /// Validate that conditional edges are properly structured
    /// </summary>
    /// <param name="cfg">Control flow graph to validate</param>
    /// <param name="expectedConditionalEdgeCount">Expected number of conditional edges</param>
    public static void ValidateConditionalEdges(MethodBlockGraph cfg, int expectedConditionalEdgeCount)
    {
        var conditionalEdges = cfg.Edges.Where(e => 
            e.Kind == CSharpEdgeKind.ConditionalTrue || 
            e.Kind == CSharpEdgeKind.ConditionalFalse).ToList();

        conditionalEdges.Count.ShouldBe(expectedConditionalEdgeCount,
            $"CFG should have exactly {expectedConditionalEdgeCount} conditional edges");

        // Validate conditional edges have proper conditions
        foreach (var edge in conditionalEdges)
        {
            if (edge.Kind == CSharpEdgeKind.ConditionalTrue || edge.Kind == CSharpEdgeKind.ConditionalFalse)
            {
                edge.Condition.ShouldNotBeNull($"Conditional edge should have condition information");
            }
        }
    }

    /// <summary>
    /// Validate CFG structure integrity (blocks properly connected)
    /// </summary>
    /// <param name="cfg">Control flow graph to validate</param>
    public static void ValidateCfgIntegrity(MethodBlockGraph cfg)
    {
        // Validate that all blocks are properly connected
        foreach (var block in cfg.BasicBlocks)
        {
            // Check successors
            foreach (var successorId in block.Successors)
            {
                var successor = cfg.GetBlock(successorId);
                successor.ShouldNotBeNull($"Block {block.Id} references non-existent successor {successorId}");
                successor.Predecessors.ShouldContain(block.Id,
                    $"Successor block {successorId} should reference block {block.Id} as predecessor");
            }

            // Check predecessors
            foreach (var predecessorId in block.Predecessors)
            {
                var predecessor = cfg.GetBlock(predecessorId);
                predecessor.ShouldNotBeNull($"Block {block.Id} references non-existent predecessor {predecessorId}");
                predecessor.Successors.ShouldContain(block.Id,
                    $"Predecessor block {predecessorId} should reference block {block.Id} as successor");
            }
        }

        // Validate that edges match block relationships
        foreach (var edge in cfg.Edges)
        {
            var sourceBlock = cfg.GetBlock(edge.Source);
            var targetBlock = cfg.GetBlock(edge.Target);

            sourceBlock.ShouldNotBeNull($"Edge references non-existent source block {edge.Source}");
            targetBlock.ShouldNotBeNull($"Edge references non-existent target block {edge.Target}");

            sourceBlock.Successors.ShouldContain(edge.Target,
                $"Source block {edge.Source} should have target {edge.Target} as successor");
            targetBlock.Predecessors.ShouldContain(edge.Source,
                $"Target block {edge.Target} should have source {edge.Source} as predecessor");
        }
    }

    /// <summary>
    /// Validate that a CFG represents a simple linear method (no branching or loops)
    /// </summary>
    /// <param name="cfg">Control flow graph to validate</param>
    public static void ValidateLinearMethod(MethodBlockGraph cfg)
    {
        ValidateBasicCfgStructure(cfg);
        ValidateEntryExitBlocks(cfg);
        
        // Linear method should have minimal complexity
        ValidateComplexityMetrics(cfg.Metrics, 1, 0, 0);
        
        // Should have no back edges or conditional edges
        ValidateBackEdges(cfg, 0);
        ValidateConditionalEdges(cfg, 0);
        
        // Validate integrity
        ValidateCfgIntegrity(cfg);
    }

    /// <summary>
    /// Validate that operations exist in basic blocks
    /// </summary>
    /// <param name="cfg">Control flow graph to validate</param>
    /// <param name="shouldHaveOperations">Whether operations should be present</param>
    public static void ValidateOperations(MethodBlockGraph cfg, bool shouldHaveOperations = true)
    {
        if (shouldHaveOperations)
        {
            var totalOperations = cfg.BasicBlocks.Sum(b => b.Operations.Count);
            totalOperations.ShouldBeGreaterThan(0, "CFG should have at least one operation");
        }

        // Validate operation structure
        foreach (var block in cfg.BasicBlocks)
        {
            foreach (var operation in block.Operations)
            {
                operation.OperationKind.ShouldNotBeNullOrEmpty("Operation should have a kind");
                operation.Summary.ShouldNotBeNull("Operation should have a summary (can be empty)");
            }
        }
    }
}