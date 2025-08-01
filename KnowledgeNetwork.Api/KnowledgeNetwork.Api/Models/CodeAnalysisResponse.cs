namespace KnowledgeNetwork.Api.Models;

/// <summary>
/// Response model for code analysis API
/// </summary>
public class CodeAnalysisResponse
{
    /// <summary>
    /// Indicates if the analysis was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if analysis failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// List of compilation errors or warnings
    /// </summary>
    public List<string> Diagnostics { get; set; } = [];

    /// <summary>
    /// Summary statistics of the analyzed code
    /// </summary>
    public CodeAnalysisSummary Summary { get; set; } = new();

    /// <summary>
    /// Detailed analysis results
    /// </summary>
    public CodeAnalysisDetails Details { get; set; } = new();

    /// <summary>
    /// Raw syntax tree information (if requested)
    /// </summary>
    public string? SyntaxTree { get; set; }

    /// <summary>
    /// Timestamp when analysis was performed
    /// </summary>
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Time taken to perform the analysis
    /// </summary>
    public TimeSpan AnalysisDuration { get; set; }
}

/// <summary>
/// Summary statistics of code analysis
/// </summary>
public class CodeAnalysisSummary
{
    /// <summary>
    /// Total number of classes found
    /// </summary>
    public int ClassCount { get; set; }

    /// <summary>
    /// Total number of methods found
    /// </summary>
    public int MethodCount { get; set; }

    /// <summary>
    /// Total number of properties found
    /// </summary>
    public int PropertyCount { get; set; }

    /// <summary>
    /// Total number of fields found
    /// </summary>
    public int FieldCount { get; set; }

    /// <summary>
    /// Total number of namespaces found
    /// </summary>
    public int NamespaceCount { get; set; }

    /// <summary>
    /// Total number of using statements
    /// </summary>
    public int UsingCount { get; set; }

    /// <summary>
    /// Total lines of code
    /// </summary>
    public int LinesOfCode { get; set; }

    /// <summary>
    /// List of all namespaces found
    /// </summary>
    public List<string> Namespaces { get; set; } = [];

    /// <summary>
    /// List of all using statements
    /// </summary>
    public List<string> UsingStatements { get; set; } = [];
}

/// <summary>
/// Detailed analysis results
/// </summary>
public class CodeAnalysisDetails
{
    /// <summary>
    /// All classes found in the code
    /// </summary>
    public List<ClassElement> Classes { get; set; } = [];

    /// <summary>
    /// All methods found in the code (including those in classes)
    /// </summary>
    public List<MethodElement> Methods { get; set; } = [];

    /// <summary>
    /// All properties found in the code
    /// </summary>
    public List<PropertyElement> Properties { get; set; } = [];

    /// <summary>
    /// All fields found in the code
    /// </summary>
    public List<FieldElement> Fields { get; set; } = [];
}