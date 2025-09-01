using System.Reflection;

namespace KnowledgeNetwork.Tests.Shared;

/// <summary>
/// Repository for loading and organizing C# test code samples.
/// Provides easy access to sample code files for testing different scenarios.
/// </summary>
public static class SampleCodeRepository
{
    private static readonly Dictionary<string, string> _codeCache = new();
    
    /// <summary>
    /// Get the base path for sample code files
    /// </summary>
    public static string SampleCodeBasePath => 
        Path.Combine(GetTestProjectRoot(), "TestData", "SampleCode");

    /// <summary>
    /// Load a specific sample code file
    /// </summary>
    /// <param name="category">Category folder (e.g., "ControlFlow", "SimpleClasses")</param>
    /// <param name="fileName">File name including .cs extension</param>
    /// <returns>C# source code content</returns>
    public static string LoadSample(string category, string fileName)
    {
        var cacheKey = $"{category}/{fileName}";
        
        if (_codeCache.TryGetValue(cacheKey, out var cachedCode))
        {
            return cachedCode;
        }

        var filePath = Path.Combine(SampleCodeBasePath, category, fileName);
        
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Sample code file not found: {filePath}");
        }

        var code = File.ReadAllText(filePath);
        _codeCache[cacheKey] = code;
        
        return code;
    }

    /// <summary>
    /// Load all sample files from a specific category
    /// </summary>
    /// <param name="category">Category folder name</param>
    /// <returns>Dictionary of filename -> source code</returns>
    public static Dictionary<string, string> LoadCategory(string category)
    {
        var categoryPath = Path.Combine(SampleCodeBasePath, category);
        
        if (!Directory.Exists(categoryPath))
        {
            throw new DirectoryNotFoundException($"Sample code category not found: {categoryPath}");
        }

        var result = new Dictionary<string, string>();
        var files = Directory.GetFiles(categoryPath, "*.cs");

        foreach (var file in files)
        {
            var fileName = Path.GetFileName(file);
            var code = File.ReadAllText(file);
            result[fileName] = code;
        }

        return result;
    }

    /// <summary>
    /// Get available sample categories
    /// </summary>
    /// <returns>List of category folder names</returns>
    public static List<string> GetCategories()
    {
        var categoriesPath = SampleCodeBasePath;
        
        if (!Directory.Exists(categoriesPath))
        {
            return new List<string>();
        }

        var directories = Directory.GetDirectories(categoriesPath);
        var categories = new List<string>();

        foreach (var dir in directories)
        {
            categories.Add(Path.GetFileName(dir));
        }

        return categories;
    }

    /// <summary>
    /// Get all files in a specific category
    /// </summary>
    /// <param name="category">Category folder name</param>
    /// <returns>List of file names in the category</returns>
    public static List<string> GetFilesInCategory(string category)
    {
        var categoryPath = Path.Combine(SampleCodeBasePath, category);
        
        if (!Directory.Exists(categoryPath))
        {
            return new List<string>();
        }

        var files = Directory.GetFiles(categoryPath, "*.cs");
        var fileNames = new List<string>();

        foreach (var file in files)
        {
            fileNames.Add(Path.GetFileName(file));
        }

        return fileNames;
    }

    // Predefined sample access methods for common scenarios
    
    /// <summary>
    /// Get simple linear method sample
    /// </summary>
    public static string GetSimpleLinearMethod() => 
        LoadSample("ControlFlow", "Simple_LinearMethod.cs");

    /// <summary>
    /// Get conditional methods sample
    /// </summary>
    public static string GetConditionalMethods() => 
        LoadSample("ControlFlow", "Conditional_Methods.cs");

    /// <summary>
    /// Get loop methods sample
    /// </summary>
    public static string GetLoopMethods() => 
        LoadSample("ControlFlow", "Loop_Methods.cs");

    // Edge case sample access methods
    
    /// <summary>
    /// Get async methods edge cases
    /// </summary>
    public static string GetAsyncMethods() => 
        LoadSample("EdgeCases", "Async_Methods.cs");

    /// <summary>
    /// Get exception handling edge cases
    /// </summary>
    public static string GetExceptionHandling() => 
        LoadSample("EdgeCases", "Exception_Handling.cs");

    /// <summary>
    /// Get complex expressions edge cases
    /// </summary>
    public static string GetComplexExpressions() => 
        LoadSample("EdgeCases", "Complex_Expressions.cs");

    /// <summary>
    /// Get control flow variations edge cases
    /// </summary>
    public static string GetControlFlowVariations() => 
        LoadSample("EdgeCases", "Control_Flow_Variations.cs");

    /// <summary>
    /// Get method variations edge cases
    /// </summary>
    public static string GetMethodVariations() => 
        LoadSample("EdgeCases", "Method_Variations.cs");

    /// <summary>
    /// Clear the internal code cache (useful for testing)
    /// </summary>
    public static void ClearCache()
    {
        _codeCache.Clear();
    }

    /// <summary>
    /// Get the root directory of the test project
    /// </summary>
    private static string GetTestProjectRoot()
    {
        // Get the directory where the current assembly is located
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var assemblyDir = Path.GetDirectoryName(assemblyLocation);
        
        // Navigate up to find the test project root
        var currentDir = assemblyDir;
        while (currentDir != null && !File.Exists(Path.Combine(currentDir, "KnowledgeNetwork.Domains.Code.Tests.csproj")))
        {
            currentDir = Path.GetDirectoryName(currentDir);
        }
        
        if (currentDir == null)
        {
            throw new InvalidOperationException("Could not locate test project root directory");
        }
        
        return currentDir;
    }
}