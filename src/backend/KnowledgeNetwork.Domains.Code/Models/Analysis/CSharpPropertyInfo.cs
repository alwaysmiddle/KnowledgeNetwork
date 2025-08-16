namespace KnowledgeNetwork.Domains.Code.Models.Analysis;

/// <summary>
/// Information about a property declaration
/// </summary>
public class CSharpPropertyInfo
{
    /// <summary>
    /// Name of the property
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Data type of the property
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Access modifiers and other modifiers
    /// </summary>
    public string Modifiers { get; set; } = string.Empty;

    /// <summary>
    /// Whether the property has a getter
    /// </summary>
    public bool HasGetter { get; set; }

    /// <summary>
    /// Whether the property has a setter
    /// </summary>
    public bool HasSetter { get; set; }

    /// <summary>
    /// Line number where the property is declared
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// Name of the class containing this property
    /// </summary>
    public string ClassName { get; set; } = string.Empty;
}