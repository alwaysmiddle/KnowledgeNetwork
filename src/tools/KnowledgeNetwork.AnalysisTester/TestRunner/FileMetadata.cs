namespace KnowledgeNetwork.AnalysisTester.TestRunner;

/// <summary>
/// File metadata information
/// </summary>
public class FileMetadata
{
    public string FileName { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime LastModified { get; set; }
    public string Language { get; set; } = string.Empty;
}