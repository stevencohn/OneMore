# OneMore Code Review Plan

Break the review into stages. A single "review everything for everything" prompt tends to give shallow, generic findings, because the model spreads its attention across every lens and every file. Separate passes go deeper and are easier to verify. Each lens pass works from a shared architecture brief (the recon output) and writes its report to the `findings/` folder, where the synthesis pass picks it up.

## Recommended structure

1. **Recon pass.** Map the project: purpose, entry points, modules, data flow, dependencies, and how to build and test it. Save the output as `onemore-architecture-brief.md` in this folder; every later pass reads it.
2. **Lens passes, one per `lens-*.md` file.** Run each in its own session. The order doesn't matter and they can run in parallel. Do not feed one lens's findings to another; sharing only the brief keeps each pass from anchoring on the others. Each pass writes its report to `findings/<lens>.md`:
   - `lens-architecture.md`
   - `lens-security.md`
   - `lens-performance.md`
   - `lens-robustness.md`
   - `lens-maintainability-design.md`
   - `lens-testing.md`
   - `lens-dependencies.md`
3. **Synthesis pass.** Reads everything in `findings/`, dedupes, ranks by impact and effort, and writes `findings/synthesis.md`.
4. **Verification pass (optional but worthwhile).** Have fresh agents try to disprove each high-severity finding from the synthesis. Run one finding by hand (4a) or all of them in parallel with subagents (4b). Each verdict goes to `findings/verification/<ID>.md`, and 4b merges them into `findings/verification.md`. This removes most false positives.

For a big codebase, also split by module within each lens.

Run architecture first only if you plan to act on structural changes before fixing line-level issues. Otherwise run everything in parallel and let synthesis treat the architecture findings as the top-level frame.

## Which model

See the table under "2a Models" below. In short: Opus 5.5 for architecture, security, performance, recon and synthesis; Sonnet 5.5 for the rest. Skip Fable 5.2 for the security pass. It has extra cybersecurity safeguards, which could get in the way of a vulnerability review.

If you run this in Claude Code, you can spawn the lens passes as parallel subagents and have the main session run the synthesis.

## The prompts

### 1. Recon ✓

Model: Sonnet 5.5. Run first.

Save the result as `onemore-architecture-brief.md` in this folder.

```
You are a senior engineer onboarding to the "OneMore" codebase. Do NOT review
or change anything yet. Explore the repo and produce a concise architecture
brief:
- What the project does, its language/stack, and how to build, run and test it
- Directory/module map with the responsibility of each
- Entry points, main data flows, external services, storage, and config/secrets handling
- Key dependencies and anything unusual or surprising
- Which areas look highest-risk or most complex (to prioritize later reviews)
Cite file paths. Keep it under ~800 words. Note anything you couldn't determine.
```

### 2. Lens passes

The prompts live in the `lens-*.md` files in this folder. Run each one with the model shown in 2a, in any order. This template shows how every lens file is built:

```
You are a senior engineer doing a <LENS> review of "OneMore".

First, read the architecture brief at:
C:\Github\OneMore\specs\design-review\onemore-architecture-brief.md

Review only through this lens. Read the actual code; don't guess from file names.
Read-only: do not modify any existing files. The only file you may create is
your report, at:
C:\Github\OneMore\specs\design-review\findings\<lens>.md
(Create the findings folder if it doesn't exist; overwrite the file if it does.)

Context: <one or two lines specific to this lens>

Lens checklist:
<this lens's checklist only>

For each finding report:
- Title and severity (critical/high/medium/low)
- Confidence (high/medium/low), and what you'd check to confirm if not high
- Location (file:line) and a short code excerpt
- Why it matters, with <lens-specific scenario wording>
- Suggested fix (brief, with a sketch if helpful)
- Effort (S/M/L)

Rules: only report issues you can point to in the code; no generic advice.
Separate "confirmed issues" from "worth investigating". Skip style nitpicks
unless they hurt maintainability. Finish with the 5 highest-impact items.

Write the complete report to the report file above. In your reply, give only
the file path and a 3-line summary.
```

Keep the lens checklists in the `lens-*.md` files only, so there is one source of truth. "Dependency CVEs" belongs to the dependencies lens, not security.

#### 2a Models
| File | Model | Why |
|---|---|---|
| ✓ `lens-architecture.md` | Opus 5.5 | Whole-system reasoning across many projects |
| ✓ `lens-security.md` | Opus 5.5 | Subtle, cross-file reasoning where missed findings cost the most |
| ✓ `lens-performance.md` | Opus 5.5 | Needs to trace hot paths and interop costs across layers |
| ✓ `lens-robustness.md` | Sonnet 5.5 | Pattern-based checks (error handling, leaks, edge cases) that Sonnet handles well |
| ✓ `lens-maintainability-design.md` | Sonnet 5.5 | Mostly local judgments about coupling, duplication and naming |
| ✓ `lens-testing.md` | Sonnet 5.5 | Largely mechanical: run the tests, map coverage, spot gaps |
| ✓ `lens-dependencies.md` | Sonnet 5.5 | Inventory and version checking, with little deep reasoning |

Each file also has its model in a "Model:" line above the prompt block, so it matches this table.

For security and performance in particular, Opus is worth the extra cost. If budget matters, those two are where to keep it. You can run any of the Sonnet lenses on Opus too, though you'll likely see only a modest gain.

Run independently: each lens pass is a separate session, and the synthesis pass is another. The synthesis pass reads all the lens reports and merges them into a single prioritized list. The easiest way keep sessions separate is to run in sequence with a `/clear` in between, or spawn subagents for each lens and have the main session run synthesis.

### 3. Synthesis ✓

Model: Opus 5.5. Run after all lens passes have finished.

```
You are a senior engineer synthesizing a code review of "OneMore".

Read every report in:
C:\Github\OneMore\specs\design-review\findings\
(ignore synthesis.md, verification.md and the verification folder if they exist)

Merge them: dedupe overlapping findings, resolve conflicts, and re-rank by
impact vs. effort. Treat the architecture findings as the top-level frame and
group related findings from other lenses under them where they overlap.
Read-only: do not modify existing files. Write your output to:
C:\Github\OneMore\specs\design-review\findings\synthesis.md

Output:
1. Top 10 priorities with rationale
2. Quick wins (small effort, real value)
3. Larger architectural improvements, in a suggested order, with dependencies
4. Findings that look weak or speculative, so I can discard them
Flag any finding you think is a false positive. For each item, name the
source report(s) so it can be traced back.

In your reply, give only the file path and a short summary.
```

### 4. Verification

Model: Opus 5.5 (Sonnet 5.5 is acceptable for lower-risk findings). Run after synthesis.

#### 4a. Verify one finding

Use this to check a single finding by hand, or as the instruction each parallel agent follows in 4b. Replace `<ID>` with the finding's ID or title from `synthesis.md`.

```
You are a skeptical reviewer of "OneMore". Read
C:\Github\OneMore\specs\design-review\findings\synthesis.md and the source
report it cites for this finding: <ID>

Read the code and try to disprove the finding: is it reachable, is it already
handled elsewhere, are the assumptions valid? Verdict: confirmed / partially /
false positive, with evidence.

Read-only: do not modify existing files. Write your verdict to:
C:\Github\OneMore\specs\design-review\findings\verification\<ID>.md
(create the folder if it doesn't exist; overwrite the file if it does).
In your reply, give only the ID and the verdict.
```

#### 4b. Verify in parallel (Claude Code subagents)

Run this in one main session. It launches one subagent per finding, so you don't run them one at a time. Each agent has a clean context and writes its own file, so parallel runs can't overwrite each other.

```
Read C:\Github\OneMore\specs\design-review\findings\synthesis.md.
List every critical and high severity finding, up to 10, with a short ID for each.

For each one, launch a separate subagent, all in parallel in a single step.
Tell each subagent to read the "4a. Verify one finding" prompt in
C:\Github\OneMore\specs\design-review\README.md and follow it exactly, using
the finding's ID in place of <ID>.

When all subagents finish, read the files in
C:\Github\OneMore\specs\design-review\findings\verification\
and merge them into
C:\Github\OneMore\specs\design-review\findings\verification.md
as one table: ID, title, verdict, one-line evidence.
In your reply, give only the table.
```

Notes:

- Cap the batch at about 10 findings. A very large fan-out costs a lot, and lower-severity findings are rarely worth verifying.
- Keep Opus for security and architecture findings, where a wrong verdict costs the most.
- Drop findings marked false positive and keep working on the confirmed and partial ones.

## Tips

- Tell it how to run the tests. If it can execute them, it can check its own claims.
- If you know the project's goals (scale, threat model, who uses it), add that to the recon. It changes what counts as a problem.
- Reports only, no edits to source. Do fixes in a later step, one finding at a time.
- `findings/` is generated output. Decide whether to commit it or add it to `.gitignore`.
- Re-running a lens overwrites its report. Copy the old one aside first if you want to compare runs.
