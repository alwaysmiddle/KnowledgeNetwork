namespace KnowledgeNetwork.AnalysisTester.Core;

/// <summary>
/// Information about a test command for display and navigation purposes
/// </summary>
public class CommandInfo
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ComponentPath { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ScenariosPath { get; set; } = string.Empty;
}