# Agentic SDLC Orchestrator — Final Write-Up

## What I Built

An agentic orchestration system that manages the full software development lifecycle (SDLC) for a URL shortener service. The system uses Claude AI as its reasoning engine, a sandboxed tool server for safe code execution, and a dependency graph to coordinate stages with human approval checkpoints.

**The agent actually writes the code.** It doesn't describe what to do — it calls tools to scaffold projects, write files, run builds, and fix compiler errors autonomously.

---

## Architecture

```
┌─────────────────────────────────────────────────────┐
│                   Orchestrator                       │
│                                                     │
│  StageManager ──► AgentLoop ──► ToolServer (HTTP)  │
│  (dependency    (ReAct loop)   (sandboxed tools)   │
│   graph)                                            │
│                                                     │
│  HumanCheckpoint ──► AuditLogger                   │
└─────────────────────────────────────────────────────┘
         │                    │
         ▼                    ▼
   Claude Haiku          Generated/
   (via internal         (URL shortener
    LLM endpoint)         source code)
```

### Components

**Orchestrator** (`Orchestrator/`)
- `Program.cs` — entry point, reads requirement, drives the stage loop
- `AgentLoop.cs` — ReAct loop: sends messages to Claude, executes tool calls, feeds results back
- `StageManager.cs` — dependency graph (Requirements → Architecture → Code → Tests+Docs → Deploy)
- `HumanCheckpoint.cs` — pauses before Deploy, requires y/n approval from human
- `AuditLogger.cs` — timestamps every decision to `audit.log`

**ToolServer** (`ToolServer/`)
- Minimal ASP.NET Core API running on `http://localhost:8000`
- Exposes a `/tool` endpoint that executes whitelisted operations
- All file writes sandboxed to `Generated/` folder with path traversal protection
- Only runs `dotnet build` and `dotnet test` — no arbitrary shell execution
- Only allows 3 whitelisted NuGet packages

**Generated** (`Generated/`)
- The URL shortener project built entirely by the agent
- `Program.cs` — 3 endpoints: POST /shorten, GET /{code}, GET /analytics/{code}
- `Models/ShortenedUrl.cs` — EF Core entity
- `Data/AppDbContext.cs` — SQLite DbContext
- SQLite database auto-created on startup

---

## How the ReAct Loop Works

The core of the agent is a **Reason → Act → Observe** loop:

```
Claude receives prompt
       │
       ▼
Claude reasons about what to do
       │
       ▼
Claude calls a tool (write_file, run_build, etc.)
       │
       ▼
ToolServer executes the tool and returns result
       │
       ▼
Result appended to conversation history
       │
       ▼
Claude sees result and reasons again
       │
       └─► Loop until Claude says "Stop" (no more tools needed)
```

This means if `run_build` returns a compiler error, Claude reads the error, calls `write_file` to fix the file, and calls `run_build` again — without human intervention.

---

## Stage Dependency Graph

```
Requirements
     │
     ▼
Architecture
     │
     ▼
    Code
   ┌──┴──┐
   ▼     ▼
Tests   Docs
   └──┬──┘
      ▼
   Deploy  ◄── Human approval required
```

Tests and Docs run after Code completes. Deploy waits for both Tests and Docs. This mirrors real SDLC constraints — you don't deploy before tests pass.

---

## Safety Design

The ToolServer was designed with security as a first principle:

| Risk | Mitigation |
|------|-----------|
| Agent writes files outside project | Path traversal check: all writes validated against `Generated/` sandbox |
| Agent runs arbitrary shell commands | Only `dotnet build` and `dotnet test` — no `exec`, no `bash`, no `curl` |
| Agent installs malicious packages | Whitelist of 3 allowed NuGet packages |
| Agent modifies orchestrator itself | ToolServer only knows about `Generated/` — orchestrator files are unreachable |
| Runaway retries | MaxRetries = 3 with exponential backoff, then safe-stop |
| Unauthorized deployment | Human checkpoint: Deploy stage requires explicit y/n approval |

---

## Why Agentic vs Manual

| Aspect | Manual Approach | Agentic Approach |
|--------|----------------|-----------------|
| Code writing | Human writes every file | Agent writes, builds, fixes errors |
| Error handling | Human reads compiler output | Agent self-corrects in the same loop |
| Documentation | Human writes separately | Agent writes docs from the same context |
| Repeatability | Varies by developer | Deterministic from requirement prompt |
| Auditability | Git history only | Full audit.log with timestamps and decisions |
| Human role | Writes everything | Writes requirement, approves deployment |

---

## Technology Choices

| Choice | Reason |
|--------|--------|
| C# / .NET 9 | Primary language, VS 2022 compatible |
| OpenAI SDK (Azure.AI.OpenAI) | Compatible with  internal LLM endpoint |
| Claude Haiku | Fast, low cost for agentic loops with many tool calls |
| SQLite | Zero-config, embedded — right for a scaffolded demo |
| Minimal API | Less boilerplate, readable endpoint definitions |
| HTTP tool server | Decoupled — ToolServer can run separately, be replaced, or scaled |

---

## What the Agent Produced

Running `dotnet run` in `Orchestrator/` with a single requirement prompt caused the agent to:

1. Call `create_project` → scaffolded ASP.NET Core webapi
2. Call `add_package` → added EF Core SQLite
3. Call `write_file` × 3 → wrote `ShortenedUrl.cs`, `AppDbContext.cs`, `Program.cs`
4. Call `run_build` → compiled successfully
5. Call `write_file` × 8 → wrote README, API docs, testing guide, deploy checklist, etc.

Total time: ~5 minutes. Human effort: one requirement sentence.

---

## What I Would Add Next

- **Rollback**: If Tests stage fails, revert Generated/ to last known-good state via git
- **Parallel tool calls**: Claude can call multiple tools in one response — ToolServer should handle concurrent requests
- **Custom alias support**: Add Scenario 2 as a real brownfield run (see ThreeScenarios.md)
- **Web UI**: Show stage progress, audit log, and generated files in a browser dashboard
- **More tool types**: `run_migration`, `docker_build`, `push_to_registry` with appropriate safety gates

---

## Files to Review

| File | What it shows |
|------|--------------|
| `Orchestrator/AgentLoop.cs` | ReAct loop implementation |
| `Orchestrator/StageManager.cs` | Dependency graph |
| `Orchestrator/HumanCheckpoint.cs` | Human-in-the-loop |
| `Orchestrator/AuditLogger.cs` | Audit trail |
| `ToolServer/Program.cs` | Sandboxed tool execution |
| `Generated/Program.cs` | URL shortener built by agent |
| `Orchestrator/audit.log` | Full decision trail from last run |
| `ThreeScenarios.md` | Greenfield, Brownfield, Ambiguous scenarios |
