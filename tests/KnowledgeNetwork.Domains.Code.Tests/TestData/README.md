# Test Data for Domains.Code Tests

Project-specific test data, fixtures, and samples for KnowledgeNetwork.Domains.Code testing.

## Purpose
Centralized location for C# code analysis test data:
- Sample C# code files for analysis testing
- Expected analysis results for validation
- Complex code scenarios for edge case testing
- Performance test datasets
- Mock data for specific analysis components

## Organization Structure
- `SampleCode/` - C# source code samples for analysis
- `ExpectedResults/` - JSON files with expected analysis outputs
- `ComplexScenarios/` - Large or complex code samples
- `EdgeCases/` - Unusual or problematic code samples
- `PerformanceData/` - Large datasets for performance testing

## Data Categories
- **Simple Code**: Basic classes, methods, properties for unit tests
- **Complex Code**: Real-world scenarios with inheritance, generics, etc.
- **Edge Cases**: Malformed, incomplete, or unusual code patterns
- **Integration Data**: Complete projects for integration testing
- **Expected Results**: Known correct outputs for validation

## Usage Guidelines
- Keep test data focused and minimal
- Use descriptive filenames that indicate test scenario
- Include both positive and negative test cases
- Document expected results alongside sample code