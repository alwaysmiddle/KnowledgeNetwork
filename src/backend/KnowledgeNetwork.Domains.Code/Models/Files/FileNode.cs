using KnowledgeNetwork.Domains.Code.Models.Common;

namespace KnowledgeNetwork.Domains.Code.Models.Files;

/// <summary>
/// Represents a file node in a file dependency graph
/// </summary>
public class FileNode
{
    /// <summary>
    /// Unique identifier for this file
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// File name with extension
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Full file path (absolute)
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Relative path within the project/solution
    /// </summary>
    public string RelativePath { get; set; } = string.Empty;

    /// <summary>
    /// Directory path containing this file
    /// </summary>
    public string DirectoryPath { get; set; } = string.Empty;

    /// <summary>
    /// File extension (including the dot)
    /// </summary>
    public string FileExtension { get; set; } = string.Empty;

    /// <summary>
    /// Programming language of the file
    /// </summary>
    public FileLanguage Language { get; set; } = FileLanguage.CSharp;

    /// <summary>
    /// Type of file (source, test, configuration, etc.)
    /// </summary>
    public FileType FileType { get; set; } = FileType.Source;

    /// <summary>
    /// Project this file belongs to
    /// </summary>
    public string ProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Namespaces declared in this file
    /// </summary>
    public List<string> DeclaredNamespaces { get; set; } = new();

    /// <summary>
    /// Using directives in this file
    /// </summary>
    public List<string> UsingDirectives { get; set; } = new();

    /// <summary>
    /// Global using directives that apply to this file
    /// </summary>
    public List<string> GlobalUsings { get; set; } = new();

    /// <summary>
    /// Types (classes, interfaces, etc.) declared in this file
    /// </summary>
    public List<DeclaredType> DeclaredTypes { get; set; } = new();

    /// <summary>
    /// External types referenced by this file
    /// </summary>
    public List<ReferencedType> ReferencedTypes { get; set; } = new();

    /// <summary>
    /// Assembly references used by this file
    /// </summary>
    public List<string> AssemblyReferences { get; set; } = new();

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Last modified timestamp
    /// </summary>
    public DateTime LastModified { get; set; }

    /// <summary>
    /// File metrics
    /// </summary>
    public FileMetrics Metrics { get; set; } = new();

    /// <summary>
    /// Source location information
    /// </summary>
    public CSharpLocationInfo? Location { get; set; }

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Get the primary namespace for this file
    /// </summary>
    public string GetPrimaryNamespace()
    {
        return DeclaredNamespaces.FirstOrDefault() ?? string.Empty;
    }

    /// <summary>
    /// Check if this file uses a specific namespace
    /// </summary>
    public bool UsesNamespace(string namespaceName)
    {
        return UsingDirectives.Contains(namespaceName) || 
               GlobalUsings.Contains(namespaceName) ||
               UsingDirectives.Any(u => u.StartsWith(namespaceName + "."));
    }

    /// <summary>
    /// Get all unique namespaces used by this file
    /// </summary>
    public List<string> GetAllUsedNamespaces()
    {
        var namespaces = new HashSet<string>();
        
        foreach (var usingDir in UsingDirectives.Concat(GlobalUsings))
        {
            namespaces.Add(usingDir);
        }
        
        foreach (var refType in ReferencedTypes)
        {
            if (!string.IsNullOrEmpty(refType.Namespace))
            {
                namespaces.Add(refType.Namespace);
            }
        }
        
        return namespaces.OrderBy(ns => ns).ToList();
    }

    /// <summary>
    /// Check if this file declares a specific type
    /// </summary>
    public bool DeclaresType(string typeName)
    {
        return DeclaredTypes.Any(t => t.Name == typeName || t.FullName == typeName);
    }
}

/// <summary>
/// Programming languages supported
/// </summary>
public enum FileLanguage
{
    CSharp,
    VisualBasic,
    FSharp,
    TypeScript,
    JavaScript,
    Other
}

/// <summary>
/// Types of files
/// </summary>
public enum FileType
{
    /// <summary>
    /// Regular source code file
    /// </summary>
    Source,

    /// <summary>
    /// Test file
    /// </summary>
    Test,

    /// <summary>
    /// Configuration file
    /// </summary>
    Configuration,

    /// <summary>
    /// Resource file
    /// </summary>
    Resource,

    /// <summary>
    /// Generated file
    /// </summary>
    Generated,

    /// <summary>
    /// Designer file
    /// </summary>
    Designer,

    /// <summary>
    /// Partial file
    /// </summary>
    Partial,

    /// <summary>
    /// Interface definition
    /// </summary>
    Interface,

    /// <summary>
    /// Data model file
    /// </summary>
    Model
}

/// <summary>
/// Information about a type declared in a file
/// </summary>
public class DeclaredType
{
    /// <summary>
    /// Type name (without namespace)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Full qualified name including namespace
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Namespace the type belongs to
    /// </summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// Kind of type (class, interface, struct, etc.)
    /// </summary>
    public string TypeKind { get; set; } = string.Empty;

    /// <summary>
    /// Visibility of the type
    /// </summary>
    public string Visibility { get; set; } = string.Empty;

    /// <summary>
    /// Whether the type is static
    /// </summary>
    public bool IsStatic { get; set; }

    /// <summary>
    /// Whether the type is generic
    /// </summary>
    public bool IsGeneric { get; set; }

    /// <summary>
    /// Source location where the type is declared
    /// </summary>
    public CSharpLocationInfo? Location { get; set; }
}

/// <summary>
/// Information about a type referenced by a file
/// </summary>
public class ReferencedType
{
    /// <summary>
    /// Type name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Full qualified name
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Namespace of the referenced type
    /// </summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// Assembly containing the type
    /// </summary>
    public string Assembly { get; set; } = string.Empty;

    /// <summary>
    /// How the type is referenced
    /// </summary>
    public TypeReferenceKind ReferenceKind { get; set; } = TypeReferenceKind.Direct;

    /// <summary>
    /// Number of times this type is referenced
    /// </summary>
    public int ReferenceCount { get; set; } = 1;

    /// <summary>
    /// Whether this is an external (non-project) type
    /// </summary>
    public bool IsExternal { get; set; }

    /// <summary>
    /// Source locations where the type is referenced
    /// </summary>
    public List<CSharpLocationInfo> ReferenceLocations { get; set; } = new();
}

/// <summary>
/// Ways a type can be referenced
/// </summary>
public enum TypeReferenceKind
{
    /// <summary>
    /// Direct usage of the type
    /// </summary>
    Direct,

    /// <summary>
    /// Usage through inheritance
    /// </summary>
    Inheritance,

    /// <summary>
    /// Usage through interface implementation
    /// </summary>
    Interface,

    /// <summary>
    /// Usage as generic type parameter
    /// </summary>
    GenericParameter,

    /// <summary>
    /// Usage in attribute
    /// </summary>
    Attribute,

    /// <summary>
    /// Usage in reflection
    /// </summary>
    Reflection
}

/// <summary>
/// Metrics for a file
/// </summary>
public class FileMetrics
{
    /// <summary>
    /// Total lines of code (including comments and blank lines)
    /// </summary>
    public int TotalLines { get; set; }

    /// <summary>
    /// Lines of code (excluding comments and blank lines)
    /// </summary>
    public int LinesOfCode { get; set; }

    /// <summary>
    /// Number of comment lines
    /// </summary>
    public int CommentLines { get; set; }

    /// <summary>
    /// Number of blank lines
    /// </summary>
    public int BlankLines { get; set; }

    /// <summary>
    /// Number of using directives
    /// </summary>
    public int UsingDirectiveCount { get; set; }

    /// <summary>
    /// Number of types declared in this file
    /// </summary>
    public int DeclaredTypeCount { get; set; }

    /// <summary>
    /// Number of types referenced by this file
    /// </summary>
    public int ReferencedTypeCount { get; set; }

    /// <summary>
    /// Number of dependencies (files this file depends on)
    /// </summary>
    public int DependencyCount { get; set; }

    /// <summary>
    /// Number of dependents (files that depend on this file)
    /// </summary>
    public int DependentCount { get; set; }

    /// <summary>
    /// Cyclomatic complexity of all methods in the file
    /// </summary>
    public int CyclomaticComplexity { get; set; }

    /// <summary>
    /// Maintainability index (0-100, higher is better)
    /// </summary>
    public double MaintainabilityIndex { get; set; }

    /// <summary>
    /// Technical debt ratio (0-1, lower is better)
    /// </summary>
    public double TechnicalDebtRatio { get; set; }
}