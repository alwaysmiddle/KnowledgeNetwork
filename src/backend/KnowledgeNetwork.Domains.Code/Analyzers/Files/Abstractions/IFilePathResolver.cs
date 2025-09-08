using KnowledgeNetwork.Domains.Code.Models.Files;

namespace KnowledgeNetwork.Domains.Code.Analyzers.Files.Abstractions;

/// <summary>
/// Handles file path operations and type determination
/// </summary>
public interface IFilePathResolver
{
    /// <summary>
    /// Gets relative path for a file from the current directory
    /// </summary>
    /// <param name="filePath">The absolute file path</param>
    /// <returns>The relative path, or the original path if conversion fails</returns>
    string GetRelativePath(string filePath);

    /// <summary>
    /// Determines the type of file based on name and path patterns
    /// </summary>
    /// <param name="fileName">The file name</param>
    /// <param name="filePath">The full file path</param>
    /// <returns>The determined file type</returns>
    FileType DetermineFileType(string fileName, string filePath);
}