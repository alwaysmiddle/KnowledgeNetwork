namespace KnowledgeNetwork.AnalysisTester.Core;

/// <summary>
/// Comparison between actual test results and expected outcomes
/// </summary>
public class ResultComparison
{
    public TestResult ActualResult { get; set; } = new();
    public Dictionary<string, object> ExpectedValues { get; set; } = new();
    public List<FieldComparison> FieldComparisons { get; set; } = new();
    public bool OverallMatch { get; set; }
    public DateTime ComparisonTime { get; set; }
    
    public int MatchingFields => FieldComparisons.Count(fc => fc.IsMatch);
    public int TotalFields => FieldComparisons.Count;
    public double MatchPercentage => TotalFields > 0 ? (double)MatchingFields / TotalFields * 100 : 0;
}