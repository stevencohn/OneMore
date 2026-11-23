# Architecture

Model: Opus 5.5

```
You are a senior engineer doing an ARCHITECTURE review of "OneMore".

First, read the architecture brief at:
C:\Github\OneMore\specs\design-review\onemore-architecture-brief.md

Review only through this lens. Read the actual code; don't guess from file names.
Read-only: do not modify any existing files. The only file you may create is
your report, at:
C:\Github\OneMore\specs\design-review\findings\architecture.md
(Create the findings folder if it doesn't exist; overwrite the file if it does.)

Context: this is a whole-system review across the solution's projects (add-in, tray, service, CLI, protocol handler, setup, plugins). Focus on structure and boundaries, not line-level issues; other passes cover those.

Lens checklist:
Architecture: module and project boundaries, dependency direction and cycles,
shared state and global singletons, inter-process communication design, extensibility
(plugins, themes, templates), configuration and settings handling, error-handling
strategy as a whole, threading model, how well the structure fits the product's goals,
and the highest-value structural refactors

For each finding report:
- Title and severity (critical/high/medium/low)
- Confidence (high/medium/low), and what you'd check to confirm if not high
- Location (file:line) and a short code excerpt
- Why it matters: what it constrains or puts at risk as the project grows
- Suggested fix (brief, with a sketch if helpful)
- Effort (S/M/L)

Rules: only report issues you can point to in the code; no generic advice.
Separate "confirmed issues" from "worth investigating". Skip style nitpicks
unless they hurt maintainability. Finish with the 5 highest-impact items.

Write the complete report to the report file above. In your reply, give only
the file path and a 3-line summary.
```
