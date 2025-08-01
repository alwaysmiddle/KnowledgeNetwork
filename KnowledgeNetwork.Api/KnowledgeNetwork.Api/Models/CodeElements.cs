namespace KnowledgeNetwork.Api.Models;

/// <summary>
/// Base class for all code elements
/// </summary>
public abstract class CodeElement
{
    public string Name { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string AccessModifier { get; set; } = string.Empty;
}

/// <summary>
/// Represents a class in the analyzed code
/// </summary>
public class ClassElement : CodeElement
{
    public string Namespace { get; set; } = string.Empty;
    public List<MethodElement> Methods { get; set; } = [];
    public List<PropertyElement> Properties { get; set; } = [];
    public List<FieldElement> Fields { get; set; } = [];
    public bool IsStatic { get; set; }
    public bool IsAbstract { get; set; }
    public bool IsSealed { get; set; }
    public string BaseClass { get; set; } = string.Empty;
    public List<string> Interfaces { get; set; } = [];
}

/// <summary>
/// Represents a method in the analyzed code
/// </summary>
public class MethodElement : CodeElement
{
    public string ReturnType { get; set; } = string.Empty;
    public List<ParameterElement> Parameters { get; set; } = [];
    public bool IsStatic { get; set; }
    public bool IsVirtual { get; set; }
    public bool IsOverride { get; set; }
    public bool IsAsync { get; set; }
    public string ContainingClass { get; set; } = string.Empty;
}

/// <summary>
/// Represents a property in the analyzed code
/// </summary>
public class PropertyElement : CodeElement
{
    public string Type { get; set; } = string.Empty;
    public bool HasGetter { get; set; }
    public bool HasSetter { get; set; }
    public bool IsStatic { get; set; }
    public bool IsVirtual { get; set; }
    public bool IsOverride { get; set; }
    public string ContainingClass { get; set; } = string.Empty;
}

/// <summary>
/// Represents a field in the analyzed code
/// </summary>
public class FieldElement : CodeElement
{
    public string Type { get; set; } = string.Empty;
    public bool IsStatic { get; set; }
    public bool IsReadOnly { get; set; }
    public bool IsConst { get; set; }
    public string ContainingClass { get; set; } = string.Empty;
    public string? DefaultValue { get; set; }
}

/// <summary>
/// Represents a method parameter
/// </summary>
public class ParameterElement
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool HasDefaultValue { get; set; }
    public string? DefaultValue { get; set; }
    public string Modifier { get; set; } = string.Empty; // ref, out, in, params
}