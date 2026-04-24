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
