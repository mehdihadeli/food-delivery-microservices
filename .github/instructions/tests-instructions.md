---
apply: always
---

# Tests Rules

## Testing Tools

- Use `Xunit3` as .Net test framrwork.
- Use `Shouldly` as test assertion Tools.
- Use `NSubstitute` for mocking in `unit Tests`.
- Use `WireMock.Net` for mocking network call on wire mock server and use for `Integration Tests`.
- Use `richardszalay.mockhttp` for mocking http client calls and simulating requests and responses in the `UnitTests`.

## Testing Standards

- **Test Naming Convention**: Use `MethodName_[When]Condition_[Should]ExpectedResult()` pattern. for example test names should be:
	- ExistsAsync_WhenGroupDoesNotExist_ShouldReturnFalse
	- GetGroupsByPageAsync_WhenPageNumberAndSizeAreValid_ShouldReturnPagedResults
- **Unit Tests**: Focus on domain logic and business rules in isolation.
- **Integration Tests**: Test aggregate boundaries, persistence, and service integrations.
- **EndToEnd Tests**: Validate complete user scenarios.
- **Test Coverage**: Minimum 85% for domain and application layers.

## Testing Guidelines

- Add `Act`, `Arrange`, `Assert` as comment in each test cases.
- Add some tests for `happy` and `unhappy path` for class under test to reaching test coverage Minimum 85%.