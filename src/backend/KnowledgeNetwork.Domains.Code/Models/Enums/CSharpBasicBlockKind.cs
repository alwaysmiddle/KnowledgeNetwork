namespace KnowledgeNetwork.Domains.Code.Models.Enums;

/// <summary>
/// Types of basic blocks in control flow
/// </summary>
public enum CSharpBasicBlockKind
{
    /// <summary>
    /// Entry point of the method
    /// </summary>
    Entry,

    /// <summary>
    /// Exit point of the method
    /// </summary>
    Exit,

    /// <summary>
    /// Regular block with operations
    /// </summary>
    Block,

    /// <summary>
    /// Block that handles exceptions
    /// </summary>
    ExceptionHandler
}