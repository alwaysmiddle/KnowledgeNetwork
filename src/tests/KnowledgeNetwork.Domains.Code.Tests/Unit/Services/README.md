# Service Unit Tests

Unit tests for service components in KnowledgeNetwork.Domains.Code.

## Purpose
Test the main service layer that orchestrates code analysis:
- `CSharpAnalysisService` - Primary C# code analysis service
- Future services for other language support

## CSharpAnalysisService Testing
Test the core service that:
- Orchestrates Roslyn-based C# analysis
- Extracts classes, methods, properties, using statements
- Performs Control Flow Graph analysis
- Creates compilation units for semantic analysis
- Handles error scenarios gracefully

## Testing Focus
- Service orchestration logic
- Integration with Roslyn APIs
- Error handling and recovery
- Performance characteristics
- Mock external dependencies (file system, etc.)
- Validate complete analysis workflow

## Testing Approach
- Mock Roslyn dependencies where appropriate
- Use real C# code samples for integration scenarios
- Test both successful and error pathways
- Validate output completeness and accuracy