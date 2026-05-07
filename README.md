# BigBall ⚽

A World Cup 2026 prediction pool app built with Blazor WebAssembly and ASP.NET Core.

## Overview

BigBall lets users create and join prediction pools for World Cup matches. Track your predictions, earn points based on scoring tiers (PRD 4.8), and compete in real-time rankings with friends and colleagues.

**Stack**: .NET 8 (Blazor WASM + ASP.NET Core API), MVVM architecture, dark theme

## Quick Start

### Clone & Build

```bash
git clone <repo-url>
cd BigBall
dotnet build
```

### Configure API secrets (local development)

`BigBall.Api` loads **.NET User Secrets** for local development and **environment variables** in production. `BigBall.Web` has no app secrets.

**One-time repo hygiene**

- In the repo root `.gitignore`, ignore `appsettings.Development.json` and `appsettings.*.json` recursively, but keep `appsettings.json` tracked.
- Remove the API dev file from Git tracking (keep your local copy): `git rm --cached src/BigBall.Api/appsettings.Development.json`, then commit. If a real secret was ever committed, rotate it and consider purging history (e.g. `git filter-repo --path src/BigBall.Api/appsettings.Development.json --invert-paths`).

**User Secrets** (run from `src/BigBall.Api/`):

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Supabase" "<your-connection-string>"
dotnet user-secrets set "Supabase:ProjectUrl" "<your-project-url>"
dotnet user-secrets set "Supabase:PublishableKey" "<your-publishable-key>"
```

Secrets are stored outside the repo (Windows: `%APPDATA%\Microsoft\UserSecrets\<guid>\secrets.json`; Linux/macOS: `~/.microsoft/usersecrets/<guid>/secrets.json`).

**Production** — inject the same values as environment variables using double underscores: `ConnectionStrings__Supabase`, `Supabase__ProjectUrl`, `Supabase__PublishableKey` (Docker Compose, hosting panel, CI/CD secrets such as GitHub Actions `${{ secrets.NAME }}`, etc.).

**Precedence** (highest to lowest): environment variables → User Secrets → `appsettings.Development.json` → `appsettings.json`. The committed `appsettings.Development.json` should use placeholders like `CONFIGURE_VIA_USER_SECRETS` for sensitive keys. Non-secret values (for example `JwtAudience: authenticated`, Serilog/Seq URLs) are safe to commit there.

### Run Locally

#### 1. Start the API (port 5080)
```bash
dotnet run --project src/BigBall.Api
```

#### 2. Start the Web Client (port 5180)
```bash
dotnet run --project src/BigBall.Web
```

Open **http://localhost:5180** in your browser.

### Seq (optional — API logs in Development)

The API uses [Serilog](https://serilog.net/) with console, rolling files under `logs/`, and (in **Development** only) [Seq](https://datalust.co/) for structured log search. Seq is not required to run the app; the API works without it, but the Seq sink may log connection warnings if the URL is unreachable.

**Run Seq with Docker** (see [Seq Docker deployment](https://datalust.co/docs/docker-deployment-overview) for the latest options, such as the first-run admin password and data retention):

```bash
docker run --name seq -d --restart unless-stopped -e ACCEPT_EULA=Y -e SEQ_FIRSTRUN_ADMINPASSWORD=<MyPassword> -v seq-data:/data -p 5341:80 datalust/seq
```

After the container is healthy, open the Seq UI at **http://localhost:5341**. The API’s [`appsettings.Development.json`](src/BigBall.Api/appsettings.Development.json) is configured to send events to that address when `ASPNETCORE_ENVIRONMENT` is `Development` (the default for `dotnet run`).

### Login

Register and sign in with the email and password (or OAuth providers) you configure in your Supabase project. The app does not ship with demo users or passwords.

## What's Inside

- **BigBall.Domain**: Scoring engine (20/16/15/10/5/0 points + penalties)
- **BigBall.Api**: Minimal API backed by PostgreSQL (Supabase) + JWT auth
- **BigBall.Web**: Blazor WASM frontend (4 screens: Login, Home, Pool Detail, Predict)
- **BigBall.Client.Core**: Shared MVVM + HTTP clients (reusable for future MAUI app)

## Notes

This is a thesis-scale implementation: some product areas are simplified or still evolving. Match data can be loaded from the bundled World Cup 2026 fixture file when enabled in configuration.

Predictions close at **official kickoff** (see PRD 4.7), not before the start.

---

Built as part of a TCC (undergraduate thesis) project.
