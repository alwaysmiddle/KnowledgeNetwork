namespace KnowledgeNetwork.Core.Models.Constants;

/// <summary>
/// Universal node types that work across all programming languages and domains
/// </summary>
public static class PrimaryNodeType
{
    // Code Entities
    public const string Namespace = "namespace";
    public const string Class = "class";
    public const string Interface = "interface";
    public const string Method = "method";
    public const string Function = "function";
    public const string Property = "property";
    public const string Field = "field";
    public const string Parameter = "parameter";
    public const string Variable = "variable";
    
    // Control Flow
    public const string BasicBlock = "basic-block";
    public const string Decision = "decision";
    public const string Loop = "loop";
    
    // Architectural
    public const string Module = "module";
    public const string Package = "package";
    public const string Component = "component";
    public const string Service = "service";
    
    // Documentation
    public const string Document = "document";
    public const string Section = "section";
    public const string Example = "example";
    
    // Views (Meta-nodes)
    public const string View = "view";
    public const string Filter = "filter";
    public const string Query = "query";
    public const string Snapshot = "snapshot";
    
    // Default
    public const string Unknown = "unknown";
}