using KnowledgeNetwork.Domains.Code.Models.Common;

namespace KnowledgeNetwork.Domains.Code.Models.Files;

/// <summary>
/// Represents a dependency relationship created by using directives
/// </summary>
public class UsingDependencyEdge
{
    /// <summary>
    /// Unique identifier for this edge
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// ID of the file that contains the using directive (source)
    /// </summary>
    public string SourceFileId { get; set; } = string.Empty;

    /// <summary>
    /// ID of the file that provides the namespace being used (target)
    /// </summary>
    public string TargetFileId { get; set; } = string.Empty;

    /// <summary>
    /// The namespace being imported
    /// </summary>
    public string NamespaceName { get; set; } = string.Empty;

    /// <summary>
    /// The exact using directive text
    /// </summary>
    public string UsingDirective { get; set; } = string.Empty;

    /// <summary>
    /// Type of using directive
    /// </summary>
    public UsingDirectiveType DirectiveType { get; set; } = UsingDirectiveType.Namespace;

    /// <summary>
    /// Whether this is a global using directive
    /// </summary>
    public bool IsGlobal { get; set; }

    /// <summary>
    /// Whether this is a static using directive
    /// </summary>
    public bool IsStatic { get; set; }

    /// <summary>
    /// Whether this is an alias using directive
    /// </summary>
    public bool IsAlias { get; set; }

    /// <summary>
    /// Alias name if this is an alias directive
    /// </summary>
    public string? AliasName { get; set; }

    /// <summary>
    /// Whether the target is in an external assembly
    /// </summary>
    public bool IsExternalAssembly { get; set; }

    /// <summary>
    /// Name of the external assembly if applicable
    /// </summary>
    public string ExternalAssemblyName { get; set; } = string.Empty;

    /// <summary>
    /// Whether this using is actually utilized in the source file
    /// </summary>
    public bool IsUtilized { get; set; }

    /// <summary>
    /// Number of times types from this namespace are used
    /// </summary>
    public int UtilizationCount { get; set; }

    /// <summary>
    /// Specific types from this namespace that are used
    /// </summary>
    public List<string> UtilizedTypes { get; set; } = new();

    /// <summary>
    /// Source location where the using directive is declared
    /// </summary>
    public CSharpLocationInfo? UsingLocation { get; set; }

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Types of using directives
/// </summary>
public enum UsingDirectiveType
{
    /// <summary>
    /// Regular namespace using (using System;)
    /// </summary>
    Namespace,

    /// <summary>
    /// Static using (using static System.Math;)
    /// </summary>
    Static,

    /// <summary>
    /// Alias using (using Dict = System.Collections.Generic.Dictionary;)
    /// </summary>
    Alias,

    /// <summary>
    /// Global using (global using System;)
    /// </summary>
    Global,

    /// <summary>
    /// Global static using (global using static System.Math;)
    /// </summary>
    GlobalStatic,

    /// <summary>
    /// Global alias using (global using Dict = System.Collections.Generic.Dictionary;)
    /// </summary>
    GlobalAlias
}