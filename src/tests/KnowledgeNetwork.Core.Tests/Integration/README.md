# Integration Tests

Integration tests for KnowledgeNetwork.Core components working together.

## Purpose
Test how Core components interact with each other and external dependencies:
- Cross-component workflows
- Data validation across model boundaries
- Integration with infrastructure components
- End-to-end domain logic scenarios

## Testing Approach
- Test real interactions between components
- Use TestServer for API integration when needed
- Use in-memory databases for data-related tests
- Focus on integration points and workflows
- Validate complete business scenarios

## Examples
- Complete knowledge graph construction workflows
- Edge-to-node relationship validation
- Cross-domain data mapping scenarios