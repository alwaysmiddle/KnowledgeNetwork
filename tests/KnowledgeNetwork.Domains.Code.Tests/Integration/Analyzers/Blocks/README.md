# Block Analyzer Integration Tests

Integration tests for `CSharpMethodBlockAnalyzer` with real C# code and Roslyn compilation.

## Purpose
Test block analysis with complete compilation units and real method implementations.

## Integration Focus
- Real method block analysis using actual C# syntax trees
- Integration with Roslyn semantic models
- Performance testing with complex real-world methods
- End-to-end control flow analysis workflows
- Validation against known complex method structures

## Real Code Scenarios
- Complex methods from actual production codebases
- Methods with nested loops and conditions
- Async/await method patterns
- Exception handling in real methods
- Performance-critical method analysis

## Test File Naming
`CSharpMethodBlockAnalyzerIntegrationTests.cs`