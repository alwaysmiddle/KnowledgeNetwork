using KnowledgeNetwork.Domains.Code.Models;
using KnowledgeNetwork.Domains.Code.Models.Analysis;

namespace KnowledgeNetwork.AnalysisTester.Models;

/// <summary>
/// Result of running analysis on a test file
/// </summary>
public class TestResult
{
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public CSharpCodeAnalysisResult? AnalysisResult { get; set; }
    public List<CSharpControlFlowGraph> ControlFlowGraphs { get; set; } = new();
    public TimeSpan Duration { get; set; }
    public bool Success { get; set; }
    public DateTime AnalyzedAt { get; set; }
    public List<string> Errors { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();

    // Computed properties for easy access
    public int ClassCount => AnalysisResult?.Classes.Count ?? 0;
    public int MethodCount => AnalysisResult?.Methods.Count ?? 0;
    public int PropertyCount => AnalysisResult?.Properties.Count ?? 0;
    public int UsingStatementCount => AnalysisResult?.UsingStatements.Count ?? 0;
    public int CFGBlockCount => ControlFlowGraphs.Sum(cfg => cfg.BasicBlocks.Count);
    public int CFGEdgeCount => ControlFlowGraphs.Sum(cfg => cfg.Edges.Count);
    public double AvgCyclomaticComplexity => ControlFlowGraphs.Any() 
        ? ControlFlowGraphs.Average(cfg => cfg.Metrics.CyclomaticComplexity) 
        : 0;
    public int TotalDecisionPoints => ControlFlowGraphs.Sum(cfg => cfg.Metrics.DecisionPoints);
    public int TotalLoops => ControlFlowGraphs.Sum(cfg => cfg.Metrics.LoopCount);
}