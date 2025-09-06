namespace KnowledgeNetwork.Domains.Code.Models.Classes.ClassEnums;

/// <summary>
/// Types of inheritance relationships
/// </summary>
public enum InheritanceType
{
    /// <summary>
    /// Class to class inheritance
    /// </summary>
    Class,

    /// <summary>
    /// Class to abstract class inheritance
    /// </summary>
    AbstractClass,

    /// <summary>
    /// Record to record inheritance
    /// </summary>
    Record,

    /// <summary>
    /// Interface to interface inheritance
    /// </summary>
    Interface
}