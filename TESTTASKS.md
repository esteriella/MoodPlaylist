# MoodPlaylist API Test Tasks

This file is the verification gate for the API test work. A task is only marked complete after its verification command passes. Production business logic must not be changed merely to make a test pass; any discovered defect is recorded before a production fix is considered.

## Conventions

- Test controller behavior at the `IUnitOfWork` and `ISpotifyService` boundaries; do not instantiate repositories or `AppDbContext` yet.
- Test `SpotifyService` at its `HttpClient` boundary with a deterministic HTTP handler; do not require live Spotify credentials in the normal test suite.
- Use xUnit `[Fact(DisplayName = "...")]` (or `[Theory(DisplayName = "...")]`) on every test.
- Use PascalCase test method names following `Method_Scenario_ExpectedResult`.
- Keep Arrange, Act, and Assert clear and avoid shared mutable state.

## Tasks

- [x] Inventory API controllers, service boundaries, project target framework, and solution format.
  - Verification: source inventory completed; API targets .NET 10 and the solution uses `.slnx`.
- [x] Create a .NET 10 xUnit test project and add it to `src/MoodPlaylist.slnx`.
  - Verification: `dotnet sln src/MoodPlaylist.slnx list` contains the test project.
- [x] Add reusable test doubles/builders for repository, unit-of-work, authenticated controller context, and HTTP responses.
  - Verification: test project builds with no production data implementation referenced by test setup.
- [x] Test `AuthController` status/result forwarding and authenticated user propagation.
  - Verification: focused Auth controller tests pass.
- [x] Test every implemented `LibraryController` action at the controller boundary.
  - Verification: focused Library controller tests pass, including Spotify collaboration and controller-thrown validation paths.
- [x] Test `SpotifyService` request URI construction, successful response parsing, empty payload behavior, and error mapping.
  - Verification: focused Spotify service tests pass without external network access.
- [x] Run the complete solution test suite and inspect warnings/changes.
  - Verification: `dotnet test src/MoodPlaylist.slnx` passes and the test-work paths pass `git diff --check`.
- [x] Add PostgreSQL Testcontainers integration coverage while retaining mocked controller unit tests.
  - Verification: integration project builds and nine container tests are discovered; execution is wired to Docker-backed CI.

## Coverage Audit — 2026-08-02

- [x] Re-inventory controllers and services after recommendation, caching, playlist-query, bulk-save, and refresh changes.
- [x] Add recommendation validation coverage for seed limits, missing moods, unusable genres, and Spotify URL normalization.
- [x] Add playlist view coverage for `mine`, `others`, `all`, unauthenticated access, and invalid values.
- [x] Add empty bulk-track validation and playlist refresh failure coverage.
- [x] Add cache miss, hit, removal, and null-result behavior tests.
- [x] Re-run the full suite with Coverlet and verify test conventions.
  - Verification: 42 tests pass; all facts have display names; test-work paths pass `git diff --check`.
  - Coverage: Auth actions 100%, Library action methods 100%, Spotify service 100%, Cache service 100%, and Spotify HTTP registration 100% line coverage.
  - Intentional remainder: unused `BaseController` name/email helpers and private Library helper permutations are not tested solely to inflate coverage.

## New Services Audit — 2026-08-02

- [x] Inventory newly added Spotify client-credentials services and updated DI wiring.
- [x] Test Spotify token request method, endpoint, Basic credentials, and form body.
- [x] Test valid-token caching and concurrent request synchronization.
- [x] Test unsuccessful and missing-token authentication responses.
- [x] Test bearer-token attachment by `SpotifyAuthenticationHandler`.
- [x] Test active JWT creation, signing-key validation, claims, metadata, and refresh-token generation.
- [x] Run the complete suite with coverage and verify conventions.
  - Verification: 52 tests pass; all facts have display names; test-work paths pass `git diff --check`.
  - Coverage: every active class under `Services` and `HttpClientDI` has 100% line coverage.

## Upper-Layer Audit — 2026-08-02

- [x] Audit middleware, exception mapping, authentication registration, and active authentication helpers.
- [x] Test every exception-to-status/message mapping and internal-detail hiding.
- [x] Test exception middleware pass-through and downstream exception conversion.
- [x] Test JWT registration in development and production plus all missing required settings.
- [x] Test hash-secret configuration and generated public-code shape.
- [x] Run the complete suite and verify test naming/display-name conventions.
  - Verification: 72 tests pass; no facts/theories are missing display names; test-work paths pass `git diff --check`.
- [x] Test `AuthMiddleware` claim enrichment and malformed-token behavior with PostgreSQL Testcontainers.
- [ ] Consider application-factory smoke tests for the complete `Program.cs` pipeline after a test host configuration strategy is introduced.

## Intentional Exclusions

- `Database` startup remains part of the deferred Testcontainers scope.
- `Logging` startup is infrastructure configuration and has no distinct application contract requiring a unit test.
- `ExceptionExtensions` and unused `BaseController` name/email helpers have no active production consumer.
- Private Library helper permutations are covered through observable controller cases rather than direct private-method tests.

## Testcontainers Setup — 2026-08-02

- [x] Add `Testcontainers.PostgreSql` 4.13.0 to the xUnit project.
- [x] Add a collection-scoped PostgreSQL 17 fixture that starts a disposable container and applies EF Core migrations.
- [x] Reset mutable tables between integration tests while retaining migration-seeded moods.
- [x] Cover authentication registration/login/logout and duplicate/invalid credential paths using PostgreSQL.
- [x] Cover migrations, mood seed data, playlist filtering, and JSONB track add/find/remove operations.
- [x] Cover database-backed `AuthMiddleware` claim enrichment.
- [x] Add a `WebApplicationFactory<Program>` backed by the PostgreSQL fixture.
- [x] Exercise registration, login, model validation, middleware, controllers, and repositories through real HTTP requests and PostgreSQL.
- [x] Add a pull-request/manual `test-webapi.yml` workflow with separate unit and Testcontainers steps.
- [x] Gate WebAPI deployment on both unit and PostgreSQL Testcontainers test steps.
- [x] Fix JSONB defects exposed by integration design: array containment and removal of the final track.
- [x] Verify Release build and 72 non-integration tests locally.
- [x] Verify all nine integration tests compile and are discovered by the test runner.
- [ ] Execute the nine integration tests locally.
  - Blocked locally because Docker is not installed; GitHub's Ubuntu runner verifies Docker before running them.

## GitHub Testcontainers Failure Follow-up — 2026-08-02

- [x] Diagnose the four failed GitHub integration tests from the attached action log.
- [x] Replace the untranslatable case-insensitive playlist title comparison with PostgreSQL `ILIKE`.
- [x] Align JSONB queries with `Track`'s serialized lowercase `id` property.
- [x] Inject startup configuration through scoped process environment variables before `Program.cs` creates its builder.
- [x] Rebuild in Release and run all 72 non-integration tests successfully.
- [ ] Confirm all nine integration tests pass on the next Docker-backed GitHub run.

## Findings

- The working tree already contains an unrelated change in `src/MoodPlaylistWeb/app/dashboard/page.tsx`; test work must not alter it.
- A repository-wide `git diff --check` still reports trailing whitespace in that pre-existing frontend change; the test-work paths pass the whitespace check.
- Live Spotify connectivity belongs in an explicitly configured integration/smoke test, not the deterministic default suite. The initial suite verifies the configured HTTP contract and expected responses using an in-memory handler.
- The completed suite contains 72 passing tests after the upper-layer audit.
- Restore/build reports NU1903 for the API's transitive `Microsoft.OpenApi` 2.4.1 dependency. This pre-existing production dependency warning is not suppressed or changed as part of test work.
