# Converter Unit Tests

Unit tests for data conversion components in KnowledgeNetwork.Domains.Code.

## Purpose
Test converters that transform between different data representations:
- Roslyn data structures to domain models
- Control Flow Graph conversions
- Analysis results to knowledge graph formats
- Domain-specific model transformations

## Current Converters
- `CfgToKnowledgeNodeConverter` - Converts Control Flow Graphs to KnowledgeNode format

## Testing Focus
- Accurate data transformation without loss
- Proper mapping of all relevant properties
- Edge case handling (null values, empty collections)
- Performance with large datasets
- Validation of output format consistency
- Error handling for malformed input data

## Testing Approach
- Create sample input data with known expected outputs
- Test both simple and complex transformation scenarios
- Verify bidirectional conversion accuracy where applicable