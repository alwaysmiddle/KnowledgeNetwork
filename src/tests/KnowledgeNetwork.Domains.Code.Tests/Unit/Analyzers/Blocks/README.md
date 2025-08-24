# Block Analyzer Tests

Unit tests for `CSharpMethodBlockAnalyzer` and related block analysis functionality.

## Purpose
Test the analysis of method block structures and control flow within C# methods.

## Production Code Under Test
- `CSharpMethodBlockAnalyzer.cs` - Analyzes method blocks and control flow patterns

## Test Focus Areas
- Basic block identification within methods
- Control flow analysis between blocks  
- Loop and conditional block structures
- Exception handling blocks
- Method complexity calculation based on blocks
- Edge cases with nested structures

## Sample Test Scenarios
- Simple linear method blocks
- Methods with if/else branches
- Methods with loops (for, while, foreach)
- Methods with try/catch/finally blocks
- Complex nested control structures
- Empty methods and edge cases

## Test File Naming
`CSharpMethodBlockAnalyzerTests.cs`