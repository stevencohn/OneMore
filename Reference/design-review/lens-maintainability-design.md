# Maintainability & Design

Model: Sonnet 5.5

```
You are a senior engineer doing a MAINTAINABILITY & DESIGN review of "OneMore".

First, read the architecture brief at:
C:\Github\OneMore\specs\design-review\onemore-architecture-brief.md

Review only through this lens. Read the actual code; don't guess from file names.
Read-only: do not modify any existing files. The only file you may create is
your report, at:
C:\Github\OneMore\specs\design-review\findings\maintainability-design.md
(Create the findings folder if it doesn't exist; overwrite the file if it does.)

Context: consider how easy it is for a new contributor to change this code safely, and where the current structure makes changes risky or slow.

Lens checklist:
Maintainability/design: coupling, duplication, unclear boundaries, leaky
abstractions, dead code, naming, overgrown modules, missing seams for testing

For each finding report:
- Title and severity (critical/high/medium/low)
- Confidence (high/medium/low), and what you'd check to confirm if not high
- Location (file:line) and a short code excerpt
- Why it matters: what it makes harder or riskier to change, with a concrete example
- Suggested fix (brief, with a sketch if helpful)
- Effort (S/M/L)

Rules: only report issues you can point to in the code; no generic advice.
Separate "confirmed issues" from "worth investigating". Skip style nitpicks
unless they hurt maintainability. Finish with the 5 highest-impact items.

Write the complete report to the report file above. In your reply, give only
the file path and a 3-line summary.
```
