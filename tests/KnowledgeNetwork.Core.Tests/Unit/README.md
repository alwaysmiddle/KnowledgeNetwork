# Unit Tests

Unit tests for KnowledgeNetwork.Core domain models and business logic.

## Purpose
Test individual components in isolation with mocked dependencies:
- Domain models (KnowledgeNode, KnowledgeEdge, NodeType, etc.)
- Core business logic and validation
- Constants and enums
- Model relationships and constraints

## Testing Approach
- Use xUnit v3 for test framework
- Use Shouldly for assertions
- Use Bogus for realistic test data generation
- Use Moq for dependency mocking
- Follow AAA pattern (Arrange-Act-Assert)

## Naming Convention
`ClassNameTests.cs` for testing individual classes
`MethodName_Scenario_ExpectedOutcome` for test methods