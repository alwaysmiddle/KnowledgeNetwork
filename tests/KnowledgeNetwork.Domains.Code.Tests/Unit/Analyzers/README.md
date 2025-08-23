# Analyzer Unit Tests

Unit tests for all analyzer components in KnowledgeNetwork.Domains.Code.

## Purpose
Test analyzer components that extract knowledge from C# code:
- Block analyzers (method block structure)
- Class analyzers (relationships, inheritance)
- File analyzers (dependencies, references)
- Method analyzers (call relationships)

## What Goes Here
- `Blocks/` - Tests for CSharpMethodBlockAnalyzer
- `Classes/` - Tests for CSharpClassRelationshipAnalyzer  
- `Files/` - Tests for CSharpFileDependencyAnalyzer
- `Methods/` - Tests for CSharpMethodRelationshipAnalyzer

## Testing Focus
- Individual analyzer logic in isolation
- Mock Roslyn syntax trees and semantic models
- Verify correct extraction of relationships and metadata
- Test edge cases and error handling
- Performance validation for large code structures