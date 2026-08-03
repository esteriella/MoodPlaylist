# Test Work Memory

## Current Scope

- API under test: `src/MoodPlaylistApi/MoodPlaylistApi.csproj`
- Test layer: controllers, with repositories mocked behind `IUnitOfWork`
- External service: `SpotifyService`, tested through `HttpClient`
- Integration layer: real PostgreSQL 17 through Testcontainers and `WebApplicationFactory`

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
- 74 non-integration tests pass on 2026-08-03 after adding the playback contract.
- Current covered behavior includes all implemented controller actions, recommendation seed validation/normalization, playlist view authorization/filtering, bulk-save validation, refresh failures, Spotify response/request behavior, caching, and typed-client registration.
- Coverlet reports 100% line coverage for Auth actions, every Library action state machine, Spotify, Cache, and `HttpClientDI`. Overall Library class coverage is 95.91% because some private helper permutations remain; `BaseController` is 60% because its unused name/email accessors have no public consumer.
- Spotify now uses client-credentials authentication through `SpotifyTokenService` and `SpotifyAuthenticationHandler`; tests cover request shape, failures, caching, concurrency, and bearer attachment.
- The active `Jwt` service is covered for signed access-token output, identity claims, weak-key rejection, and random refresh-token generation.
- Final service coverage reports 100% line coverage for `Jwt`, `CacheService`, `SpotifyService`, `SpotifyTokenService`, `SpotifyAuthenticationHandler`, and `HttpClientDI`.
- Exception mapping, exception middleware, JWT DI validation/environment behavior, hash-secret configuration, and code generation now have focused tests.
- `AuthMiddleware` claim enrichment and malformed-token behavior are covered with PostgreSQL Testcontainers because the middleware directly uses `AppDbContext`.
- `WebApplicationFactory<Program>` now exercises registration, login, validation, middleware, controllers, repositories, and EF migrations through real HTTP requests.
- PostgreSQL integration tests use `Testcontainers.PostgreSql` 4.13.0 with `postgres:17-alpine`, apply real migrations once, and truncate mutable `Playlists`/`Users` data between tests.
- Nine integration tests are tagged `Category=Integration`; 74 tests run under `Category!=Integration`.
- `.github/workflows/test-webapi.yml` runs on API pull requests/manual dispatch. `.github/workflows/deploy-webapi.yml` runs both test categories before image build and deployment.
- Local Docker is unavailable on the current machine, so integration execution is CI-verified; local Release compilation and integration discovery pass.
- JSONB fixes made during integration setup: `ExistsAsync` now uses array containment, and `RemoveTrack` coalesces an empty aggregate to `[]` instead of violating the non-null column.
- `MoodPlaylistApiFactory` replaces the application's database registration with the fixture connection string and supplies safe test configuration. HTTP-level auth tests now exercise the complete ASP.NET pipeline against PostgreSQL.
- Direct controller tests intentionally retain repository mocks for fast upper-layer isolation; HTTP integration tests provide the container-backed controller coverage.
- First GitHub Testcontainers run passed five and failed four tests. Root causes were an EF-untranslatable `StringComparison` query, uppercase JSONB `Id` queries against lowercase serialized `id`, and test-host configuration being added after `Program.cs` required the connection string.
- Fixes now use PostgreSQL `ILIKE` and lowercase JSONB property names. Because minimal-hosting executes `Program.cs` before `WebApplicationFactory.CreateHost` configuration is applied, `MoodPlaylistApiFactory` now sets required environment configuration in its constructor and restores previous values on disposal. Docker-backed confirmation is pending the next GitHub run.
- Default non-integration tests do not contact Spotify or PostgreSQL; integration tests start disposable PostgreSQL and still mock external Spotify connectivity.
- Live Spotify smoke testing remains deferred.
- Track responses expose computed, URL-escaped Spotify embed and external playback links. DTO tests cover both the public JSON contract and unsafe path-character escaping.
- The solution currently emits NU1903 for transitive `Microsoft.OpenApi` 2.4.1; this is production dependency maintenance, not a test failure.
