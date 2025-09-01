namespace KnowledgeNetwork.AnalysisTester.Core;

/// <summary>
/// Test scenario definition - describes what to test and how
/// </summary>
public class TestScenario
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Component { get; set; } = string.Empty;
    public Dictionary<string, object> Input { get; set; } = new();
    public Dictionary<string, object>? ExpectedOutput { get; set; }
    public List<string> ValidationRules { get; set; } = new();
    public Dictionary<string, object> Options { get; set; } = new();
}