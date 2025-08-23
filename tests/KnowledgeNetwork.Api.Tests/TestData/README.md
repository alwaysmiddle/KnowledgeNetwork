# API Test Data

Test data, fixtures, and samples for KnowledgeNetwork.Api testing.

## Purpose
Centralized location for API testing data:
- HTTP request/response samples
- Authentication tokens and user data
- API payload examples for various endpoints
- Performance test datasets
- Error scenario data for API testing

## Organization Structure
- `Requests/` - Sample HTTP request payloads and headers
- `Responses/` - Expected HTTP response data
- `Authentication/` - JWT tokens, user credentials, and auth data
- `PerformanceData/` - Large payloads for load testing
- `ErrorScenarios/` - Invalid requests and error response examples

## Data Categories
- **Request Data**: JSON payloads, query parameters, headers
- **Response Data**: Expected API responses and status codes
- **Authentication Data**: Tokens, credentials, user profiles
- **Performance Data**: Large datasets for scalability testing
- **Error Data**: Invalid payloads, malformed requests, edge cases

## Usage Guidelines
- Keep request/response data realistic and representative
- Use valid JSON schemas that match API contracts
- Include both successful and error scenarios
- Document API version compatibility for test data
- Keep authentication data secure and use test-only credentials
- Maintain consistency with OpenAPI specifications

## Current Status
API layer currently has minimal implementation - test data will be added as endpoints and contracts are developed.