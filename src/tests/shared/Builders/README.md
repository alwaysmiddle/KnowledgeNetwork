# Test Data Builders

Shared test data builders using the Builder pattern for creating test objects across all test projects.

## Purpose
Centralized collection of fluent builders for creating test data:
- Domain object builders with sensible defaults
- Complex object graph construction
- Test scenario-specific data generation
- Consistent test data across all test projects

## Builder Pattern Benefits
- **Readable Tests**: Fluent API makes test setup clear and maintainable
- **Flexibility**: Easy to customize specific properties while keeping defaults
- **Reusability**: Common builders shared across multiple test projects
- **Maintainability**: Changes to object structure only require builder updates

## Organization Structure
- Domain-specific builders grouped by aggregate root
- Base builder classes for common patterns
- Specialized builders for complex scenarios
- Extension methods for common test configurations

## Usage Example
```csharp
var codeNode = new CodeNodeBuilder()
    .WithName("TestMethod")
    .WithType(NodeType.Method)
    .WithComplexity(5)
    .Build();

var analysisResult = new AnalysisResultBuilder()
    .WithSourceFile("Test.cs")
    .AddNode(codeNode)
    .WithSuccessfulAnalysis()
    .Build();
```

## Current Status
Builders will be added as domain objects and test scenarios are developed across the test projects.