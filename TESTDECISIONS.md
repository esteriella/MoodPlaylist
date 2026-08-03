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

## TD-006: Token Service Concurrency

Spotify token tests verify that concurrent callers share one authentication request. This protects the semaphore and double-checked cache contract without introducing timing sleeps or live Spotify calls.

## TD-007: Static JWT Settings

JWT tests run in a non-parallel xUnit collection because `JwtSettingsHelper` stores process-wide static configuration. Tests restore valid settings after weak-key scenarios to prevent cross-test state leakage.

## TD-008: Database-Coupled Middleware

`AuthMiddleware` coverage is deferred to the PostgreSQL Testcontainers phase. It directly reads and updates `AppDbContext`; an EF in-memory substitute would hide relational/provider differences and conflict with the planned database testing boundary.

## TD-009: Exception Contract

Exception tests assert serialized JSON fields instead of deserializing back into `ApiResponse<T>`. The response type intentionally has private constructors, and production types should not be opened solely for tests.

## TD-010: PostgreSQL Testcontainers

Repository and database-coupled middleware tests use a real disposable PostgreSQL 17 container and real EF migrations. The container is shared per test collection for speed, while mutable tables are truncated before each test for isolation. Controller tests continue mocking repositories because their intended boundary remains the upper layer.

## TD-011: CI Test Separation

Unit and integration tests are selected with the `Category` trait. Both the pull-request test workflow and deployment workflow run them as separate required steps, making Docker failures and application failures easy to distinguish while ensuring deployment cannot bypass database integration tests.

## TD-012: Two Controller Test Layers

Direct controller tests keep mocked repository boundaries for fast, precise behavior checks. Separate `WebApplicationFactory` tests run controller routes through the real middleware, model validation, repositories, EF migrations, and PostgreSQL Testcontainer. This provides database realism without making every controller unit test slow or broad.

## TD-013: PostgreSQL-Specific Queries

Case-insensitive database comparisons use `EF.Functions.ILike`, which Npgsql translates to PostgreSQL `ILIKE`. JSONB queries use the wire property names defined by `JsonPropertyName`; for `Track.Id`, that name is lowercase `id`.

## TD-014: Test Host Configuration Timing

Configuration required by top-level `Program.cs` is supplied as environment variables before `WebApplicationFactory` starts the entry point. `CreateHost` configuration is too late for values read immediately after `WebApplication.CreateBuilder`. The PostgreSQL test collection is non-parallel, and the factory restores previous environment values when disposed. Service replacement in `ConfigureWebHost` remains appropriate for swapping the registered `AppDbContext` after startup configuration is available.

## TD-015: Playback Link Contract

Spotify playback destinations are computed by the API from the trusted track ID rather than accepted from client input or persisted separately. DTO tests assert the serialized `playback` contract and URL escaping. Browser component tests separately verify iframe rendering, the external Spotify fallback, close behavior, and the no-selection state without contacting Spotify.
