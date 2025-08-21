using Spectre.Console;

namespace KnowledgeNetwork.AnalysisTester.TestRunner;

/// <summary>
/// Manages test file discovery and organization
/// </summary>
public class TestFileManager
{
    /// <summary>
    /// Discover test files matching patterns in a directory
    /// </summary>
    public List<string> DiscoverTestFiles(string directory, string[] patterns)
    {
        var files = new List<string>();

        if (!Directory.Exists(directory))
        {
            AnsiConsole.MarkupLine($"[red]Directory not found: {directory}[/]");
            return files;
        }

        foreach (var pattern in patterns)
        {
            try
            {
                var matchingFiles = Directory.GetFiles(directory, pattern, SearchOption.AllDirectories);
                files.AddRange(matchingFiles);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error searching for pattern '{pattern}': {ex.Message}[/]");
            }
        }

        return files.Distinct().OrderBy(f => f).ToList();
    }

    /// <summary>
    /// Load file content safely
    /// </summary>
    public async Task<string?> LoadFileContentAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                AnsiConsole.MarkupLine($"[red]File not found: {filePath}[/]");
                return null;
            }

            return await File.ReadAllTextAsync(filePath);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error reading file {filePath}: {ex.Message}[/]");
            return null;
        }
    }

    /// <summary>
    /// Get file metadata
    /// </summary>
    public FileMetadata GetFileMetadata(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        
        return new FileMetadata
        {
            FileName = fileInfo.Name,
            FullPath = fileInfo.FullName,
            Extension = fileInfo.Extension,
            SizeBytes = fileInfo.Length,
            LastModified = fileInfo.LastWriteTime,
            Language = DetectLanguageFromExtension(fileInfo.Extension)
        };
    }

    /// <summary>
    /// Organize files by detected language
    /// </summary>
    public Dictionary<string, List<string>> OrganizeByLanguage(List<string> files)
    {
        var organizedFiles = new Dictionary<string, List<string>>();

        foreach (var file in files)
        {
            var language = DetectLanguageFromExtension(Path.GetExtension(file));
            
            if (!organizedFiles.ContainsKey(language))
            {
                organizedFiles[language] = new List<string>();
            }
            
            organizedFiles[language].Add(file);
        }

        return organizedFiles;
    }

    /// <summary>
    /// Detect programming language from file extension
    /// </summary>
    public string DetectLanguageFromExtension(string extension)
    {
        return extension.ToLower() switch
        {
            ".cs" => "csharp",
            ".ts" => "typescript",
            ".js" => "javascript",
            ".tsx" => "typescript",
            ".jsx" => "javascript",
            ".py" => "python",
            ".java" => "java",
            ".cpp" or ".cc" or ".cxx" => "cpp",
            ".c" => "c",
            ".h" or ".hpp" => "c/cpp",
            ".go" => "go",
            ".rs" => "rust",
            ".php" => "php",
            ".rb" => "ruby",
            ".md" => "markdown",
            ".txt" => "text",
            _ => "unknown"
        };
    }

    /// <summary>
    /// Filter files by supported languages
    /// </summary>
    public List<string> FilterSupportedFiles(List<string> files, string[] supportedLanguages)
    {
        return files.Where(file =>
        {
            var language = DetectLanguageFromExtension(Path.GetExtension(file));
            return supportedLanguages.Contains(language);
        }).ToList();
    }

    /// <summary>
    /// Get file statistics for a directory
    /// </summary>
    public DirectoryStatistics GetDirectoryStatistics(string directoryPath, string[] patterns)
    {
        var files = DiscoverTestFiles(directoryPath, patterns);
        var organizedFiles = OrganizeByLanguage(files);
        
        var totalSize = 0L;
        var totalLines = 0;
        
        foreach (var file in files)
        {
            try
            {
                var fileInfo = new FileInfo(file);
                totalSize += fileInfo.Length;
                
                var lines = File.ReadAllLines(file);
                totalLines += lines.Length;
            }
            catch
            {
                // Skip files that can't be read
            }
        }

        return new DirectoryStatistics
        {
            TotalFiles = files.Count,
            TotalSizeBytes = totalSize,
            TotalLines = totalLines,
            FilesByLanguage = organizedFiles.ToDictionary(
                kvp => kvp.Key, 
                kvp => kvp.Value.Count),
            SupportedFiles = FilterSupportedFiles(files, new[] { "csharp" }).Count
        };
    }
}

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