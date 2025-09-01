namespace KnowledgeNetwork.AnalysisTester.TestRunner;

/// <summary>
/// Directory analysis statistics
/// </summary>
public class DirectoryStatistics
{
    public int TotalFiles { get; set; }
    public long TotalSizeBytes { get; set; }
    public int TotalLines { get; set; }
    public Dictionary<string, int> FilesByLanguage { get; set; } = new();
    public int SupportedFiles { get; set; }
    
    public string TotalSizeFormatted => FormatBytes(TotalSizeBytes);
    
    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}