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
- Structured Serilog logging and development tracing
- Deterministic frontend and backend test suites
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
| External integration | Spotify Web API through a typed `HttpClient` |
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
- Optional: Docker, for building the API image

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
dotnet user-secrets set "Spotify:BaseUrl" "https://api.spotify.com/v1/"
dotnet user-secrets set "Spotify:ClientSecret" "YOUR_SPOTIFY_BEARER_TOKEN"
```

`Spotify:ClientSecret` is currently placed directly into the outbound bearer authorization header. Supply an access token compatible with Spotify's API, not a raw client secret. For a production system, replace this with a client-credentials token provider that obtains and renews short-lived access tokens.

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

If `dotnet-ef` is already installed, update it with `dotnet tool update --global dotnet-ef`. The current migration creates the schema; mood records and their Spotify seed configuration must exist before mood discovery can return useful results.

## Run the application

Use two terminals from the repository root.

Terminal 1 — API:

```powershell
dotnet restore src/MoodPlaylist.slnx
dotnet run --project src/MoodPlaylistApi --launch-profile https
```

The API listens on `https://localhost:7115` and `http://localhost:5015`. In Development, the OpenAPI document is exposed at `/openapi/v1.json`.

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

Collect cross-platform coverage:

```powershell
dotnet test src/MoodPlaylist.slnx --collect:"XPlat Code Coverage"
```

The backend suite uses xUnit and Moq and currently covers controller contracts, authentication result forwarding, recommendation validation and normalization, playlist query modes and mutations, cache behavior, Spotify response parsing/error mapping, and typed `HttpClient` registration. Spotify tests use a recording in-memory HTTP handler, so the normal suite needs neither network access nor live credentials.

The latest documented audit records 42 passing backend tests and 100% line coverage for public authentication actions, library action methods, the Spotify service, cache service, and Spotify HTTP registration. See `TESTTASKS.md` and `TESTDECISIONS.md` for the verification record and rationale. PostgreSQL Testcontainers integration coverage is explicitly tracked as future work.

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

## Development workflow

1. Branch from `master` using a focused branch such as `feature/playlist-sharing` or `fix/token-refresh`.
2. Keep API contracts, DTOs, client models, and tests synchronized.
3. Add or update tests at the closest stable boundary. Controllers are tested against mocked interfaces; Spotify behavior is tested at the HTTP boundary.
4. Run the full local quality gate and inspect `git diff --check` before committing.
5. Write a pull request that explains the user impact, implementation choices, test evidence, configuration changes, and deployment risk.
6. Merge only after review and successful checks.

The GitHub Actions definitions are path-filtered, test-gated deployment workflows. Frontend deployment requires dependency installation, linting, tests, and a production build to pass. API deployment requires restore, Release build, and the complete .NET test suite to pass. The local quality gate remains the fastest way to catch failures before pushing.

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
2. runs the complete xUnit test suite;
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
Spotify__BaseUrl=https://api.spotify.com/v1/
Spotify__ClientSecret=...
```

## Engineering decisions demonstrated

- **Testable boundaries:** controllers and external services depend on interfaces, supporting fast and deterministic unit tests.
- **Contract-aware integration:** Spotify's seed limits, ID formats, markets, and failure responses are handled explicitly.
- **Defensive API design:** validation, authorization, throttling, exception mapping, and uniform responses are cross-cutting concerns rather than repeated controller code.
- **Operational readiness:** health checks, structured logs, correlation enrichment, database retries, containers, and deployment automation are built into the service.
- **Type safety across the stack:** nullable C#, TypeScript models, and Zod-backed client validation reduce ambiguous states at system boundaries.
- **Documented testing intent:** coverage is used to verify public behavior rather than inflate metrics with implementation-detail tests.

## Current limitations and roadmap

- Replace the manually supplied Spotify bearer token with a production client-credentials token service and automatic renewal.
- Add PostgreSQL integration tests with Testcontainers for repository and migration behavior.
- Add pull-request validation so the same deployment checks run before changes reach `master`.
- Add repeatable seed data for local mood configuration.
- Move browser tokens from `localStorage` to secure, HTTP-only cookies for stronger protection against token theft through injected scripts.
- Add end-to-end tests for registration, discovery, recommendations, and playlist management.
- Pin GitHub Actions to current major versions or immutable commit SHAs and add deployment environments/approvals.

## License

This project is licensed under the terms in [LICENSE.txt](LICENSE.txt).
