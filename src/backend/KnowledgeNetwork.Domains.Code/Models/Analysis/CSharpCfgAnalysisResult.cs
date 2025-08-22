using KnowledgeNetwork.Core.Models.Core;

namespace KnowledgeNetwork.Domains.Code.Models.Analysis;

/// <summary>
/// Result of C# Control Flow Graph analysis
/// </summary>
public class CSharpCfgAnalysisResult
{
    /// <summary>
    /// Whether the analysis was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Language identifier for the analysis
    /// </summary>
    public string LanguageId { get; set; } = "csharp";

    /// <summary>
    /// List of errors encountered during analysis
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// All knowledge nodes extracted from CFG analysis (methods, basic blocks, operations)
    /// </summary>
    public List<KnowledgeNode> Nodes { get; set; } = new();

    /// <summary>
    /// All knowledge edges extracted from CFG analysis (control flow edges)
    /// </summary>
    public List<KnowledgeEdge> Edges { get; set; } = new();

    /// <summary>
    /// Duration of the analysis
    /// </summary>
    public TimeSpan? Duration { get; set; }

    /// <summary>
    /// Analysis metadata (metrics, statistics, configuration)
    /// </summary>
    public Dictionary<string, object?> Metadata { get; set; } = new();
}