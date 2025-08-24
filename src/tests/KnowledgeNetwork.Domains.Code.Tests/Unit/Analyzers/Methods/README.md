# Method Analyzer Tests

Unit tests for `CSharpMethodRelationshipAnalyzer` and method-level analysis functionality.

## Purpose
Test the analysis of C# method relationships, calls, and interactions.

## Production Code Under Test
- `CSharpMethodRelationshipAnalyzer.cs` - Analyzes method call relationships and interactions

## Test Focus Areas
- Method call graph construction
- Direct and indirect method invocations
- Constructor chaining analysis
- Field access from methods
- Method overriding and virtual calls
- Static vs instance method relationships
- Cross-class method dependencies

## Sample Test Scenarios
- Simple method call chains
- Recursive method calls
- Polymorphic method invocations
- Constructor chains and dependencies
- Methods accessing fields and properties
- Static method interactions
- Cross-assembly method calls
- Generic method relationships

## Test File Naming
`CSharpMethodRelationshipAnalyzerTests.cs`