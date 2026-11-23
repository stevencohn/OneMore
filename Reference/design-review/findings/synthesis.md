# OneMore — Design Review Synthesis

Date: 2026-11-23 · Branch: `main` @ `39847cd1`

Inputs (abbreviations used for traceability):

| Abbrev | Report | IDs |
|---|---|---|
| **ARCH** | `architecture.md` | A1–A12, W1–W6 |
| **SEC** | `security.md` | C1–C8, W1–W5 |
| **PERF** | `performance.md` | P1–P10, W1–W6 (numbered list) |
| **ROB** | `robustness.md` | R1–R13, W1–W8 |
| **MAINT** | `maintainability-design.md` | #1–#10, W1–W6 |
| **TEST** | `testing.md` | #1–#10, "worth investigating" |
| **DEP** | `dependencies.md` | #1–#6, A–I |

References look like `ARCH A3`, `ROB R2`, `MAINT #5`.

## How I checked conflicts

Before ranking, I checked the claims that the reports disagree on against the code:

| Claim | Check | Result |
|---|---|---|
| ARCH §0: "CI does run the tests" vs TEST #1: "CI never runs tests on push/PR" | `.github/workflows/build.yml:3-4,23,265` | **TEST is right.** The only trigger is `workflow_dispatch`, and the test step runs only when `inputs.run_tests` is set (default false). ARCH's statement is misleading. |
| `PipelineService` is outside `ServiceLoop` (ARCH A1) | grep for `ServiceLoop`/`ApplicationExit` | **Confirmed.** Only `CommandService`, `NavigationService` and `ReminderService` call `ServiceLoop.Start`. The pipeline cancels on `Application.ApplicationExit` only (`PipelineService.cs:115`). |
| `ProgressDialog` worker has no catch (ROB R1) | `UI/ProgressDialog.cs:140-163` | **Confirmed.** The outer `try` wraps only the thread's construction, not the body of the `async` lambda. |
| The CLI path does not check `ICliCommand` (ARCH A5, SEC C4, MAINT W2) | `CommandService.cs:228-271` | **Confirmed.** It resolves the type by name and builds the factory. The only gate is `IsValidAction`. MAINT W2's caution ("maybe enforced lower down") does not hold. |
| `nam`/`name` typo (MAINT #1) | `CommandProvider.cs:68-74` | **Confirmed.** The `om…` fallback can never match. |
| `onemore:` is trusted in all Office applications (SEC W1) | `TrustedProtocolAction.cs:32,168` | **Confirmed** at the code level. Exploitability is still unverified. |
| `WaitForPipeDrain` is synchronous (ROB R7) | `CommandService.cs:740` | **Confirmed.** |
| `OneNote.cs` size: MAINT says 2,128 lines, TEST says 2,508 | line count | **2,508.** MAINT's number is wrong, but its finding still stands. |

Smaller disagreements and how I resolved them:

- **Settings severity.** ARCH and ROB rate it High, MAINT Medium-High, PERF Low-Medium. I used **High**, because the risk is data loss. PERF's caching suggestion has to come *after* locking, which PERF P8 itself warns about.
- **CLI allowlist.** SEC rates it Low, ARCH Medium, TEST High (for coverage). I made it a **quick win**: the fix is one line, and the issue only becomes serious if the pipe ACL is ever loosened.
- **Pipe naming.** ARCH A4 frames it as availability and concurrency, SEC C1 as squatting. Both are real and have the **same fix**, so I merged them.
- **WebView2 bootstrapper.** SEC W4 rates it Low, DEP #6 Medium. I used **Low–Medium**, and listed it as a quick win because the fix is small.

---

## Architectural frame

I used ARCH's findings as the top-level themes and grouped related findings from the other lenses under each one. The Top 10 and the later sections pull from these groups.

| Theme | ARCH anchor | Related findings |
|---|---|---|
| **T1. Background lifecycle and threading** | A1, A7, A10, A12, W3 | ROB R1, R3, R8, R9, R12; MAINT W1, W3; PERF W1 |
| **T2. Shared mutable state across processes (settings, DB)** | A3, W2 | ROB R2, R10, W2, W5; MAINT #5; PERF P8; TEST #8 |
| **T3. IPC boundary (pipe, protocol, CLI)** | A4, A5, A6, W1 | SEC C1, C4, W1, W3, W5; ROB R7, W6; MAINT #2, W2; TEST #4 |
| **T4. Command identity and registry** | A9, W4 | MAINT #1, #6, #7; ROB W8; TEST replay item |
| **T5. COM connection model and the `OneNote` wrapper** | A8 | ROB R4, R5, R11, W1; MAINT #3, #4; PERF P10, W4; TEST #3, #6 |
| **T6. Hierarchy data access (no single source of hierarchy data)** | A8 (indirectly) | PERF P1, P2, P4, P6, P7, P3, P5, P9, W3, W5 |
| **T7. Assembly boundaries and layering** | A2, A11, W6 | MAINT W4, W5, #10; DEP I; TEST satellite-apps item |
| **T8. Install, update and supply chain** | (none) | SEC C2, C3, C5, C8, W4; ROB R6; DEP #1–#6, A, H; TEST #5 |
| **T9. Test and CI safety net** | (none) | TEST #1–#10; MAINT W6 |
| **T10. Page-editing core** | (none) | MAINT #8, #9; TEST #6 |
| **T11. Telemetry hygiene** | (none) | SEC C6, C7; ROB W4 |

---

## 1. Top 10 priorities

The list is ranked by impact relative to effort. Several items are bundles, because one change fixes findings reported by several lenses.

### 1. Run the existing tests as a CI gate on every push and PR
- **Sources:** TEST #1 (ARCH §0 contradicts it, and is wrong). Related: TEST #2.
- **Effort:** S.
- **Why first:** 1,154 tests already pass in about 19 s and need no OneNote, because they use `MockApplication`. Right now they protect nothing. Every other item on this list is a refactor of code that many commands depend on. A regression gate makes them safe to do.
- **What to do:**
  - Add a `push`/`pull_request` workflow that runs `build.ps1 -Fast` then `-Test`.
  - In the release workflow, default `run_tests` to on and move the test step before signing.
  - Add the csproj-missing-file guard from TEST #2 at the same time.

### 2. Make settings writes atomic and locked, and never save after a failed read
- **Sources:** ARCH A3, ROB R2, MAINT #5 (PERF P8 is related, but comes later).
- **Effort:** S–M.
- **Why:** A failed read becomes an in-memory default, and a later `Save()` writes that default over the user's real settings. That can wipe favorites, plugins and backup options without any warning. The number of writers keeps growing: background pipeline, tray, CLI, settings dialog, and `CommandProvider.SaveToMRU` after every command.
- **What to do:**
  - Write to a temp file, then `File.Replace` with a `.bak`.
  - If loading fails, mark the provider read-only, or restore from the `.bak`.
  - Take a named cross-process mutex around reload, merge and save, and merge by collection rather than saving the whole snapshot.
  - Add a schema `version` attribute.
  - Extend the same atomic-write helper to `SnippetMacros`, `TableThemeProvider`, `ThemeProvider` and `RecentColors` (ROB W5).

### 3. Catch exceptions inside the `ProgressDialog` worker
- **Sources:** ROB R1. Related: ARCH A7, which names the same `async`-lambda-on-a-`Thread` trap in `CommandService.cs:147`.
- **Effort:** S. One fix covers about 20 call sites.
- **Why:** An exception in any unguarded callback (`SearchAndReplaceCommand`, `PageColorCommand`, `CompareDialog`, …) is an unhandled thread-pool exception, and that can kill dllhost partway through a batch. If the process survives, the TopMost modal dialog stays on screen. The trap is confirmed in the code.
- **What to do:**
  - Wrap the body in `try`/`catch`.
  - Marshal `DialogResult` and `Close()` back to the dialog's thread.
  - Remove `thread.Abort()`.
  - Fix the same pattern at `CommandService.cs:147`.

### 4. Put the pipeline under one stop mechanism, and stop permanently disabling stages
- **Sources:** ARCH A1, ROB R8. Related: ROB R10.
- **Effort:** S.
- **Why:** `ServiceLoop.StopAll()` exists because `ApplicationExit` is unreliable in dllhost, yet the heaviest worker, the pipeline, relies on that event. When dllhost lingers, two pipelines can end up running against one DB, and the orphaned one keeps activating OneNote. Separately, a stage that fails five times in a row (about 10 minutes of OneNote being busy) is switched off until restart. It has no timeout, so a hung COM call wedges every later cycle.
- **What to do:**
  - Register the pipeline's `CancellationTokenSource` with `ServiceLoop`.
  - Replace the permanent "off" with an exponential cool-down.
  - Give each stage a deadline through a linked token.
  - Also fix R10: an `AbandonedMutexException` in the static `ResolveDatabasePath` initializer breaks every database provider for the whole session, which then trips R8.

### 5. Harden the updater: Authenticode check, no click-through on a mismatch, a sound download, quoted paths
- **Sources:** SEC C2, SEC C5, ROB R6, TEST #5.
- **Effort:** S–M.
- **Why:** This path ends in an elevated install.
  - The hash comes from the same origin as the binary.
  - `NotFound` is a normal state, so users learn to click Yes.
  - A non-2xx response body, or a stale tail left in the file by `File.OpenWrite`, produces a mismatch that the user can then accept.
  - The `.cmd` script uses unquoted `%TEMP%` paths.
- **What to do:**
  - Verify the signer against a pinned thumbprint or subject.
  - Refuse to install on `Mismatch`.
  - Call `EnsureSuccessStatusCode`, write with `FileMode.Create`, and download to a `.part` file in a random subfolder.
  - Run `msiexec` directly with an argument list, with no `.cmd`.
  - Add table tests for `ParseChecksum` and a shared, tested `MapMachine` for the ARM64EC rule (TEST #5).

### 6. Navigator: stop reading every notebook's hierarchy on each navigation and each dropdown open
- **Sources:** PERF P2 (S–M) and PERF P1 (M). Related: PERF P4, W3.
- **Effort:** S–M for P2, M for P1.
- **Why:** This is the largest continuous performance cost found. Every page visit (polled every 1.25 s) runs `GetHierarchy(null, hsPages)` across all notebooks plus a content fetch. The ribbon `getContent` callback runs the same reads synchronously on OneNote's UI thread. The cost grows with total notebook size, not with the page being visited.
- **What to do:**
  - Do P2 first: the menu reads only `Navigator.json` and the pinned list, and deleted pages are purged in the background.
  - Then P1: work out "is this page deleted?" during the one ancestor walk, and get `name` from `hsSelf`.

### 7. Harden the IPC boundary: CLI allowlist now, then a per-user pipe, a server check, and framing
- **Sources:** SEC C4, ARCH A5 (guard), ARCH A4, SEC C1, ROB R7, ARCH W1, SEC W3, TEST #4. Related: ROB W6.
- **Effort:** S for the guard, M for the rest.
- **Why:**
  - Any type in `Commands.*` can be run over `onemorecli://`.
  - The pipe name is machine-global and single-instance, so on RDS/Citrix the first user owns it (squatting and outages).
  - `WaitForPipeDrain` has no timeout, so one stalled client hangs every Refresh link and CLI call.
  - Requests are read once into 4 KB, so long or CJK paths are cut off silently.
- **What to do, in order:**
  1. Require `ICliCommand` before `Activator.CreateInstance` (one line).
  2. Drop `WaitForPipeDrain` or put a timeout on it, and read until a delimiter with a length cap.
  3. Derive the pipe name per user and session in a shared helper, have clients verify the server's SID, and allow more than one server instance.
  4. Move to a framed, versioned protocol (ARCH W1).
- **Tests:** add tests for the allowlist and for oversize requests (TEST #4). This needs an injectable pipe name.

### 8. Restrict the `onemore:` trusted protocol to OneNote
- **Sources:** SEC W1.
- **Effort:** S for the key, M for the per-command review.
- **Why:** `TrustedProtocolAction.cs:32` writes the trusted-protocol key under `All Applications`. That means a link in an email, a Word document or a shared page runs `[CommandService]` commands with one click and no Office warning. Some of those commands write to Outlook contacts or send content to kroki.io/PlantUML. The code is confirmed. Whether it can be exploited depends on the per-command side effects. The fix is cheap compared with the exposure.
- **What to do:**
  - Scope the key to OneNote, if the Office version supports per-application keys.
  - Ask the user to confirm protocol calls that act outside OneNote.

### 9. Fail fast and clearly when OneNote is unavailable, and stop ignoring `Update` failures
- **Sources:** ARCH A8 (S part), ROB R5, ROB R4, ROB R11. Related: MAINT #4.
- **Effort:** S for the exception and the constructor, M for the caller pass.
- **Why:**
  - `CreateApplication` returns `null` after its retries, which shows up later as an NRE and a generic error, and telemetry shows nothing useful.
  - The `out Page` constructor blocks on `.Result`, leaks the COM object (RCW) when it throws, and wraps errors in `AggregateException`.
  - About 196 callers ignore a `false` from `Update`, so a failed save is reported as success.
- **What to do:**
  - Throw a typed `OneNoteUnavailableException`, and map it to a clear message in `CommandFactory`.
  - In the constructor, `try { … } catch { Dispose(); throw; }` and use `GetAwaiter().GetResult()`.
  - Make an unexpected `Update` failure throw, or go through one reporting helper.
  - Replace `Thread.Sleep` with `Task.Delay`.

### 10. Elevated installer actions: impersonate the DB backup, and verify the WebView2 signature
- **Sources:** SEC C3, SEC W4, DEP #6.
- **Effort:** S.
- **Why:** These are the only two places where SYSTEM-level code works on user-controlled input.
  - A deferred custom action with `Impersonate="no"` copies between paths in the user's profile, which allows a junction or symlink attack.
  - A downloaded executable runs elevated with no signature check.
- **What to do:**
  - Move the backup to an immediate, impersonated custom action. SYSTEM is not needed for it.
  - Call `WinVerifyTrust` and require the "Microsoft Corporation" signer before running the WebView2 bootstrapper.

**Just below the cut:** PERF P4 (reminders re-read every hierarchy every 5 minutes), MAINT #3 (hierarchy lookup copied in 10+ places, and the copies already differ), and ROB R9 (the backup stage blocks the pipeline on a modal dialog).

---

## 2. Quick wins (S effort, real value)

Most of these are under an hour each. Items that are already part of the Top 10 are not repeated here.

| # | Item | Source | Note |
|---|---|---|---|
| 1 | Fix the `$"om{name}…"` typo to use `nam` | MAINT #1 | Verified bug |
| 2 | Add a test that every `AddIn.*Cmd` has a label and a matching `Command` type | MAINT #1, ARCH A9 | First step toward the registry |
| 3 | `Command.ShowMessage`: remove the dead `box` setup or make it real. Make `ShowInfo`/`ShowMessage` respect `runningFromCli` | MAINT #7 | Today a command run from the CLI can show a modal box |
| 4 | Plugin timeout: `Kill()` the process tree, and unhook `Exited` before disposing the dialog | ROB R3 | Fixes orphaned `powershell.exe` processes |
| 5 | Backup stage: no `Join` on the UI thread, warn once per session, treat a `Prune` failure as a warning | ROB R9, ARCH A7 | Removes hourly modal dialogs and pipeline stalls |
| 6 | `HttpClientFactory` and `ThemeManager` → `Lazy<T>`. Make `AddIn.Telemetry` read-only | ARCH A12, ROB R12 | Low impact, trivial fix |
| 7 | Find the host PID without depending on hotkey registration | ARCH A10 | The host watcher currently turns off when hotkeys fail |
| 8 | Fallback GUID format in `PrepareHierarchyCache` | ROB R13 | `Substring(1,-2)` throws |
| 9 | `OpenImageWithCommand`: use `UseShellExecute` instead of `cmd /c start` with an unquoted path | SEC C8 | |
| 10 | Register the protocol handler as `"exe" "%1"` | SEC W5 | Quoting fix |
| 11 | Scrub paths and URLs from telemetry exception messages, with a unit test | SEC C6 | Privacy |
| 12 | Gate full XSD validation to the retry after `hrXmlIsInvalid` | PERF P5 | Measure first, using the existing `updateElapsedMs` |
| 13 | Search: build section path and color from in-memory ancestors, and drop `Link` | PERF P6 | Removes about 3k COM round trips per all-notebooks search |
| 14 | Identity pass: reuse the first match when `rescue.Count == 0` | PERF P7 | |
| 15 | Ribbon images: compute `IsBlackThemeEnabled` once, and cache search images (this also fixes a stream leak) | PERF P9 | |
| 16 | `PaletteTrace` → its own file plus `[Conditional]`. Delete it when #2926 closes | MAINT #6 | |
| 17 | Pin the WiX extensions to `7.0.0`, and add lock files plus `nuget.config` source mapping | DEP #1 | Reproducible installers |
| 18 | Add `dependabot.yml` (nuget and github-actions) and an advisory scan step | DEP #2 | |
| 19 | Remove the `System.Net.Http` 4.3.4 PackageReference | DEP #3 | Smoke-test the updater, telemetry and Kroki afterwards |
| 20 | Fail fast in `build.ps1` when the hard-coded SDK or `stdole` paths are missing | DEP #5 | |
| 21 | Add `PLUGIN_API_VERSION=1` to the plugin environment | ARCH W5 | Cheap insurance for future changes |
| 22 | Use `BeginImmediate` and re-read the version inside the hashtag schema upgrades | ROB W2, ARCH W2 | Small step before a real migrator |
| 23 | Delete the stray `OneMoreService/`, `y.csproj.user` and root `SettingsDialog.Designer.cs`. Add `OneMoreCli` and `OneMoreBundle` to the `CLAUDE.md` topology table | ARCH W6, MAINT W5, DEP I | Doc and repo drift |
| 24 | Wrap the final `WriteCliResponse` so a client disconnect doesn't trigger the 60 s listener back-off | ROB W6 | |

---

## 3. Larger architectural improvements: suggested order and dependencies

```
Phase 0  Safety net ───────────────┐
Phase 1  Lifecycle + UI dispatcher │  (independent of 2)
Phase 2  Settings engine ──────────┤
Phase 3  OneNote session / COM ────┼──► Phase 4  Single hierarchy source
Phase 5  Command registry ─────────┴──► Phase 6  IPC v2 + CLI batch runner
Phase 7  Layering cleanup ─────────────► Phase 8  OneMore.Core / OneMore.UI extraction ──► (.NET 10 path)
Side track: Page-editing core (needs Phase 0)
```

### Phase 0: Safety net (prerequisite for everything below)
- **Work:**
  - CI gate (Top 10 #1).
  - Make `MockApplication` strict and able to inject failures: throw on malformed XML, honor `dateExpectedLastModified`, add `FailNext(method, exc)`, return valid empty XML from `FindPages`/`FindMeta` (TEST #3).
  - Redirect AppData and the DB path to a per-run temp folder in `[AssemblyInitialize]` (TEST #8).
- **Sources:** TEST #1, #2, #3, #8, #9.
- **Unblocks:** testing every refactor below, especially COM error paths (Phase 3) and page-editing edge cases.

### Phase 1: One lifecycle model and one UI thread
- **Work:**
  1. Grow A1's token registration into an `AddIn`-owned list of hosted services with `Start` and `StopAsync`, torn down in `StopServices`.
  2. Add a `UiDispatcher`: one STA thread running `Application.Run`. The same pattern already exists in `HotkeyManager`. Remove the seven or more hand-written STA-thread blocks.
  3. Services report through an `INotifier` interface (UI in the add-in, log-only in the tray and CLI) instead of calling `MoreMessageBox`.
  4. After this, revisit `Environment.Exit(0)` in `OnDisconnection` (ARCH W3) and the "not sure why this fixes it" `Task.Run` (MAINT W1).
- **Sources:** ARCH A1, A7, W3; ROB R1, R9; MAINT W1; TEST (RunCore threading).
- **Depends on:** Phase 0 (tests). Top 10 #3 and #4 are the S-sized first steps.
- **Unblocks:** Phase 7, because the `INotifier` change removes the Services→UI dependency.

### Phase 2: Settings engine
- **Work:**
  - Build on Top 10 #2: a process-wide cached root, invalidated by `Save()` and an mtime check.
  - An `Update(collection, Action<XElement>)` API.
  - Migrations that run once under the lock.
  - Move the hard-coded `nameof(SomeSheet)` and string-literal keys to constants.
- **Sources:** ARCH A3 (items 3 and 4), PERF P8, MAINT #5.
- **Depends on:** atomic writes and locking from Top 10 #2. Caching before locking would make lost updates worse (PERF P8).

### Phase 3: COM connection model and the `OneNote` wrapper
- **Work:**
  1. Typed exception and constructor fix (Top 10 #9).
  2. `OneNote.OpenWithPage` async factory, replacing the `out Page` constructor at 129 sites.
  3. `CommandFactory` owns one scoped `OneNote` session per invocation and passes it in. This also removes the throwaway owner-window activation (PERF P10).
  4. Extract `HierarchyResolver` (`FindNotebook`, `FindSectionNode`, `WithHierarchyRetry`) to replace the 10+ copies that already differ, and move the page-name matching rules into a pure class with tests.
  5. Extract an `IOneNote` interface over the roughly 15 calls commands actually use.
- **Sources:** ARCH A8; ROB R4, R5, R11, W1; MAINT #3, #4; PERF P10, W4; TEST #6.
- **Depends on:** Phase 0, for the failure-injecting mock.
- **Unblocks:** Phase 4 and the CLI batch runner (Phase 6).

### Phase 4: One source of hierarchy data
- **Work:**
  - Make the `IdentitySnapshot` the shared source of hierarchy data.
  - Turn reminders into a pipeline stage that reads the snapshot (PERF P4).
  - Navigator purges deleted pages from the snapshot (PERF P1/P2 follow-up).
  - Remaining callers of `GetRecycledPageIds` prune the recycle-bin subtree once (PERF W3).
- **Sources:** PERF P1, P2, P4, W3.
- **Depends on:** the P2 and P1 quick fixes (Top 10 #6) and Phase 3's resolver.

### Phase 5: Command registry
- **Work:**
  - Put the metadata on command classes: `[OneMoreCommand(Id, Label, Hotkey, Menu, Surfaces)]`.
  - Build the registry once at startup.
  - Ribbon, palette, hotkeys, protocol, CLI and Replay all resolve through it. Each surface checks its own flag.
  - MRU and Replay store the registry id instead of the CLR type name, which removes the stack-trace walk.
  - Use one generic `onAction` dispatcher, or generate the `…Cmd` forwarders, and add a test that checks the ribbon XML against the registry.
  - Replace `Command`'s chained setters with a `CommandContext`.
- **Sources:** ARCH A5, A9, W4; MAINT #1, #7; ROB W8; SEC C4 (long-term form); TEST #4.
- **Depends on:** quick win #2 (the consistency test) and Phase 0.
- **Unblocks:** Phase 6, so the IPC surfaces can be enforced in one place.

### Phase 6: IPC v2 and a shared CLI batch runner
- **Work:**
  - Per-user/session pipe name, server-SID check, several server instances (Top 10 #7, part 3).
  - Length-prefixed, versioned JSON messages.
  - Split `InvokeCliCommand` into `CliRequest.Parse`, `CliTargetResolver`, `CliBatchRunner` and a thin pipe layer.
  - One `CliBatchRunner` with an `ICliSink`, shared by `CommandService` and `OneMoreCli/Program`. This also removes the drift that already exists between them: retries, `.Distinct()` and error reporting.
- **Sources:** ARCH A4, A6, W1; SEC C1, W3; ROB R7, W6; MAINT #2; TEST #4.
- **Depends on:** Phase 3 (resolver) and Phase 5 (registry). The shared runner also needs a shared assembly, which is easier after Phase 8, though for now it could live in the add-in assembly.

### Phase 7: Layering cleanup
- **Work:**
  - Move `HashtagStage`, `HashtagScheduler`, `HashtagProvider`, `HealingStage` and the TOC heading model out of `Commands/`.
  - Remove the Services→UI dependency (via Phase 1's `INotifier`).
  - Add an architecture test in `OneMoreTests` that fails when `Models`, `Services` or `Helpers` reference `Commands` or `UI`.
  - Split `HashtagProvider`'s migrations into a `HashtagMigrator` (MAINT #9).
  - Add a coordinated `DatabaseMigrator.EnsureCurrent()` under the existing `Global\OneMoreDbMigration` mutex (ARCH W2, ROB W2).
- **Sources:** ARCH A11, W2; MAINT #9, #10; ROB W2.
- **Depends on:** Phase 1.

### Phase 8: Extract `OneMore.Core` and `OneMore.UI`
- **Work:**
  - Extract in stages: the tray first, then the CLI, then the Calendar.
  - Remove `InternalsVisibleTo` for everything except the tests.
  - Delete the duplicated `OneMoreCalendar/Things/*` controls.
- **Sources:** ARCH A2; MAINT W4; TEST satellite-apps item.
- **Depends on:** Phase 7. This is the structural enabler for the staged .NET 10 migration.

### Side track: Page-editing core
- **Work:**
  - Extract a direction-parameterized `WordSpan` helper from `PageEditor.EditNode`.
  - Replace the public `ReverseScanning` flag with a parameter, or reset it in `finally`.
  - Add direct tests for the pure word-boundary logic.
  - Split `ApplyStylesCommand` into a `CodeSpanStyler` and a non-command `PageThemer`, which also fixes the half-initialized nested command in MAINT #7.
  - Make page hashing lazy, with streaming hashes for savers (PERF P3). Audit every `Update` caller first, because hashing drives change tracking.
- **Sources:** MAINT #8, #9; TEST #6; PERF P3.
- **Depends on:** Phase 0 (strict mock). It is independent of Phases 1–8.

---

## 4. Weak, speculative or false-positive findings

### False positives or factual errors
| Finding | Source | Verdict |
|---|---|---|
| "CI does run the tests" (`build.yml:267`) | ARCH §0 | **False as stated.** The step exists but runs only on a manual dispatch with an opt-in checkbox that defaults to false. TEST #1 is correct. |
| `IndexProvider.LoadFts5` uses `ProcessArchitecture` | ROB W3 | **False positive.** Loading `SQLite.Interop.dll` has to match the add-in DLL's own process bitness, and that is exactly what `ProcessArchitecture` reports. The CLAUDE.md warning is about inferring *OneNote's* architecture, which this code does not do. ROB itself concedes that it is "correct for the DLL itself". |
| SQLite extension loading left enabled on a user-writable DB | DEP A (second half) | **Resolved by SEC.** SEC "no findings" confirms extension loading is turned on only for the duration of the call (`IndexProvider.cs:329-342`). The *SQLite version* half of DEP A still needs checking against advisories. Its CVE reference is from memory and unverified. |
| `OneNote.cs` is 2,128 lines | MAINT #4 | **Wrong number** (it is 2,508). The finding stands. |
| "Maybe `ICliCommand` is enforced lower down" | MAINT W2 | **Disproven.** It is not enforced before instantiation. See Top 10 #7. |

### Weak or speculative (fine to deprioritize or drop)
| Finding | Source | Why it's weak |
|---|---|---|
| Embed refresh used to exfiltrate a private page through a shared notebook | SEC W2 | The attacker needs the victim's private page IDs and edit access to a shared page. Low confidence, and no demonstrated path. Re-check after Top 10 #8. |
| `-Embedding` injection through protocol-handler argv | SEC W5 | The result is a 15 s wait. Only the quoting fix (quick win #10) is worth doing. |
| Embedded telemetry API key | SEC C7 | Intentional and documented. The mitigation is server-side (throttling, schema validation), not a code change. |
| Duplicate content when `UpdatePageContent` is retried after an RPC failure | ROB W1 | Low confidence, needs a contrived mock to show. Monitor only. |
| Settings and DB on roaming/network profiles | ROB W5 | Subsumed by Top 10 #2. The multi-machine part is speculative. |
| Replay clears the MRU when a locale-sensitive value fails to parse | ROB W8 | Minor. It goes away with Phase 5. |
| Telemetry fire-and-forget pile-up | ROB W4 | Needs an API outage plus a batch run. Low impact. |
| Racy lazy singletons | ARCH A12 | Real, but the worst outcome is a duplicate theme load or a header race. Kept as a quick win, not a priority. |
| `Environment.Exit(0)` in `OnDisconnection` | ARCH W3 | Matters only if the surrogate AppID is shared, which is unconfirmed. Revisit after Phase 1. |
| `async void` methods | MAINT W3 | Most are WinForms event handlers, which is fine. Check `CopyAsMarkdownCommand.WriteMarkdown` only. |
| Make `Logger.Current` injectable everywhere | MAINT #10 | High effort, low payoff. Do it opportunistically. The process-global CLI mirror matters only if CLI commands run concurrently, which a single-instance pipe prevents today. |
| `DisposeAsyncCore` only yields | MAINT #4, W5 | Cosmetic. |
| Binding redirects "have no effect" in dllhost | DEP #4 | Medium confidence, not traced at runtime, and the product ships and works. Investigate only if a load failure appears in telemetry. |
| InputSimulator, Vortice, Chinese package maintenance | DEP B, C, D | No concrete risk was shown. Low value unless ARM64 problems appear. |
| `seguisym.ttf` licensing | DEP F | A legal question, not an engineering one. Check why the font is bundled before spending effort. |
| Third-party notices | DEP G | Same as above. Worth one check, not a work item. |
| Specific ms/MB estimates in PERF P1, P3 and W1–W6 | PERF | The direction is sound, but the numbers are estimates from reading the code. Measure before committing to P3 in particular, because its fix touches change tracking. PERF W4 and W6 (RCW per poll, logger flushes) are unmeasured and probably small. |
| Pipe squatting on single-user desktops | SEC C1 | Real only on multi-session hosts (RDS, Citrix, fast user switching). The same fix as ARCH A4 covers it, so there is no separate work. |
| Add test categories | TEST #10 | The suite runs in 19 s. Not needed yet. |
