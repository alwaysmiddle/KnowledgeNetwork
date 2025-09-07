namespace KnowledgeNetwork.Domains.Code.Models.Classes.ClassEnums;

/// <summary>
/// How the composed object is accessed
/// </summary>
public enum CompositionAccessType
{
    /// <summary>
    /// Direct field access
    /// </summary>
    Field,

    /// <summary>
    /// Property access
    /// </summary>
    Property,

    /// <summary>
    /// Collection (List, Array, etc.)
    /// </summary>
    Collection,

    /// <summary>
    /// Dictionary or map
    /// </summary>
    Dictionary,

    /// <summary>
    /// Auto-property
    /// </summary>
    AutoProperty,

    /// <summary>
    /// Constructor parameter
    /// </summary>
    Constructor
}