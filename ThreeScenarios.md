# Three Scenarios: Agentic SDLC Orchestrator

This document demonstrates three scenarios where the agentic orchestration system provides real value beyond a human manually writing code.

---

## Scenario 1: Greenfield — Build a New Service from Scratch

**Context:** A team needs a new internal URL shortener service for sharing long dashboard links in Slack. No existing codebase. Requirements are clear.

**What the agent does:**

1. **Requirements stage** — Agent reads the requirement prompt and breaks it into engineering tasks: data model, 3 REST endpoints, SQLite storage, collision-safe short code generation.
2. **Architecture stage** — Agent designs the component structure: Minimal API, EF Core DbContext, SQLite, CORS policy.
3. **Code stage** — Agent calls tools in sequence:
   - `create_project` → scaffolds `UrlShortenerApi`
   - `add_package` → adds `Microsoft.EntityFrameworkCore.Sqlite`
   - `write_file` × 3 → writes `Models/ShortenedUrl.cs`, `Data/AppDbContext.cs`, `Program.cs`
   - `run_build` → compiles and reports errors
   - If build fails → agent reads error, fixes the file, builds again (self-healing)
4. **Tests stage** — Agent writes unit tests and calls `run_tests`
5. **Docs stage** — Agent writes `README.md` with endpoint docs, curl examples, setup guide
6. **Human checkpoint** — Orchestrator pauses before Deploy and asks for human approval
7. **Deploy stage** — Agent writes `DEPLOY.md` checklist

**Outcome:** A working, tested, documented ASP.NET Core API — built entirely by the agent in ~5 minutes. A human would spend 2-3 hours doing the same.

**Key agent behavior:** The ReAct loop means the agent *observes* build output and *corrects* errors autonomously. It doesn't just write code — it verifies it compiles.

---

## Scenario 2: Brownfield — Add a Feature to an Existing Codebase

**Context:** The URL shortener is already running in production. The team wants to add a **custom alias** feature — users can choose their own short code (e.g., `/docs` instead of `/aB3x9k`).

**What changes:**
- Requirement prompt changes to: *"Add optional custom alias support to POST /shorten. If alias is provided, use it as the code. Return 409 Conflict if alias already taken."*
- The agent now calls `read_file` first to understand the existing `Program.cs` and `ShortenedUrl.cs` before writing changes
- Agent modifies `ShortenedUrl.cs` to add `CustomAlias` field
- Agent updates `POST /shorten` endpoint to handle optional alias
- Agent runs `run_build` to verify no regressions
- Agent updates README with new request body docs

**What's different from Scenario 1:**
- Agent reads existing files before writing (brownfield awareness)
- Agent only modifies what's needed — doesn't recreate the whole project
- Build verification catches any breaking changes

**Outcome:** Feature added safely without breaking existing endpoints. The human only wrote the requirement sentence — the agent did the rest.

---

## Scenario 3: Ambiguous Requirements — Agent Handles Uncertainty

**Context:** A product manager gives a vague requirement: *"Make the URLs expire."*

**What the agent does:**

The agent doesn't have enough information to write code, so during the **Requirements stage** it produces a structured list of clarifying questions embedded in its output:

```
Ambiguous requirement detected: "Make the URLs expire"

Engineering questions:
1. Should expiration be time-based (e.g., 24 hours) or access-count-based (e.g., max 10 clicks)?
2. What happens on access after expiry — 404 or redirect to an error page?
3. Should expiry be set per-URL at creation time or globally configured?
4. Should expired URLs be deleted from DB or soft-deleted (kept for audit)?
5. Is there a UI to show expiry status, or API-only?
```

The **StageManager** treats the Requirements stage as incomplete and does not advance to Architecture. The orchestrator surfaces these questions to the human via the audit log and console output.

Once the human responds (e.g., *"Time-based, 7 days default, return 410 Gone, soft delete"*), the requirement is updated and the orchestrator re-runs from Requirements with the clarified prompt.

**What this demonstrates:**
- The agent doesn't blindly write wrong code — it identifies ambiguity early
- Retry logic handles the re-run cleanly
- Human-in-the-loop isn't just for Deploy — it's built into the Requirements stage when confidence is low
- Audit log records both the ambiguous input and the clarified version

**Outcome:** No wasted implementation cycles. The ambiguity is caught at the cheapest possible stage (requirements) rather than discovered during code review or testing.

---

## Summary

| Scenario | Agent Behavior | Human Role |
|----------|---------------|------------|
| Greenfield | Build from scratch, self-heal build errors | Write requirement sentence, approve Deploy |
| Brownfield | Read existing code, make targeted changes | Write feature requirement |
| Ambiguous | Surface clarifying questions, wait for input | Answer questions, approve re-run |

The orchestrator's value is consistent across all three: **the human stays at the decision layer**, while the agent handles the execution layer — writing, building, testing, and documenting.
