namespace KnowledgeNetwork.Domains.Code.Models.Classes;

/// <summary>
/// Summary of class members
/// </summary>
public class ClassMemberSummary
{
    /// <summary>
    /// Number of fields in the class
    /// </summary>
    public int FieldCount { get; set; }

    /// <summary>
    /// Number of properties in the class
    /// </summary>
    public int PropertyCount { get; set; }

    /// <summary>
    /// Number of methods in the class (excluding constructors)
    /// </summary>
    public int MethodCount { get; set; }

    /// <summary>
    /// Number of constructors
    /// </summary>
    public int ConstructorCount { get; set; }

    /// <summary>
    /// Number of nested types
    /// </summary>
    public int NestedTypeCount { get; set; }

    /// <summary>
    /// Number of events
    /// </summary>
    public int EventCount { get; set; }

    /// <summary>
    /// Number of indexers
    /// </summary>
    public int IndexerCount { get; set; }

    /// <summary>
    /// Number of operators
    /// </summary>
    public int OperatorCount { get; set; }
}