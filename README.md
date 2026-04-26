# BigBall ⚽

A World Cup 2026 prediction pool app built with Blazor WebAssembly and ASP.NET Core.

## Overview

BigBall lets users create and join prediction pools for World Cup matches. Track your predictions, earn points based on scoring tiers (PRD 4.8), and compete in real-time rankings with friends and colleagues.

**Stack**: .NET 8 (Blazor WASM + API stub), MVVM architecture, dark theme

## Quick Start

### Clone & Build

```bash
git clone <repo-url>
cd BigBall
dotnet build
```

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

Use any email + password (stub auth). Example:
- Email: `joao.pereira@gmail.com`
- Password: `x`

## What's Inside

- **BigBall.Domain**: Scoring engine (20/16/15/10/5/0 points + penalties)
- **BigBall.Api**: Minimal API with in-memory data + JWT auth
- **BigBall.Web**: Blazor WASM frontend (4 screens: Login, Home, Pool Detail, Predict)
- **BigBall.Client.Core**: Shared MVVM + HTTP clients (reusable for future MAUI app)

## Notes

This is a **stub implementation** with in-memory data. Full Supabase integration and sports data provider are out of scope for this iteration.

---

Built as part of a TCC (undergraduate thesis) project.
