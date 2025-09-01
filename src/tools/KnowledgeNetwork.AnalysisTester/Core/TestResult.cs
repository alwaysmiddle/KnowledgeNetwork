namespace KnowledgeNetwork.AnalysisTester.Core;

/// <summary>
/// Result of executing a test scenario
/// </summary>
public class TestResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
    public DateTime ExecutionTime { get; set; }
    public TimeSpan Duration { get; set; }
    public string ComponentName { get; set; } = string.Empty;
    public Dictionary<string, object> Metrics { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}