# Test Work Memory

## Current Scope

- API under test: `src/MoodPlaylistApi/MoodPlaylistApi.csproj`
- Test layer: controllers, with repositories mocked behind `IUnitOfWork`
- External service: `SpotifyService`, tested through `HttpClient`
- Deferred scope: real PostgreSQL/Testcontainers repository integration tests

## Persistent Constraints

- Do not modify business logic simply to satisfy tests.
- Every fact/theory has a descriptive display name.
- Test method names use PascalCase.
- Tests must be deterministic and independent by default.
- Preserve unrelated user changes.

## Current State

- API inventory completed on 2026-08-02.
- `src/MoodPlaylistApi.Tests` was created and added to `src/MoodPlaylist.slnx`.
- `MoodPlaylistApi` targets `net10.0`.
- Controllers depend on `IUnitOfWork`; `LibraryController` also depends on `ISpotifyService`.
- `SpotifyService` receives a typed `HttpClient` configured by `HttpClientDI`.
- 42 tests pass on 2026-08-02 after a second missing-test audit.
- Current covered behavior includes all implemented controller actions, recommendation seed validation/normalization, playlist view authorization/filtering, bulk-save validation, refresh failures, Spotify response/request behavior, caching, and typed-client registration.
- Coverlet reports 100% line coverage for Auth actions, every Library action state machine, Spotify, Cache, and `HttpClientDI`. Overall Library class coverage is 95.91% because some private helper permutations remain; `BaseController` is 60% because its unused name/email accessors have no public consumer.
- Default tests do not contact Spotify or PostgreSQL.
- A live Spotify smoke test and PostgreSQL Testcontainers coverage remain deferred.
- The solution currently emits NU1903 for transitive `Microsoft.OpenApi` 2.4.1; this is production dependency maintenance, not a test failure.
