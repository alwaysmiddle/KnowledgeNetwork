using System.Collections.Generic;

namespace KnowledgeNetwork.Api.Models.Analysis
{
    /// <summary>
    /// Lightweight representation of a type (class, interface, struct, enum)
    /// </summary>
    public record TypeInfo
    {
        public required string QualifiedName { get; init; }
        public required string Name { get; init; }
        public required string Namespace { get; init; }
        public required string Kind { get; init; } // "class", "interface", "struct", "enum"
        public required SourceLocation Location { get; init; }
        public required IReadOnlyList<string> BaseTypes { get; init; }
        public required IReadOnlyList<string> ImplementedInterfaces { get; init; }
        public required string AccessModifier { get; init; }
        public required bool IsAbstract { get; init; }
        public required bool IsSealed { get; init; }
        public required bool IsStatic { get; init; }
        public required bool IsPartial { get; init; }
        public required bool IsGeneric { get; init; }
        public required int GenericParameterCount { get; init; }
    }
    
    /// <summary>
    /// Lightweight representation of a method or function
    /// </summary>
    public record MethodInfo
    {
        public required string QualifiedName { get; init; }
        public required string Name { get; init; }
        public required string ContainingType { get; init; }
        public required string ReturnType { get; init; }
        public required SourceLocation Location { get; init; }
        public required IReadOnlyList<ParameterInfo> Parameters { get; init; }
        public required string AccessModifier { get; init; }
        public required bool IsStatic { get; init; }
        public required bool IsAbstract { get; init; }
        public required bool IsVirtual { get; init; }
        public required bool IsOverride { get; init; }
        public required bool IsAsync { get; init; }
        public required bool IsGeneric { get; init; }
        public required int GenericParameterCount { get; init; }
    }
    
    /// <summary>
    /// Lightweight representation of a property
    /// </summary>
    public record PropertyInfo
    {
        public required string QualifiedName { get; init; }
        public required string Name { get; init; }
        public required string ContainingType { get; init; }
        public required string Type { get; init; }
        public required SourceLocation Location { get; init; }
        public required string AccessModifier { get; init; }
        public required bool IsStatic { get; init; }
        public required bool IsAbstract { get; init; }
        public required bool IsVirtual { get; init; }
        public required bool IsOverride { get; init; }
        public required bool HasGetter { get; init; }
        public required bool HasSetter { get; init; }
        public required bool IsReadOnly { get; init; }
    }
    
    /// <summary>
    /// Lightweight representation of a field
    /// </summary>
    public record FieldInfo
    {
        public required string QualifiedName { get; init; }
        public required string Name { get; init; }
        public required string ContainingType { get; init; }
        public required string Type { get; init; }
        public required SourceLocation Location { get; init; }
        public required string AccessModifier { get; init; }
        public required bool IsStatic { get; init; }
        public required bool IsReadOnly { get; init; }
        public required bool IsConst { get; init; }
        public required bool IsVolatile { get; init; }
    }
    
    /// <summary>
    /// Parameter information for methods
    /// </summary>
    public record ParameterInfo
    {
        public required string Name { get; init; }
        public required string Type { get; init; }
        public required bool HasDefaultValue { get; init; }
        public required string? DefaultValue { get; init; }
        public required string Modifier { get; init; } // "ref", "out", "in", "params", ""
        public required int Position { get; init; }
    }
    
    /// <summary>
    /// Represents a relationship between code elements
    /// </summary>
    public record SymbolRelationship
    {
        public required string SourceId { get; init; }
        public required string TargetId { get; init; }
        public required string RelationType { get; init; } // "inherits", "implements", "calls", "references"
        public required SourceLocation? Location { get; init; } // Where the relationship is established
    }
}