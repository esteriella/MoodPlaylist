# Test Design Decisions

## TD-001: Controller Boundary

Controller tests invoke actions directly and replace `IUnitOfWork`, repositories, and `ISpotifyService` with test doubles. This keeps EF Core and repository behavior out of the upper-layer suite.

## TD-002: Spotify Boundary

The default `SpotifyService` tests use a recording `HttpMessageHandler`. This verifies the outbound method/URI and maps representative Spotify responses without relying on network availability, credentials, rate limits, or mutable third-party data.

## TD-003: Live Connectivity

A live Spotify smoke test is deferred until test credentials and an explicit opt-in execution policy are available. It should be separated from the normal test run to avoid flaky or secret-dependent CI.

## TD-004: Production Changes

Tests should characterize the current public behavior. If they expose a production defect, record the failure and fix the root cause explicitly rather than weakening the assertion or adding test-only branches to production code.

## TD-005: Coverage Targets

Coverage is used to find missing contracts, not as a reason to test private implementation details. Public controller actions and service behavior should be fully exercised; unused protected helpers and exhaustive private-helper permutations may remain uncovered when they add no distinct observable contract.
