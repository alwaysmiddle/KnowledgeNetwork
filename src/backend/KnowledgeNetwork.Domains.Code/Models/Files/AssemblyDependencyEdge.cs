using KnowledgeNetwork.Domains.Code.Models.Common;

namespace KnowledgeNetwork.Domains.Code.Models.Files;

/// <summary>
/// Represents a dependency on an external assembly
/// </summary>
public class AssemblyDependencyEdge
{
    /// <summary>
    /// Unique identifier for this edge
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// ID of the file that depends on the assembly (source)
    /// </summary>
    public string SourceFileId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the assembly being depended upon
    /// </summary>
    public string AssemblyName { get; set; } = string.Empty;

    /// <summary>
    /// Full assembly qualified name
    /// </summary>
    public string AssemblyFullName { get; set; } = string.Empty;

    /// <summary>
    /// Version of the assembly
    /// </summary>
    public string AssemblyVersion { get; set; } = string.Empty;

    /// <summary>
    /// Type of assembly dependency
    /// </summary>
    public AssemblyDependencyType DependencyType { get; set; } = AssemblyDependencyType.Reference;

    /// <summary>
    /// Source of the assembly
    /// </summary>
    public AssemblySource Source { get; set; } = AssemblySource.NuGet;

    /// <summary>
    /// Whether this is a direct or transitive dependency
    /// </summary>
    public bool IsDirectDependency { get; set; } = true;

    /// <summary>
    /// Framework or runtime the assembly targets
    /// </summary>
    public string TargetFramework { get; set; } = string.Empty;

    /// <summary>
    /// Specific namespaces from this assembly that are used
    /// </summary>
    public List<string> UsedNamespaces { get; set; } = new();

    /// <summary>
    /// Specific types from this assembly that are used
    /// </summary>
    public List<AssemblyTypeUsage> UsedTypes { get; set; } = new();

    /// <summary>
    /// Number of times types from this assembly are used
    /// </summary>
    public int UsageCount { get; set; } = 1;

    /// <summary>
    /// Importance/criticality of this dependency
    /// </summary>
    public DependencyImportance Importance { get; set; } = DependencyImportance.Moderate;

    /// <summary>
    /// Whether this assembly is part of the .NET Base Class Library
    /// </summary>
    public bool IsBaseClassLibrary { get; set; }

    /// <summary>
    /// Whether this is a Microsoft-owned assembly
    /// </summary>
    public bool IsMicrosoftAssembly { get; set; }

    /// <summary>
    /// Security risk level of this dependency
    /// </summary>
    public SecurityRiskLevel SecurityRisk { get; set; } = SecurityRiskLevel.Low;

    /// <summary>
    /// License information for the assembly
    /// </summary>
    public string License { get; set; } = string.Empty;

    /// <summary>
    /// Source location where the dependency is first introduced
    /// </summary>
    public CSharpLocationInfo? IntroductionLocation { get; set; }

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Types of assembly dependencies
/// </summary>
public enum AssemblyDependencyType
{
    /// <summary>
    /// Direct assembly reference
    /// </summary>
    Reference,

    /// <summary>
    /// Runtime dependency
    /// </summary>
    Runtime,

    /// <summary>
    /// Development/build-time dependency
    /// </summary>
    Development,

    /// <summary>
    /// Test dependency
    /// </summary>
    Test,

    /// <summary>
    /// Analyzer dependency
    /// </summary>
    Analyzer,

    /// <summary>
    /// Framework dependency
    /// </summary>
    Framework,

    /// <summary>
    /// Tool dependency
    /// </summary>
    Tool
}

/// <summary>
/// Sources where assemblies come from
/// </summary>
public enum AssemblySource
{
    /// <summary>
    /// NuGet package
    /// </summary>
    NuGet,

    /// <summary>
    /// Global Assembly Cache (GAC)
    /// </summary>
    GAC,

    /// <summary>
    /// Local file reference
    /// </summary>
    Local,

    /// <summary>
    /// .NET Framework/Runtime
    /// </summary>
    Framework,

    /// <summary>
    /// Project reference within solution
    /// </summary>
    ProjectReference,

    /// <summary>
    /// Unknown source
    /// </summary>
    Unknown
}

/// <summary>
/// Importance levels for dependencies
/// </summary>
public enum DependencyImportance
{
    /// <summary>
    /// Low importance - rarely used
    /// </summary>
    Low,

    /// <summary>
    /// Moderate importance - occasionally used
    /// </summary>
    Moderate,

    /// <summary>
    /// High importance - frequently used
    /// </summary>
    High,

    /// <summary>
    /// Critical importance - essential for application
    /// </summary>
    Critical
}

/// <summary>
/// Security risk levels for external dependencies
/// </summary>
public enum SecurityRiskLevel
{
    /// <summary>
    /// Very low risk - well-known, trusted sources
    /// </summary>
    VeryLow,

    /// <summary>
    /// Low risk - reputable sources with good track record
    /// </summary>
    Low,

    /// <summary>
    /// Moderate risk - some concerns but generally acceptable
    /// </summary>
    Moderate,

    /// <summary>
    /// High risk - significant security concerns
    /// </summary>
    High,

    /// <summary>
    /// Very high risk - known security issues or untrusted sources
    /// </summary>
    VeryHigh,

    /// <summary>
    /// Unknown risk - insufficient information to assess
    /// </summary>
    Unknown
}

/// <summary>
/// Information about how a specific type from an assembly is used
/// </summary>
public class AssemblyTypeUsage
{
    /// <summary>
    /// Name of the type being used
    /// </summary>
    public string TypeName { get; set; } = string.Empty;

    /// <summary>
    /// Full qualified name of the type
    /// </summary>
    public string FullTypeName { get; set; } = string.Empty;

    /// <summary>
    /// Namespace the type belongs to
    /// </summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// How the type is being used
    /// </summary>
    public List<TypeUsageKind> UsageKinds { get; set; } = new();

    /// <summary>
    /// Number of times this type is used
    /// </summary>
    public int UsageCount { get; set; } = 1;

    /// <summary>
    /// Whether this type is critical to application functionality
    /// </summary>
    public bool IsCritical { get; set; }

    /// <summary>
    /// Source locations where this type is used
    /// </summary>
    public List<CSharpLocationInfo> UsageLocations { get; set; } = new();
}