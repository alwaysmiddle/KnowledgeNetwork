namespace KnowledgeNetwork.Core.Models.Constants;

/// <summary>
/// Predefined edge types for relationships between nodes
/// </summary>
public static class EdgeTypes
{
    // Structural Relationships
    public const string Contains = "contains";
    public const string Inherits = "inherits-from";
    public const string Implements = "implements";

    // Execution Relationships
    public const string Calls = "calls";
    public const string Creates = "creates";
    public const string Throws = "throws";

    // Control Flow Relationships
    public const string FlowsTo = "flows-to";
    public const string BranchesTo = "branches-to";
    public const string LoopsTo = "loops-to";

    // Dependency Relationships
    public const string DependsOn = "depends-on";
    public const string Imports = "imports";
    public const string References = "references";

    // Documentation Relationships
    public const string Documents = "documents";
    public const string Explains = "explains";

    // View Relationships
    public const string Aggregates = "aggregates";
    public const string FocusesOn = "focuses-on";
}

/// <summary>
/// Categories for edge types
/// </summary>
public static class EdgeCategories
{
    public const string Structure = "structure";
    public const string Execution = "execution";
    public const string ControlFlow = "control-flow";
    public const string Dependency = "dependency";
    public const string Documentation = "documentation";
    public const string View = "view";
}