using KnowledgeNetwork.Domains.Code.Models.Common;
using KnowledgeNetwork.Domains.Code.Models.Enums;

namespace KnowledgeNetwork.Domains.Code.Models.Methods;

/// <summary>
/// Represents a method node in a method relationship graph
/// </summary>
public class MethodNode
{
    /// <summary>
    /// Unique identifier for this method
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Method name (without parameters)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Full method signature including parameters
    /// </summary>
    public string Signature { get; set; } = string.Empty;

    /// <summary>
    /// Return type of the method
    /// </summary>
    public string ReturnType { get; set; } = string.Empty;

    /// <summary>
    /// Method visibility (public, private, protected, etc.)
    /// </summary>
    public MethodVisibility Visibility { get; set; } = MethodVisibility.Private;

    /// <summary>
    /// Whether the method is static
    /// </summary>
    public bool IsStatic { get; set; }

    /// <summary>
    /// Whether the method is abstract
    /// </summary>
    public bool IsAbstract { get; set; }

    /// <summary>
    /// Whether the method is virtual
    /// </summary>
    public bool IsVirtual { get; set; }

    /// <summary>
    /// Whether the method is an override
    /// </summary>
    public bool IsOverride { get; set; }

    /// <summary>
    /// Whether the method is async
    /// </summary>
    public bool IsAsync { get; set; }

    /// <summary>
    /// Whether this is a constructor
    /// </summary>
    public bool IsConstructor { get; set; }

    /// <summary>
    /// Method parameters
    /// </summary>
    public List<MethodParameter> Parameters { get; set; } = new();

    /// <summary>
    /// Source location information
    /// </summary>
    public CSharpLocationInfo? Location { get; set; }

    /// <summary>
    /// Complexity metrics for this method
    /// </summary>
    public MethodComplexityMetrics Metrics { get; set; } = new();

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Visibility levels for methods
/// </summary>
public enum MethodVisibility
{
    Private,
    Protected,
    Internal,
    Public,
    ProtectedInternal,
    PrivateProtected
}

/// <summary>
/// Represents a method parameter
/// </summary>
public class MethodParameter
{
    /// <summary>
    /// Parameter name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Parameter type
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Whether the parameter has a default value
    /// </summary>
    public bool HasDefaultValue { get; set; }

    /// <summary>
    /// Default value if any
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Whether the parameter is ref
    /// </summary>
    public bool IsRef { get; set; }

    /// <summary>
    /// Whether the parameter is out
    /// </summary>
    public bool IsOut { get; set; }

    /// <summary>
    /// Whether the parameter is params
    /// </summary>
    public bool IsParams { get; set; }
}

/// <summary>
/// Complexity metrics for a method
/// </summary>
public class MethodComplexityMetrics
{
    /// <summary>
    /// Number of lines in the method
    /// </summary>
    public int LineCount { get; set; }

    /// <summary>
    /// Number of parameters
    /// </summary>
    public int ParameterCount { get; set; }

    /// <summary>
    /// Number of local variables
    /// </summary>
    public int LocalVariableCount { get; set; }

    /// <summary>
    /// Number of method calls made by this method
    /// </summary>
    public int MethodCallCount { get; set; }

    /// <summary>
    /// Number of field/property accesses
    /// </summary>
    public int FieldAccessCount { get; set; }

    /// <summary>
    /// Whether the method has exception handling
    /// </summary>
    public bool HasExceptionHandling { get; set; }

    /// <summary>
    /// Cognitive complexity score
    /// </summary>
    public int CognitiveComplexity { get; set; }
}