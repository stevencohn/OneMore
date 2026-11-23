# Dependencies

Model: Sonnet 5.5

```
You are a senior engineer doing a DEPENDENCIES review of "OneMore".

First, read the architecture brief at:
C:\Github\OneMore\specs\design-review\onemore-architecture-brief.md

Review only through this lens. Read the actual code; don't guess from file names.
Read-only: do not modify any existing files. The only file you may create is
your report, at:
C:\Github\OneMore\specs\design-review\findings\dependencies.md
(Create the findings folder if it doesn't exist; overwrite the file if it does.)

Context: inventory NuGet packages, framework targets, and any bundled or vendored binaries across all projects in the solution, including setup/installer projects. Check versions against known advisories if you have a way to look them up; otherwise flag what to check.

Lens checklist:
Dependencies: known vulnerabilities (CVEs), outdated or end-of-life packages and
target frameworks, unused or redundant packages, version mismatches across projects,
unpinned or floating versions, licensing concerns, vendored code, supply-chain risks

For each finding report:
- Title and severity (critical/high/medium/low)
- Confidence (high/medium/low), and what you'd check to confirm if not high
- Location (file:line) and a short code excerpt
- Why it matters, with a concrete risk scenario (what could go wrong and when)
- Suggested fix (brief, with a sketch if helpful)
- Effort (S/M/L)

Rules: only report issues you can point to in the code; no generic advice.
Separate "confirmed issues" from "worth investigating". Skip style nitpicks
unless they hurt maintainability. Finish with the 5 highest-impact items.

Write the complete report to the report file above. In your reply, give only
the file path and a 3-line summary.
```
