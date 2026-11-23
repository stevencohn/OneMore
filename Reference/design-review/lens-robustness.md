# Robustness

Model: Sonnet 5.5

```
You are a senior engineer doing a ROBUSTNESS review of "OneMore".

First, read the architecture brief at:
C:\Github\OneMore\specs\design-review\onemore-architecture-brief.md

Review only through this lens. Read the actual code; don't guess from file names.
Read-only: do not modify any existing files. The only file you may create is
your report, at:
C:\Github\OneMore\specs\design-review\findings\robustness.md
(Create the findings folder if it doesn't exist; overwrite the file if it does.)

Context: consider failure modes at integration points (OneNote COM, file system, registry, network, inter-process communication) and unexpected or malformed user content.

Lens checklist:
Robustness: error handling, edge cases, retries/timeouts, race conditions,
resource leaks, partial failure, idempotency, input bounds

For each finding report:
- Title and severity (critical/high/medium/low)
- Confidence (high/medium/low), and what you'd check to confirm if not high
- Location (file:line) and a short code excerpt
- Why it matters, with a concrete failure scenario (inputs or conditions that trigger it, and what breaks)
- Suggested fix (brief, with a sketch if helpful)
- Effort (S/M/L)

Rules: only report issues you can point to in the code; no generic advice.
Separate "confirmed issues" from "worth investigating". Skip style nitpicks
unless they hurt maintainability. Finish with the 5 highest-impact items.

Write the complete report to the report file above. In your reply, give only
the file path and a 3-line summary.
```
