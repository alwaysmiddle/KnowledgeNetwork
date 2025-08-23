# Analyzer Integration Tests

Integration tests for analyzer components working together with real Roslyn APIs.

## Purpose
Test analyzers with real C# code and actual Roslyn compilation:
- End-to-end analyzer workflows
- Real Roslyn syntax trees and semantic models
- Cross-analyzer interactions
- Performance with realistic code samples
- Integration with file system and compilation units

## Integration Scenarios
- Complete code analysis workflows using real C# files
- Multi-analyzer coordination (classes + methods + files)
- Large codebase processing
- Cross-project analysis scenarios
- Error recovery with malformed real code

## Subfolders
- `Blocks/` - Integration tests for block analysis with real methods
- `Classes/` - Integration tests for class analysis with real inheritance
- `Files/` - Integration tests for file analysis with real projects
- `Methods/` - Integration tests for method analysis with real call graphs

## Testing Approach
- Use real C# code samples from test-samples/
- Test with actual Roslyn compilation units
- Validate complete analysis pipelines
- Measure performance with realistic data volumes