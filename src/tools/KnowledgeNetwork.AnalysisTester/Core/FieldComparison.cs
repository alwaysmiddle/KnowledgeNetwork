namespace KnowledgeNetwork.AnalysisTester.Core;

/// <summary>
/// Comparison result for a single field/property
/// </summary>
public class FieldComparison
{
    public string FieldName { get; set; } = string.Empty;
    public object? ExpectedValue { get; set; }
    public object? ActualValue { get; set; }
    public bool IsMatch { get; set; }
    
    public string ExpectedString => ExpectedValue?.ToString() ?? "null";
    public string ActualString => ActualValue?.ToString() ?? "null";
}