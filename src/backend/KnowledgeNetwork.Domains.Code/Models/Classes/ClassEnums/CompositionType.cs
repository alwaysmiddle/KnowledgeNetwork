namespace KnowledgeNetwork.Domains.Code.Models.Classes.ClassEnums;

/// <summary>
/// Types of composition relationships
/// </summary>
public enum CompositionType
{
    /// <summary>
    /// Weak composition - contained object can exist independently
    /// </summary>
    Aggregation,

    /// <summary>
    /// Strong composition - contained object's lifetime is tied to container
    /// </summary>
    Composition,

    /// <summary>
    /// Association - loose relationship, typically through method parameters or return types
    /// </summary>
    Association,

    /// <summary>
    /// Dependency - uses the other class but doesn't store a reference
    /// </summary>
    Dependency
}