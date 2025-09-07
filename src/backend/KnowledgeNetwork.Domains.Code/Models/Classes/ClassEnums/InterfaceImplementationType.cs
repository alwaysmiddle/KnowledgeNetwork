namespace KnowledgeNetwork.Domains.Code.Models.Classes.ClassEnums;

/// <summary>
/// Types of interface implementation
/// </summary>
public enum InterfaceImplementationType
{
    /// <summary>
    /// Direct implementation by the class
    /// </summary>
    Direct,

    /// <summary>
    /// Inherited from base class
    /// </summary>
    Inherited,

    /// <summary>
    /// Both direct and inherited
    /// </summary>
    DirectAndInherited
}