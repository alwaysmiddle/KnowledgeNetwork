# C# Sample Code for Testing

C# source code files used for testing analysis components.

## Purpose
Real C# code samples for testing various analysis scenarios:
- Simple classes and methods for unit tests
- Complex inheritance hierarchies
- Control flow graph test cases
- Method relationship examples
- File dependency scenarios

## Organization by Test Type
- `SimpleClasses/` - Basic class structures for unit testing
- `Inheritance/` - Class inheritance and interface implementation examples
- `ControlFlow/` - Methods with complex control flow for CFG testing
- `Dependencies/` - Files with various using statements and references
- `MethodCalls/` - Complex method call chains and relationships
- `EdgeCases/` - Problematic or unusual code patterns

## Naming Convention
- `{TestScenario}_{Description}.cs` - e.g., `Inheritance_MultipleInterfaces.cs`
- `Simple_{Component}.cs` - for basic testing scenarios
- `Complex_{Component}.cs` - for comprehensive testing scenarios
- `Edge_{Issue}.cs` - for edge case and error testing

## Usage in Tests
- Reference these files in test methods using relative paths
- Use with real Roslyn compilation for integration tests
- Combine with expected results for validation testing
- Keep samples focused on specific analysis scenarios