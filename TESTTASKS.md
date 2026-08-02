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
- [ ] Later: replace repository mocks with PostgreSQL Testcontainers integration coverage.

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

## Findings

- The working tree already contains an unrelated change in `src/MoodPlaylistWeb/app/dashboard/page.tsx`; test work must not alter it.
- A repository-wide `git diff --check` still reports trailing whitespace in that pre-existing frontend change; the test-work paths pass the whitespace check.
- Live Spotify connectivity belongs in an explicitly configured integration/smoke test, not the deterministic default suite. The initial suite verifies the configured HTTP contract and expected responses using an in-memory handler.
- The completed suite contains 42 passing tests after the second coverage audit.
- Restore/build reports NU1903 for the API's transitive `Microsoft.OpenApi` 2.4.1 dependency. This pre-existing production dependency warning is not suppressed or changed as part of test work.
