namespace KnowledgeNetwork.AnalysisTester.Core;

/// <summary>
/// Complete result of a test execution, including timing and comparison data
/// </summary>
public class TestExecutionResult
{
    public CommandInfo Command { get; set; } = new();
    public TestScenario Scenario { get; set; } = new();
    public TestResult Result { get; set; } = new();
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan TotalDuration => EndTime - StartTime;
    public ResultComparison? Comparison { get; set; }
    public Exception? Exception { get; set; }
    
    public bool HasComparison => Comparison != null;
    public bool OverallSuccess => Result.Success && (Comparison?.OverallMatch ?? true);
}