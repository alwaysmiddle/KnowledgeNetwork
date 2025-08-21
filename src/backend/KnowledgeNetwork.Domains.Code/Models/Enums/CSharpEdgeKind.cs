namespace KnowledgeNetwork.Domains.Code.Models.Enums;

/// <summary>
/// Types of control flow edges
/// </summary>
public enum CSharpEdgeKind
{
    /// <summary>
    /// Normal sequential flow
    /// </summary>
    Regular,

    /// <summary>
    /// Conditional branch (true path)
    /// </summary>
    ConditionalTrue,

    /// <summary>
    /// Conditional branch (false path)
    /// </summary>
    ConditionalFalse,

    /// <summary>
    /// Back edge indicating a loop
    /// </summary>
    BackEdge,

    /// <summary>
    /// Exception flow edge
    /// </summary>
    Exception,

    /// <summary>
    /// Return statement edge to exit
    /// </summary>
    Return,

    /// <summary>
    /// Switch case edge
    /// </summary>
    SwitchCase,

    /// <summary>
    /// Default case in switch
    /// </summary>
    SwitchDefault
}