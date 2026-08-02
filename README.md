# MoodPlaylist

MoodPlaylist is a full-stack music discovery and playlist-management application. A listener selects one or more moods, the API translates those choices into Spotify genre seeds and audio-feature targets, and the application returns recommendations that can be saved into a personal playlist library.

The project demonstrates end-to-end product engineering across a typed React interface, a layered ASP.NET Core API, PostgreSQL persistence, third-party API integration, authentication, observability, automated testing, containerization, and path-based deployment workflows.

## Highlights

- Mood-driven music discovery backed by Spotify recommendations
- Multi-mood recommendation blending across genre and audio-feature constraints
- Spotify track, URL, and URI seed normalization
- Account registration, login, logout, and JWT-protected operations
- Personal playlist creation, editing, filtering, refresh, and track management
- Public discovery of playlists created by other users
- One-hour in-memory caching for mood recommendation results
- PostgreSQL persistence through Entity Framework Core
- Consistent API responses, validation, and centralized exception handling
- Per-user and per-IP fixed-window rate limiting
- Database and Spotify health checks
- Structured Serilog logging, correlation IDs, and development tracing
- Interactive Swagger UI and Scalar API reference documentation
- Deterministic frontend and backend test suites
- PostgreSQL integration testing with disposable Testcontainers databases
- Docker packaging and automated Vercel/GHCR/Render deployment workflows

## Technology stack

| Area | Technology |
| --- | --- |
| Web client | Next.js 16, React 19, TypeScript, Tailwind CSS 4 |
| Client validation | Zod |
| Client testing | Vitest, Testing Library, jsdom |
| API | ASP.NET Core 10, C# 14 |
| Data access | Entity Framework Core 10, Npgsql |
| Database | PostgreSQL |
| Authentication | JWT bearer authentication, hashed credentials, refresh tokens |
| External integration | Spotify Web API through typed `HttpClient` clients and OAuth client credentials |
| API testing | xUnit, Moq, Coverlet |
| Operations | Docker, Serilog, ASP.NET Core health checks and rate limiting |
| Delivery | GitHub Actions, GHCR, Render, Vercel |

## Features

### Mood-based recommendations

Moods are stored with Spotify genre seeds and JSON-based audio-feature constraints. Recommendation requests can combine mood IDs and Spotify track seeds, with a maximum of five combined seeds to respect Spotify's contract. When multiple moods are selected, the API distributes available genre slots across them and averages matching audio-feature targets.

Track seeds accept a raw Spotify ID, a `spotify:track:` URI, or an `open.spotify.com/track/...` URL. Recommendation requests also support a result limit from 1 to 100 and an optional two-letter market code.

### Playlist library

Authenticated users can:

- create and update playlists;
- save multiple recommended tracks at once;
- refresh a mood-linked playlist with new recommendations;
- remove tracks and check whether a track is already saved; and
- browse their own playlists, other users' playlists, or all playlists.

Playlist queries support pagination, sort direction, mood filtering, and creator-tag filtering. Public users may browse `others` or `all`; the `mine` view requires authentication.

### Reliability and security

- JWT bearer validation protects library mutations and user-specific reads.
- Anonymous traffic is limited to 10 requests per minute; authenticated users receive 20 requests per minute. Each partition may queue two requests.
- A global exception middleware maps domain and integration failures into a shared API response shape.
- Model-validation failures include a traceable error ID for support and diagnosis.
- PostgreSQL operations use retry-on-failure and a 60-second command timeout.
- `/health` reports application metadata and checks PostgreSQL and Spotify availability.
- Serilog enriches logs with service and environment context and writes console output plus rolling development log files.

## Architecture

```text
Browser
  |
  v
Next.js application
  |-- App Router pages and reusable UI components
  |-- Auth context and typed API clients
  |-- Zod/runtime error handling
  |
  v
ASP.NET Core API
  |-- Controllers: HTTP contract and authorization
  |-- Services: Spotify integration and caching
  |-- Unit of Work / repositories: business persistence operations
  |-- Middleware: authentication enrichment and exception mapping
  |
  +----> PostgreSQL (users, moods, playlists, tracks)
  |
  +----> Spotify Web API (search and recommendations)
```

The API follows dependency-injection and repository/unit-of-work patterns. Controllers depend on interfaces rather than concrete storage implementations, which allows controller behavior to be tested without a live database. The Spotify service is isolated behind `ISpotifyService` and a typed `HttpClient`, making its outbound contract deterministic in tests.

## Repository structure

```text
MoodPlaylist/
|-- .github/workflows/          # Frontend and API deployment automation
|-- src/
|   |-- MoodPlaylist.slnx       # .NET solution
|   |-- MoodPlaylistApi/        # ASP.NET Core API and EF Core migrations
|   |-- MoodPlaylistApi.Tests/  # xUnit API tests and test support
|   `-- MoodPlaylistWeb/        # Next.js application and Vitest tests
|-- TESTTASKS.md                # Backend verification checklist and audit
|-- TESTDECISIONS.md            # Test architecture decisions
|-- TESTMEMORY.md               # Test-suite maintenance context
`-- LICENSE.txt
```

## Prerequisites

Install the following before running the complete application:

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 22](https://nodejs.org/) (the deployment workflow uses 22.13)
- [pnpm](https://pnpm.io/installation)
- PostgreSQL
- A Spotify API credential/token accepted by the configured Spotify endpoint
- Docker, when running PostgreSQL integration tests or building the API image

## Local configuration

The repository intentionally does not commit secrets. Configure the API from `src/MoodPlaylistApi` with .NET user secrets:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=moodplaylist;Username=postgres;Password=YOUR_PASSWORD"
dotnet user-secrets set "Jwt:Issuer" "MoodPlaylistApi"
dotnet user-secrets set "Jwt:Audience" "MoodPlaylistWeb"
dotnet user-secrets set "Jwt:Key" "REPLACE_WITH_A_LONG_RANDOM_SIGNING_KEY"
dotnet user-secrets set "Jwt:MaxAge" "10"
dotnet user-secrets set "Jwt:MaxRefreshAge" "20"
dotnet user-secrets set "HashHelper:SecretKey" "REPLACE_WITH_A_RANDOM_HASHING_SECRET"
dotnet user-secrets set "Spotify:BaseUrl" "https://api.spotify.com/"
dotnet user-secrets set "Spotify:AccountsBaseUrl" "https://accounts.spotify.com/"
dotnet user-secrets set "Spotify:ClientId" "YOUR_SPOTIFY_CLIENT_ID"
dotnet user-secrets set "Spotify:ClientSecret" "YOUR_SPOTIFY_CLIENT_SECRET"
```

The API exchanges the Spotify client ID and secret at the Accounts API, caches the returned access token until shortly before expiry, and attaches it to Spotify API requests through a delegating handler. Keep both credentials in user secrets locally and managed secret storage in deployed environments.

Create `src/MoodPlaylistWeb/.env.local` for the web client:

```dotenv
NEXT_PUBLIC_API_BASE_URL=https://localhost:7115
```

The API allows the local frontend origin `http://localhost:3000`. When using the HTTPS API profile for the first time, trust the local development certificate:

```powershell
dotnet dev-certs https --trust
```

Do not commit `.env.local`, database passwords, JWT keys, hashing secrets, Spotify tokens, Vercel tokens, or deployment hooks.

## Database setup

Create the PostgreSQL database named in your connection string, then apply the committed migration:

```powershell
cd src/MoodPlaylistApi
dotnet tool install --global dotnet-ef
dotnet ef database update
```

If `dotnet-ef` is already installed, update it with `dotnet tool update --global dotnet-ef`. The committed initial migration creates the schema and inserts repeatable mood seed data, including Spotify genres and audio-feature constraints.

## Run the application

Use two terminals from the repository root.

Terminal 1 — API:

```powershell
dotnet restore src/MoodPlaylist.slnx
dotnet run --project src/MoodPlaylistApi --launch-profile https
```

The API listens on `https://localhost:7115` and `http://localhost:5015`. API documentation is available at:

- Swagger UI: `https://localhost:7115/swagger`
- Scalar reference: `https://localhost:7115/scalar`
- OpenAPI JSON: `https://localhost:7115/openapi/v1.json`

Terminal 2 — web client:

```powershell
cd src/MoodPlaylistWeb
pnpm install --frozen-lockfile
pnpm dev
```

Open `http://localhost:3000`.

## Useful API endpoints

| Method | Route | Access | Purpose |
| --- | --- | --- | --- |
| `POST` | `/auth/register` | Public | Create an account |
| `POST` | `/auth/login` | Public | Authenticate and receive tokens |
| `POST` | `/auth/logout` | Authenticated | Revoke the current user's refresh session |
| `GET` | `/library/available-moods` | Public | List configured moods |
| `GET` | `/library/available-moods/{id}/tracks` | Authenticated | Get cached recommendations for one mood |
| `GET` | `/library/recommendations` | Authenticated | Blend mood and track seeds into recommendations |
| `GET` | `/library/playlists` | Conditional | Query `mine`, `others`, or `all` playlist views |
| `POST` | `/library/playlists` | Authenticated | Create a playlist |
| `PUT` | `/library/playlists/{playlistId}` | Authenticated | Update an owned playlist |
| `POST` | `/library/playlists/{playlistId}/tracks` | Authenticated | Save tracks in bulk |
| `POST` | `/library/playlists/{playlistId}/refresh` | Authenticated | Add fresh mood recommendations |
| `DELETE` | `/library/playlists/{playlistId}/tracks/{trackId}` | Authenticated | Remove a saved track |
| `GET` | `/library/playlists/{playlistId}/tracks/{trackId}/exists` | Authenticated | Check saved-track membership |
| `GET` | `/health` | Public | Report API, database, and Spotify health |

Protected requests use `Authorization: Bearer <token>`.

Example recommendation request:

```http
GET /library/recommendations?moodIds=MoodGuid&trackIds=spotify:track:TrackId&limit=20&market=NG
Authorization: Bearer YourJwt
```

## Testing and quality checks

### Backend tests

```powershell
dotnet test src/MoodPlaylist.slnx
```

This complete command requires a running Docker engine because it includes the PostgreSQL Testcontainers category. Without Docker, run the non-integration filter shown below.

Collect cross-platform coverage for the complete suite:

```powershell
dotnet test src/MoodPlaylist.slnx --collect:"XPlat Code Coverage"
```

The backend suite uses xUnit, Moq, WebApplicationFactory, and Testcontainers. Unit tests cover controller contracts, authentication result forwarding, recommendation validation and normalization, playlist query modes and mutations, cache behavior, JWT creation/configuration, exception mapping, request logging, Spotify token acquisition/caching, response parsing, and typed `HttpClient` registration. Spotify tests use recording in-memory HTTP handlers, so they need neither network access nor live credentials.

The latest documented audit records 72 passing non-integration tests plus nine PostgreSQL integration tests. The integration suite applies real EF Core migrations to a disposable PostgreSQL 17 container and exercises repositories, JSONB track persistence, authentication middleware, and complete HTTP registration/login flows. Docker is required to execute these tests. The nine integration tests are discovered locally and are configured to execute on GitHub's Ubuntu runners; they have not been executed on the current development machine because Docker is unavailable there. See `TESTTASKS.md` and `TESTDECISIONS.md` for the verification record and rationale.

Run the suites independently when diagnosing failures:

```powershell
dotnet test src/MoodPlaylist.slnx --filter "Category!=Integration"
dotnet test src/MoodPlaylist.slnx --filter "Category=Integration"
```

### Frontend tests

```powershell
cd src/MoodPlaylistWeb
pnpm test
pnpm test:coverage
```

The Vitest suite runs in jsdom with Testing Library and exercises API request construction, error handling, model schemas, and login-page behavior.

### Full local quality gate

Run this before opening a pull request:

```powershell
dotnet restore src/MoodPlaylist.slnx
dotnet build src/MoodPlaylist.slnx --no-restore
dotnet test src/MoodPlaylist.slnx --no-build

cd src/MoodPlaylistWeb
pnpm install --frozen-lockfile
pnpm lint
pnpm test
pnpm build
```

The backend quality gate expects Docker. If Docker is unavailable locally, run the non-integration suite and rely on the pull-request workflow to execute the nine PostgreSQL container tests before merge.

## Development workflow

1. Branch from `master` using a focused branch such as `feature/playlist-sharing` or `fix/token-refresh`.
2. Keep API contracts, DTOs, client models, and tests synchronized.
3. Add or update tests at the closest stable boundary. Controllers are tested against mocked interfaces; Spotify behavior is tested at the HTTP boundary.
4. Run the full local quality gate and inspect `git diff --check` before committing.
5. Write a pull request that explains the user impact, implementation choices, test evidence, configuration changes, and deployment risk.
6. Merge only after review and successful checks.

The GitHub Actions definitions include path-filtered deployment workflows and API pull-request validation. Frontend deployment requires dependency installation, linting, tests, and a production build to pass. API pull requests and deployment require restore, Release build, 72 non-integration tests, and nine Docker-backed PostgreSQL integration tests to pass. The local quality gate remains the fastest way to catch failures before pushing.

## Deployment workflows

### Frontend: Vercel

On a push to `master` that changes `src/MoodPlaylistWeb/**`, `.github/workflows/deploy-frontend.yml`:

1. checks out the repository;
2. installs Node.js 22.13 and pnpm;
3. installs dependencies with the lockfile enforced;
4. runs ESLint and the Vitest suite;
5. creates a production Next.js build; and
6. deploys production assets with the `VERCEL_TOKEN` repository secret.

Set `NEXT_PUBLIC_API_BASE_URL` in the Vercel project environment to the deployed API URL.

### API: GHCR and Render

On a push to `master` that changes `src/MoodPlaylistApi/**`, `.github/workflows/deploy-webapi.yml`:

1. restores and builds the .NET 10 solution in Release mode;
2. runs unit and PostgreSQL Testcontainers suites as separate gates;
3. builds the multi-stage .NET 10 Dockerfile;
4. publishes `ghcr.io/<repository-owner>/moodplaylistapi:latest` to GitHub Container Registry; and
5. invokes the Render deploy hook stored in `RENDER_DEPLOY_HOOK`.

The deployed API needs the same configuration keys described in **Local configuration**, supplied as environment variables. ASP.NET Core maps nested configuration keys with double underscores, for example:

```dotenv
ConnectionStrings__DefaultConnection=...
Jwt__Issuer=...
Jwt__Audience=...
Jwt__Key=...
Jwt__MaxAge=10
Jwt__MaxRefreshAge=20
HashHelper__SecretKey=...
Spotify__BaseUrl=https://api.spotify.com/
Spotify__AccountsBaseUrl=https://accounts.spotify.com/
Spotify__ClientId=...
Spotify__ClientSecret=...
```

## Engineering decisions demonstrated

- **Testable boundaries:** controllers and external services depend on interfaces, supporting fast and deterministic unit tests.
- **Contract-aware integration:** Spotify's seed limits, ID formats, markets, and failure responses are handled explicitly.
- **Defensive API design:** validation, authorization, throttling, exception mapping, and uniform responses are cross-cutting concerns rather than repeated controller code.
- **Operational readiness:** health checks, structured logs, correlation enrichment, database retries, containers, and deployment automation are built into the service.
- **Type safety across the stack:** nullable C#, TypeScript models, and Zod-backed client validation reduce ambiguous states at system boundaries.
- **Documented testing intent:** coverage is used to verify public behavior rather than inflate metrics with implementation-detail tests.

## Security and production-readiness review

MoodPlaylist is currently a development and portfolio application, not a production-ready identity system. The API demonstrates authentication concepts, but some choices deliberately favor a compact implementation over the controls expected for real customer accounts. The following items should be treated as known security work, not recommended patterns to copy into production.

| Current implementation | Why it is not production standard | Recommended improvement |
| --- | --- | --- |
| Passwords are transformed with a single keyed `HMACSHA256` operation. | HMAC is intentionally fast. If the database and application secret are exposed, attackers can test password guesses much faster than with a password-specific KDF. Every password also shares one application key rather than having an independent salt and work factor. | Use ASP.NET Core Identity's `IPasswordHasher<TUser>` or Argon2id, bcrypt, or PBKDF2 with a unique salt, a versioned work factor, constant-time verification, and rehash-on-login support. Prefer ASP.NET Core Identity unless there is a strong reason to own this security-sensitive code. |
| Password length is restricted to 8–12 characters. | A 12-character maximum discourages passphrases and can reject password-manager-generated credentials. Composition rules are less effective than length and compromised-password screening. | Accept at least 64 characters, never silently truncate, allow Unicode and spaces, require sensible minimum length, and optionally check passwords against a breached-password service without transmitting the raw password. |
| Refresh tokens are stored in plaintext on the user record and returned to the client. | A database leak immediately exposes reusable sessions. One token per user also prevents independent device/session management. | Store only a SHA-256 hash of each refresh token in a dedicated session table. Include session ID, user ID, creation, expiry, rotation lineage, device metadata, and revocation timestamps. Rotate on every use and detect token reuse. |
| The middleware attempts to refresh expired tokens and exposes replacements in `X-New-JWT` headers. | Authentication and authorization run before this custom middleware. An expired bearer token may be rejected before renewal logic can provide a dependable session flow. Response headers also create a non-obvious token protocol. | Remove implicit renewal. Add an explicit `POST /auth/refresh` endpoint that validates a refresh-token session, rotates it atomically, and returns a new short-lived access token. |
| Access and refresh tokens are stored in browser `localStorage`. | Any successful cross-site-scripting attack can read and exfiltrate these tokens. | For a browser application, prefer `Secure`, `HttpOnly`, `SameSite` cookies and add CSRF protection for state-changing operations. If bearer storage is unavoidable, use short-lived access tokens in memory and keep refresh credentials out of JavaScript-accessible storage. |
| Registration immediately creates an active account. | The API does not establish that the user controls the supplied email address. It also makes automated account creation easier. | Add email verification using a random, single-use, hashed, short-lived token. Gate sensitive features until verification succeeds and safely support resending with rate limits. |
| Login has no account lockout, progressive delay, or endpoint-specific throttling. | The global rate limiter helps, but it is not a complete credential-stuffing and brute-force defense. | Add stricter per-account and per-IP login limits, progressive delays or temporary lockout, security-event logging, and optional CAPTCHA only after suspicious behavior. Do not reveal whether an email exists. |
| Email comparison is not visibly normalized before lookup and uniqueness checks. | Case and Unicode differences can produce confusing identities or rely on database collation behavior. | Normalize and persist a canonical email value, build the unique index on it, and use the same canonical form for registration, login, and recovery. |
| JWT signing uses one symmetric application key. | Shared symmetric secrets are harder to rotate safely across multiple services and any verifier can also mint tokens. | Use managed key storage and rotation. For multiple services, prefer asymmetric signing with a published verification key, explicit `kid`, issuer/audience validation, short access-token lifetime, and clock-skew policy. Never store keys in source control. |
| Logout clears only the current user's single refresh token. | Existing access tokens remain valid until expiry, and there is no device-level or all-session revocation model. | Keep access tokens short-lived and revoke refresh sessions individually or in bulk. Add `logout-all`, password-change revocation, and a security-stamp/token-version check where immediate invalidation is required. |
| Public playlist reads and authenticated mutations share broad controller-level rules. | Coarse authorization becomes difficult to reason about as collaboration, moderation, and administration grow. | Introduce policy/resource-based authorization handlers for playlist ownership, visibility, roles, and moderation permissions. Enforce ownership in both the API layer and repository query. |
| CORS origins are hard-coded and HTTP methods are broadly listed. | Deployment-specific policy embedded in code is easy to misconfigure and difficult to audit across environments. | Bind an allowlist from validated configuration, keep credentials restricted to trusted origins, and allow only required headers and methods. CORS is not an authorization mechanism. |
| Development tracing and rolling file logs are enabled by environment rules. | Authentication headers, tokens, passwords, reset links, or personal data can leak if request logging is expanded without redaction. Container-local files are also not durable observability storage. | Centralize structured logs, redact security headers and sensitive properties, apply retention/access controls, record auditable security events, and never log raw credentials or recovery tokens. |
| Spotify client credentials are held in application configuration. | Client credentials grant application-level access and become sensitive production secrets even though access-token acquisition and renewal are automated. | Store credentials in a managed secret service, restrict operator access, rotate them, monitor token failures, and keep them out of logs, images, workflow output, and client-side bundles. |
| Dependency restore currently reports a high-severity advisory for a transitive `Microsoft.OpenApi` version. | Shipping known vulnerable dependencies increases supply-chain risk even when the vulnerable path is not obviously exercised. | Upgrade or override the dependency after compatibility testing, enable automated dependency updates, run `dotnet list package --vulnerable --include-transitive`, and make high-severity findings a reviewed CI gate. |

Additional production controls should include TLS at every public boundary, HSTS verification behind the chosen proxy, secret rotation, encrypted managed database backups, least-privilege database credentials, data-retention/deletion policies, security headers, request-size limits, dependency and container scanning, and an incident-response process.

## Authentication and account-management roadmap

The safest extension path is to migrate account management to ASP.NET Core Identity and keep the existing JWT/API contract behind dedicated application services. Identity supplies reviewed password hashing, security stamps, token providers, lockout, email confirmation, and extensible user management. If the custom implementation is retained, it should reproduce those guarantees deliberately and receive focused security review.

### Proposed authentication surface

| Method | Route | Access | Responsibility |
| --- | --- | --- | --- |
| `POST` | `/auth/register` | Public | Create a pending account and send email verification |
| `POST` | `/auth/verify-email` | Public | Consume a single-use email verification token |
| `POST` | `/auth/resend-verification` | Public | Issue a new verification message without disclosing account existence |
| `POST` | `/auth/login` | Public | Verify credentials and create a refresh-token session |
| `POST` | `/auth/refresh` | Refresh credential | Rotate the refresh token and issue a new access token |
| `POST` | `/auth/logout` | Authenticated | Revoke the current session |
| `POST` | `/auth/logout-all` | Authenticated | Revoke all sessions for the current account |
| `POST` | `/auth/forgot-password` | Public | Always return a generic response and conditionally send a reset link |
| `POST` | `/auth/reset-password` | Public with reset token | Set a new password using a single-use, short-lived token |
| `POST` | `/auth/change-password` | Authenticated | Verify the current password, set a new one, and revoke other sessions |
| `GET` | `/profile` | Authenticated | Return the current user's safe profile fields |
| `PATCH` | `/profile` | Authenticated | Update allowed fields such as display name |
| `POST` | `/profile/change-email` | Authenticated | Start verification for a new email address |
| `POST` | `/profile/confirm-email-change` | Authenticated with token | Commit a verified email change and revoke other sessions |
| `GET` | `/profile/sessions` | Authenticated | List active devices/sessions without exposing token values |
| `DELETE` | `/profile/sessions/{sessionId}` | Authenticated | Revoke one device/session |
| `DELETE` | `/profile` | Authenticated and recently re-verified | Delete or schedule deletion of the account and owned data |

### Forgot- and reset-password design

1. `POST /auth/forgot-password` accepts an email but always returns the same status and message, whether or not an account exists. This prevents account enumeration.
2. For an eligible account, generate at least 32 cryptographically random bytes. Store only a SHA-256 hash with the user ID, purpose, creation time, short expiry, and consumed timestamp.
3. Send the raw token only through an approved email provider in an HTTPS frontend URL. Do not place it in logs, analytics, or database plaintext.
4. Apply tight per-IP and per-account rate limits. A resend should invalidate or supersede older active tokens according to a documented policy.
5. `POST /auth/reset-password` validates the hash, purpose, expiry, consumption state, and user security stamp in one transaction. It then hashes the new password with the approved password hasher and marks the token consumed.
6. Revoke all refresh sessions, advance the user's security stamp/token version, notify the account owner, and require a fresh login. Existing short-lived access tokens expire naturally unless immediate server-side revocation is required.

Reset tokens should be opaque random values, not reusable JWTs. Password reset responses and timing should not disclose account existence. Tests should cover expiration, replay, concurrent consumption, wrong purpose, invalid user, enumeration-safe responses, session revocation, and email-provider failure.

### Profile and password changes

- Keep profile DTOs explicit; never bind the `User` entity directly or return password hashes, token hashes, internal IDs, or security metadata.
- Require the current password or recent step-up authentication for password, email, MFA, and account-deletion changes.
- A normal profile edit may update display name or preferences. Email change should be a separate verified workflow and must not overwrite the current address until the new one is confirmed.
- Changing a password should revoke other refresh sessions, rotate the current session, advance the security stamp/token version, and send a security notification.
- Add optimistic concurrency or row-version checks to prevent lost updates.
- Record security audit events for password, email, MFA, session, and account-status changes without storing sensitive values.

### Recommended implementation sequence

1. Introduce an `IIdentityService`/account application service and migrate password storage to ASP.NET Core Identity's hasher. Support verification of existing hashes only long enough to rehash users safely on their next successful login.
2. Add a `UserSession` entity and migration, explicit refresh rotation, token hashing, reuse detection, current-session logout, and all-session logout.
3. Move browser authentication to secure cookies and add CSRF protection, or document and test an alternative client-specific token strategy.
4. Add an email abstraction with a development sink and a production provider. Implement email verification, resend, forgot password, and reset password.
5. Add profile read/update, change-password, verified email-change, session management, and account deletion.
6. Add policy-based authorization, administrative roles only if the product needs them, and optional MFA with TOTP plus single-use recovery codes. Avoid SMS as the only high-assurance factor.
7. Add integration and end-to-end tests covering the database, cookies/headers, token rotation, concurrent requests, email links, expiry, revocation, authorization boundaries, and abuse controls.
8. Add pull-request security gates for vulnerable dependencies, secret scanning, static analysis, container scanning, migration validation, and the complete authentication test suite.

OAuth/OIDC social login can be added later through a trusted provider. Link external identities only after proving ownership of both the local account and provider identity; do not automatically merge accounts based solely on an unverified matching email.

## Current limitations and roadmap

- Add frontend pull-request validation so linting, tests, and production builds run before changes reach `master`, not only during deployment.
- Complete the authentication roadmap above, beginning with adaptive password hashing, refresh-session rotation, secure browser cookies, and account recovery.
- Extend HTTP end-to-end coverage from authentication into discovery, recommendations, and complete playlist management.
- Pin GitHub Actions to current major versions or immutable commit SHAs and add deployment environments/approvals.

## License

This project is licensed under the terms in [LICENSE.txt](LICENSE.txt).
