using KnowledgeNetwork.Domains.Code.Models.Common;

namespace KnowledgeNetwork.Domains.Code.Models.Files;

/// <summary>
/// Represents a project reference dependency for multi-project solutions
/// </summary>
public class ProjectReferenceDependencyEdge
{
    /// <summary>
    /// Unique identifier for this edge
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// ID of the file in the source project
    /// </summary>
    public string SourceFileId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the source project
    /// </summary>
    public string SourceProjectName { get; set; } = string.Empty;

    /// <summary>
    /// ID of the file in the target project (if applicable)
    /// </summary>
    public string TargetFileId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the target project being referenced
    /// </summary>
    public string TargetProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Path to the target project file
    /// </summary>
    public string TargetProjectPath { get; set; } = string.Empty;

    /// <summary>
    /// Type of project reference
    /// </summary>
    public ProjectReferenceType ReferenceType { get; set; } = ProjectReferenceType.Standard;

    /// <summary>
    /// Specific namespaces from the target project that are used
    /// </summary>
    public List<string> UsedNamespaces { get; set; } = new();

    /// <summary>
    /// Specific types from the target project that are used
    /// </summary>
    public List<string> UsedTypes { get; set; } = new();

    /// <summary>
    /// Number of times types from the target project are used
    /// </summary>
    public int UsageCount { get; set; } = 1;

    /// <summary>
    /// Whether this is a direct or transitive project dependency
    /// </summary>
    public bool IsDirectDependency { get; set; } = true;

    /// <summary>
    /// Target framework of the referenced project
    /// </summary>
    public string TargetFramework { get; set; } = string.Empty;

    /// <summary>
    /// Whether the reference is conditional (based on build configuration)
    /// </summary>
    public bool IsConditional { get; set; }

    /// <summary>
    /// Build conditions for this reference
    /// </summary>
    public List<string> BuildConditions { get; set; } = new();

    /// <summary>
    /// Whether this creates a circular project dependency
    /// </summary>
    public bool IsCircularDependency { get; set; }

    /// <summary>
    /// Architectural layer relationship
    /// </summary>
    public LayerRelationship LayerRelationship { get; set; } = LayerRelationship.SameLayer;

    /// <summary>
    /// Source location where the dependency is introduced
    /// </summary>
    public CSharpLocationInfo? DependencyLocation { get; set; }

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Types of project references
/// </summary>
public enum ProjectReferenceType
{
    /// <summary>
    /// Standard project reference
    /// </summary>
    Standard,

    /// <summary>
    /// Analyzer project reference
    /// </summary>
    Analyzer,

    /// <summary>
    /// Test project reference
    /// </summary>
    Test,

    /// <summary>
    /// Shared project reference
    /// </summary>
    Shared,

    /// <summary>
    /// Package reference (NuGet)
    /// </summary>
    Package,

    /// <summary>
    /// Framework reference
    /// </summary>
    Framework
}

/// <summary>
/// Architectural layer relationships between projects
/// </summary>
public enum LayerRelationship
{
    /// <summary>
    /// Same architectural layer
    /// </summary>
    SameLayer,

    /// <summary>
    /// Upward dependency (lower layer to higher layer) - VIOLATION
    /// </summary>
    UpwardDependency,

    /// <summary>
    /// Downward dependency (higher layer to lower layer) - CORRECT
    /// </summary>
    DownwardDependency,

    /// <summary>
    /// Horizontal dependency (across layers at same level)
    /// </summary>
    HorizontalDependency,

    /// <summary>
    /// Cross-cutting concern (infrastructure, logging, etc.)
    /// </summary>
    CrossCutting,

    /// <summary>
    /// Unknown or undefined layer relationship
    /// </summary>
    Unknown
}