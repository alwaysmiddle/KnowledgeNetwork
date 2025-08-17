using KnowledgeNetwork.Core.Models.Core;

namespace KnowledgeNetwork.Core.Models.Constants;

/// <summary>
/// Predefined bidirectional relationship types with categories
/// </summary>
public static class RelationshipTypes
{
    // Structural Relationships
    public static readonly RelationshipType Contains = new()
    {
        Forward = "contains",
        Reverse = "contained-by",
        Category = "structure"
    };

    public static readonly RelationshipType Inherits = new()
    {
        Forward = "inherits-from",
        Reverse = "inherited-by",
        Category = "structure"
    };

    public static readonly RelationshipType Implements = new()
    {
        Forward = "implements",
        Reverse = "implemented-by",
        Category = "structure"
    };

    // Execution Relationships
    public static readonly RelationshipType Calls = new()
    {
        Forward = "calls",
        Reverse = "called-by",
        Category = "execution"
    };

    public static readonly RelationshipType Creates = new()
    {
        Forward = "creates",
        Reverse = "created-by",
        Category = "execution"
    };

    public static readonly RelationshipType Throws = new()
    {
        Forward = "throws",
        Reverse = "thrown-by",
        Category = "execution"
    };

    // Control Flow Relationships
    public static readonly RelationshipType FlowsTo = new()
    {
        Forward = "flows-to",
        Reverse = "flows-from",
        Category = "control-flow"
    };

    public static readonly RelationshipType BranchesTo = new()
    {
        Forward = "branches-to",
        Reverse = "branches-from",
        Category = "control-flow"
    };

    public static readonly RelationshipType LoopsTo = new()
    {
        Forward = "loops-to",
        Reverse = "loops-from",
        Category = "control-flow"
    };

    // Dependency Relationships
    public static readonly RelationshipType DependsOn = new()
    {
        Forward = "depends-on",
        Reverse = "dependency-of",
        Category = "dependency"
    };

    public static readonly RelationshipType Imports = new()
    {
        Forward = "imports",
        Reverse = "imported-by",
        Category = "dependency"
    };

    public static readonly RelationshipType References = new()
    {
        Forward = "references",
        Reverse = "referenced-by",
        Category = "dependency"
    };

    // Documentation Relationships
    public static readonly RelationshipType Documents = new()
    {
        Forward = "documents",
        Reverse = "documented-by",
        Category = "documentation"
    };

    public static readonly RelationshipType Explains = new()
    {
        Forward = "explains",
        Reverse = "explained-by",
        Category = "documentation"
    };

    // View Relationships
    public static readonly RelationshipType Aggregates = new()
    {
        Forward = "aggregates",
        Reverse = "aggregated-by",
        Category = "view"
    };

    public static readonly RelationshipType FocusesOn = new()
    {
        Forward = "focuses-on",
        Reverse = "focused-by",
        Category = "view"
    };
}