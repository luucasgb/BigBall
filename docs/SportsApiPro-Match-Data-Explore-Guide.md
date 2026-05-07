# SportsApi Pro — Manual match data (Explore UI)

This guide walks through discovering **competition / tournament IDs** and **listing games** using the **Explore** section on [SportsApi Pro documentation](https://docs.sportsapipro.com/), without writing application code. Keep your API key private; the docs describe API keys as [server-side only](https://docs.sportsapipro.com/authentication).

Football data is exposed under **two different REST bases**. IDs from one base **do not** automatically match the other — always pick endpoints from **either** V1 **or** V2 for a single workflow.

| API | Base URL | Typical use in docs |
|-----|------------|---------------------|
| **Football V1** | `https://v1.football.sportsapipro.com` | `/games/fixtures`, `/competitions/top`, `/standings`, `/games/season-results` |
| **Football V2** | `https://v2.football.sportsapipro.com` | `/api/search`, `/api/tournaments/.../seasons`, `/api/tournament/.../season/.../rounds` |

---

## 1. Why you see two different IDs for the same league

SportsApi Pro documents a **dual-ID model** for Football **V2**: the stable identifier for a league or cup is **`uniqueTournament.id`** (season-independent). A separate **`tournament.id`** is tied to a specific season/group and changes when seasons roll over.

- **Do not** persist per-season `tournament.id` as if it were permanent — [Canonical IDs](https://docs.sportsapipro.com/api-reference/football-v2/canonical-ids) explains storage and queries.

**Important:** The numeric **`competitionId`** used by **Football V1** listings is **not** the same number as **V2** `uniqueTournament.id` for every competition. Example from the docs:

- **UEFA Champions League** — V1 competition id **572** ([Standings — common competition IDs](https://docs.sportsapipro.com/api-reference/standings/get#common-competition-ids)); V2 canonical **`uniqueTournament.id` = 7** ([Canonical IDs](https://docs.sportsapipro.com/api-reference/football-v2/canonical-ids#confirmed-canonical-ids-top-competitions)).
- **FIFA World Cup** — V2 canonical **`uniqueTournament.id` = 16** (same page).

So: **always confirm the ID in the Explore UI** for the exact base URL and endpoint you are calling.

---

## 2. Discovering IDs in Explore

Use one or more of these approaches.

### A. Football V2 — Search (best for names)

In Explore, open **Football V2** → **Global, Search & Schedule** → **`GET /api/search?q={query}`**  
([Global API reference](https://docs.sportsapipro.com/api-reference/football-v2/global))

- Try queries such as `uefa champions league`, `world cup`, `fifa world cup`.
- Results include **`type`** (`team`, `player`, `tournament`) and numeric **`id`** fields you reuse on **V2** tournament endpoints.

Minimum query length is documented as **2 characters**.

### B. Football V1 — Top competitions

**`GET /competitions/top`** with `sports=1` (football) returns popular leagues with a **`competitions[].id`** field — this is the **V1 competition id** ([Top competitions](https://docs.sportsapipro.com/api-reference/competitions/top)).

Example names from the docs’ “Typical Top Football Competitions” table on that page include UEFA Champions League (**572** in that catalog).

### C. Canonical references (V2 storage)

For stable **V2** tournament identifiers, see [Confirmed canonical IDs (top competitions)](https://docs.sportsapipro.com/api-reference/football-v2/canonical-ids#confirmed-canonical-ids-top-competitions), including:

| Competition | `uniqueTournament.id` (V2) |
|-------------|----------------------------|
| UEFA Champions League | **7** |
| FIFA World Cup | **16** |

Still verify in **`/api/search`** or **`/api/tournaments/{id}/seasons`** that the tournament exists and has the season you care about.

---

## 3. UEFA Champions League — 2025/26 season (list matches)

Pick **either** V1 **or** V2 below.

### Option A — Football V1 (competition **572**)

1. **Confirm the competition id**  
   Prefer **`GET /competitions/top?...`** or the standings doc’s table: Champions League = **572** ([Standings](https://docs.sportsapipro.com/api-reference/standings/get#common-competition-ids)).

2. **Discover season numbers**  
   `GET /standings?competitions=572&withSeasonsFilter=true`  
   Read **`seasonFilters`** — each entry has **`num`** (season number) and **`name`** (e.g. `2025/26`) ([Season results](https://docs.sportsapipro.com/api-reference/games/season-results)).

3. **List games** (pick what you need):
   - **Upcoming + past in one stream (paginated):**  
     `GET /games/fixtures?competitions=572`  
     ([Competition details / fixtures](https://docs.sportsapipro.com/api-reference/competitions/details), [Games best practices](https://docs.sportsapipro.com/api-reference/games/best-practices))
   - **Completed matches only:**  
     `GET /games/results?competitions=572`  
     ([Team / competition results](https://docs.sportsapipro.com/api-reference/games/results))
   - **Paged season results (completed, per season):**  
     `GET /games/season-results?competitions=572&seasonNum=<num>&page=1&pageSize=100`  
     Adjust **`page`** until **`pagination.hasMore`** is false. Note provider limits described on [Season results](https://docs.sportsapipro.com/api-reference/games/season-results) (e.g. emphasis on **current** season for some uses).

Use **`paging.nextPage`** / **`paging.previousPage`** when present on fixtures/results responses.

### Option B — Football V2 (`uniqueTournament.id` **7**)

1. **Discover seasons (always start here)**  
   `GET /api/tournaments/7/seasons`  
   **`7`** is the canonical Champions League **`uniqueTournament.id`** ([Canonical IDs](https://docs.sportsapipro.com/api-reference/football-v2/canonical-ids)). Pick the **`seasons[]`** entry whose label matches **2025/26** ([Tournament endpoints](https://docs.sportsapipro.com/api-reference/football-v2/tournament)).

2. **Season-scoped `tournamentId`**  
   Paths like `/api/tournament/{tournamentId}/season/{seasonId}/...` must use the **`tournamentId`** (and **`seasonId`**) values **returned for that season** from the **`/seasons`** payload — the docs warn against hardcoding per-season IDs ([Canonical IDs — resolution flow](https://docs.sportsapipro.com/api-reference/football-v2/canonical-ids)). In Explore, expand the JSON and copy the numeric IDs the response uses for **`/api/tournament/.../season/...`**.

3. **List rounds, then matches per round**  
   - `GET /api/tournament/{tournamentId}/season/{seasonId}/rounds`  
   - For each round number:  
     `GET /api/tournament/{tournamentId}/season/{seasonId}/round/{round}`  

   Champions League has knockout phases; **Knockout / Cup Tree** may also help:  
   `GET /api/tournament/{tournamentId}/season/{seasonId}/knockout`  
   ([Tournament endpoints](https://docs.sportsapipro.com/api-reference/football-v2/tournament))

4. **Alternate aggregations**  
   - **Recent finished matches (paginated):**  
     `GET /api/tournament/{tournamentId}/season/{seasonId}/events/last/{page}` (`page` 0 = most recent).  
   - **Calendar day slice:**  
     `GET /api/tournament/{tournamentId}/scheduled-events/{date}` with **`date`** = `YYYY-MM-DD`.

---

## 4. FIFA World Cup — 2026 (list matches)

The men’s **2026** edition is a **short window** (group stage + knockout). IDs should still be **discovered**, not guessed.

### Football V2 (canonical **`uniqueTournament.id` = 16**)

1. Confirm with **`GET /api/search?q=world+cup`** or **`GET /api/tournaments/16/seasons`** that **16** is the FIFA World Cup **`uniqueTournament.id`** you want ([Canonical IDs](https://docs.sportsapipro.com/api-reference/football-v2/canonical-ids)).

2. Select the **2026** season from **`/seasons`**, then copy **`tournamentId`** / **`seasonId`** from that response for all **`/api/tournament/.../season/...`** calls (same rule as §3 Option B).

3. Use the same V2 patterns as in §3 Option B:
   - **`/rounds`** + **`/round/{round}`** for all scheduled matches by phase,
   - **`/scheduled-events/{date}`** day-by-day during the tournament,
   - **`/knockout`** for the bracket if applicable.

### Football V1

There is no single “World Cup” id called out in the same standing tables as **572** for UCL; **discover** it:

- **`GET /competitions/top?limit=100&sports=1`** and scan **`competitions`** for FIFA / World Cup, **or**
- Use any search/catalog endpoint your Explore UI exposes for V1 competitions,

then use **`/games/fixtures?competitions={id}`** and **`/games/results?competitions={id}`** as in §3 Option A once you have the correct **`competitionId`**.

---

## 5. Practical tips

- **`/games/allscores`** covers a **date range** but **does not** honor a competitions filter; for competition-specific lists prefer **`/games/fixtures`** or **`/games/results`** ([Games list (legacy)](https://docs.sportsapipro.com/api-reference/games/list), [Best practices](https://docs.sportsapipro.com/api-reference/games/best-practices)).
- **Wide date ranges** on **`/games/allscores`** may return empty **`games`** arrays; prefer short ranges or competition-scoped endpoints ([Games list](https://docs.sportsapipro.com/api-reference/games/list)).
- **Rate limits:** each HTTP call counts; paginate with small delays if you script later ([Rate limits](https://docs.sportsapipro.com/rate-limits)).

---

## 6. Doc links (quick)

| Topic | URL |
|-------|-----|
| Canonical / dual IDs (V2) | https://docs.sportsapipro.com/api-reference/football-v2/canonical-ids |
| Football V2 tournament endpoints | https://docs.sportsapipro.com/api-reference/football-v2/tournament |
| Football V2 search & schedule | https://docs.sportsapipro.com/api-reference/football-v2/global |
| V1 top competitions | https://docs.sportsapipro.com/api-reference/competitions/top |
| V1 standings & season filters | https://docs.sportsapipro.com/api-reference/standings/get |
| V1 fixtures by competition | https://docs.sportsapipro.com/api-reference/competitions/details |
| V1 season results (paged) | https://docs.sportsapipro.com/api-reference/games/season-results |
| Choosing games endpoints | https://docs.sportsapipro.com/api-reference/games/best-practices |

---

*Generated from SportsApi Pro documentation available via the project MCP catalog; re-check live Explore paths if the docs site navigation changes.*
