using KnowledgeNetwork.Domains.Code.Analyzers.Files.Abstractions;
using KnowledgeNetwork.Domains.Code.Models.Files;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Files.Utilities;

/// <summary>
/// Handles file path operations and type determination
/// </summary>
public class FilePathResolver : IFilePathResolver
{
    /// <summary>
    /// Gets relative path for a file from the current directory
    /// </summary>
    public string GetRelativePath(string filePath)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var uri = new Uri(currentDirectory + Path.DirectorySeparatorChar);
        var fileUri = new Uri(filePath);
        
        return uri.IsBaseOf(fileUri) 
            ? Uri.UnescapeDataString(uri.MakeRelativeUri(fileUri).ToString().Replace('/', Path.DirectorySeparatorChar)) 
            : filePath;
    }

    /// <summary>
    /// Determines the type of file based on name and path patterns
    /// </summary>
    public FileType DetermineFileType(string fileName, string filePath)
    {
        var lowerFileName = fileName.ToLowerInvariant();
        var lowerPath = filePath.ToLowerInvariant();

        if (lowerFileName.Contains("test") || lowerPath.Contains("test"))
            return FileType.Test;
        
        if (lowerFileName.EndsWith(".designer.cs"))
            return FileType.Designer;
        
        if (lowerFileName.EndsWith(".g.cs") || lowerFileName.EndsWith(".generated.cs"))
            return FileType.Generated;
        
        if (lowerFileName.Contains("config") || lowerFileName.EndsWith(".config"))
            return FileType.Configuration;
        
        if (lowerFileName.Contains("resource") || lowerFileName.EndsWith(".resx"))
            return FileType.Resource;

        return FileType.Source;
    }
}