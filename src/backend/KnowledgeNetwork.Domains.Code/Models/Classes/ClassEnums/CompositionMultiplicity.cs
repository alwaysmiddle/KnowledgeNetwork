namespace KnowledgeNetwork.Domains.Code.Models.Classes.ClassEnums;

/// <summary>
/// Multiplicity of the composition relationship
/// </summary>
public enum CompositionMultiplicity
{
    /// <summary>
    /// Single instance (1:1)
    /// </summary>
    One,

    /// <summary>
    /// Optional single instance (0:1)
    /// </summary>
    ZeroOrOne,

    /// <summary>
    /// Multiple instances (1:many)
    /// </summary>
    Many,

    /// <summary>
    /// Optional multiple instances (0:many)
    /// </summary>
    ZeroOrMany
}