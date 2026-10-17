---
name: fix-telemetry
description: "Diagnose an error group from OneMore telemetry (ERR-nnn), propose a fix plan, and on approval create the GitHub issue and a local branch. Reads the exception, environment, dates, and occurrence count from the Errors sheet of telemetry-errors.xlsx."
---

# Diagnose a telemetry error group

Given an `ErrorGroup` key (`ERR-nnn`), read its rows from the telemetry workbook,
diagnose the exception against the source, and produce a fix plan. After the user
approves the plan, create the GitHub issue and a local branch. Do not edit code,
commit, push, or run `build.ps1`.

## Step 1 — Get the ErrorGroup

Ask for a single input unless the invocation already supplied it:
"Which ErrorGroup (ERR-nnn)?" Accept a bare number (`184` or `ERR-184`) and normalize
it to `ERR-` plus at least three digits (`ERR-184`, `ERR-007`).

## Step 2 — Load the rows

Workbook: `C:\Github\OneMoreTelemetry\telemetry-errors.xlsx`, sheet `Errors`. It is
read-only for this skill; never modify it. Columns:

| Column | Use |
|--------|-----|
| `EventId`, `Timestamp` | one row per occurrence; `Timestamp` is `yyyy-MM-dd HH:mm:ss.fff` |
| `EventName` | the command that failed (e.g. `ApplyStyle`) |
| `YourCodeStack` | exception type, message and stack trace |
| `Environment` | `Version`, `OneVersion`, `OneArc`, `MoreArc`, `Culture`, `MoreCulture`, `OS` |
| `StackHash`, `ErrorGroup`, `GroupSize` | grouping |
| `Status`, `Notes` | triage state; `Notes` may name an existing issue (`... · Issue #nnnn`) |

Read with the ImportExcel PowerShell module (installed), e.g.:

```powershell
$rows = Import-Excel 'C:\Github\OneMoreTelemetry\telemetry-errors.xlsx' -WorksheetName Errors |
    Where-Object ErrorGroup -eq 'ERR-184'
```

Fall back to Python `openpyxl` only if ImportExcel is unavailable. From the rows derive:

- **Exception:** `YourCodeStack` of the first row (rows in a group share a `StackHash`)
- **Environment:** `Environment` of the first row; if rows differ, list the distinct
  values and how many rows each covers
- **Start / end:** earliest and latest `Timestamp` (parse them; do not trust row order,
  the sheet is sorted newest first)
- **Occurrences:** number of rows (cross-check with `GroupSize`)

Stop and tell the user if no rows match. If `Status` is `Fixed`/`Fixd`/`Ignore` or
`Notes` already references an issue, show that and ask whether to continue.

## Step 3 — Parse the exception

- Identify the exception type, message, and any inner exceptions.
- Separate `River.OneMoreAddIn.*` frames from framework and COM frames
  (`System.*`, `Microsoft.Office.Interop.OneNote`). The top-most OneMore frame is the
  likely fault site.
- For `COMException`, decode the HRESULT (OneNote errors are typically `0x8004200x`).
- Release builds may lack line numbers; say so rather than guessing lines.

## Step 4 — Interpret environment and timing

- **Version:** find the matching release tag (`git tag`, `git log`). Compare the
  implicated files at that tag with `main` (`git show <tag>:<path>`) to see whether
  the bug is already fixed or changed. Use `git log --since/--until` on the
  implicated files to see whether the error window begins at a release (regression).
- **Bitness:** follow the CLAUDE.md rules. The add-in runs in `dllhost.exe`, not
  `ONENOTE.EXE`. Apply the ARM64EC heuristic when reasoning about architecture,
  and do not use `RuntimeInformation.ProcessArchitecture` for OneNote's bitness.
- **Culture:** the logged culture is only a name; it does not reflect user overrides
  such as a custom decimal separator.
- **Frequency:** occurrences ÷ days in the window; also note how many distinct
  sessions/users the rows suggest (a burst in minutes is probably one user). Classify
  as one-off, low, sustained, or spike. Telemetry is opt-in, so real impact is higher;
  the `estimate-usage` skill describes scaling by opt-in rate (about 10–20%).

## Step 5 — Read the code

Prefer Glob/Grep/Read over shell commands. For each OneMore frame, locate the class
and method under `OneMore/`, read the whole method and its callers, and check
`git log -L` or `git blame` on the fault lines for recent changes. Check how the
exception reached telemetry (`AddIn.cs`, `Commands/CommandFactory.cs`,
`Commands/Command.cs`, `Helpers/Logger.cs`).

## Step 6 — Diagnose and present the plan

State the root cause with a confidence level, or ranked hypotheses when evidence is
thin. Each hypothesis must cite specific code or data. Common candidates: null
reference, stale COM object or page ID, OneNote busy or page locked, culture/locale
parsing, DPI scaling, bitness mismatch, missing resource, STA/threading, unexpected
page XML shape. If the evidence cannot decide, say what extra data would (log
excerpt, page XML, repro steps) instead of guessing.

Present the plan concisely with:

- **Summary** — a short phrase of a few words describing the failure, used for the
  issue title and branch name (e.g. `ApplyStyle fails silently`)
- **Evidence** — ErrorGroup, exception, window, count and rate, severity
- **Root cause** — with evidence as `file_path:line_number`
- **Proposed fix** — files and approach; distinguish a real fix from defensive guards
- **Regression test** — idea for OneMoreTests (see the `make-test` skill)
- **Risks** — side effects, plus any extra `Logger` diagnostics worth adding

Then request approval. In plan mode, use ExitPlanMode; otherwise ask the user in chat.
Do not continue to Step 7 until the plan is approved.

## Step 7 — After approval: issue and branch

Use PowerShell and the `gh` CLI with the default repo `stevencohn/OneMore`.

1. **Create the issue** (approval of the plan is the go-ahead; no further question):
   - Title: `<ErrorGroup> <summary>`, e.g. `ERR-184 ApplyStyle fails silently`
   - Body: the plan sections above (telemetry details, stack of OneMore frames, root
     cause, proposed fix, regression test), using a single-quoted here-string
   - `--label bug --assignee stevencohn`

   ```powershell
   gh issue create --repo stevencohn/OneMore --title "ERR-184 ApplyStyle fails silently" `
       --label bug --assignee stevencohn --body $body
   ```

2. **Create and switch to a local branch** named `<Issue#>-<summary>`: the issue number,
   then the summary lowercased with spaces replaced by hyphens and punctuation removed,
   kept short (e.g. `2751-applystyle-fails-silently`). Use
   `git switch -c <name>` from the current branch; carry any uncommitted changes along
   and never discard them. Do not push.

3. Report the issue URL and branch name. Mention that the workbook's `Status` and
   `Notes` for the group have not been updated (the skill never edits the workbook), so
   the user can record the issue number there.

## Constraints

- Never edit `telemetry-errors.xlsx`.
- Do not edit code, commit, or push as part of this skill; implementing the fix is a
  separate request.
- Resource strings: edit `Properties\Resources.resx` and `Resources.Designer.cs`
  together, and never touch `Resources.xx-XX` files.
- Never edit `docs\*.htm`.
