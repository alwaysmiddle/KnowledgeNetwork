# File Analyzer Tests

Unit tests for `CSharpFileDependencyAnalyzer` and file-level dependency analysis functionality.

## Purpose
Test the analysis of C# file dependencies, using statements, and cross-file relationships.

## Production Code Under Test
- `CSharpFileDependencyAnalyzer.cs` - Analyzes file-level dependencies and references

## Test Focus Areas
- Using statement analysis and extraction
- Assembly reference dependencies
- Project reference analysis
- Namespace dependency mapping
- Type reference tracking across files
- External library dependencies
- File-to-file relationship analysis

## Sample Test Scenarios
- Files with multiple using statements
- Cross-project type references
- External NuGet package dependencies
- Namespace organization patterns
- Circular file dependencies
- Missing reference detection
- Assembly boundary analysis

## Test File Naming
`CSharpFileDependencyAnalyzerTests.cs`