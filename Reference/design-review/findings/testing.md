# OneMore — Testing review

Lens: testing (coverage of critical paths, brittleness, seams, CI).
Read-only review; only this report was written.

## Test run result

- **Built and ran successfully.** I ran the already-built `OneMoreTests\bin\Debug\OneMoreTests.dll` (built 2026-11-22) with VS 18 `vstest.console.exe /Platform:x64`.
- **Result: 1154 / 1154 passed, 0 failed, ~19 s test time (22 s wall).** No `[Ignore]` attributes; no `Thread.Sleep`/`Task.Delay` in tests.
- I did **not** rebuild (the owner builds manually), so results reflect the last build, not necessarily HEAD.
- Only one test project exists. `OneMoreCli`, `OneMoreTray`, `OneMoreCalendar`, `OneMoreProtocolHandler` and `OneMoreSetupActions` have no tests (just `AsrEvaluator.cs` is source-linked into OneMoreTests).
- Every `*.cs` file in OneMoreTests is listed in the csproj today (checked), so no test is silently excluded right now.

## Confirmed issues

### 1. CI never runs tests on push/PR; tests are an opt-in checkbox on a manual release workflow — HIGH
- Confidence: high
- Location: `.github/workflows/build.yml:3-26` (only `workflow_dispatch`), `:23-26` (`run_tests` default `false`), `:264-267`
```yaml
on:
  workflow_dispatch:
...
      run_tests:
        default: false
...
      - name: Run tests
        if: matrix.arch == 'x64' && inputs.run_tests
```
- No other workflow builds or tests (the rest are issue/PR-link, pages, winget, cleanup). The test step also runs *after* signing, MSI build and kit finishing, so even when enabled it does not gate those steps.
- Why it matters: 1154 passing tests protect nothing on main. Example: a `PageEditor` refactor that breaks `ClearFormatting` merges and ships unless the author runs tests locally.
- Fix: add a `pull_request`/`push` workflow on `windows-2025-vs2026` running `build.ps1 -Fast` then `-Test`, uploading the `.trx`. In the release workflow run tests before signing and default them on. Tests need no OneNote install (MockApplication), so a hosted runner should work (verify on a clean clone).
- Effort: S

### 2. Test csproj is old-style with one `<Compile Include>` per file — a new test file not added is silently never run — MEDIUM
- Confidence: medium (nothing is missing today; the risk is forward-looking)
- Location: `OneMoreTests/OneMoreTests.csproj` (~100 `<Compile Include=...>` entries)
- Why it matters: forgetting the csproj entry (easy when a file is created outside VS) gives a green build with the tests never compiled; it also causes merge conflicts.
- Fix: a guard in `build.ps1 -Test`/CI that fails if a `*Tests.cs` on disk is not in the csproj, or convert to SDK-style globbing (verify it still builds through `build.ps1`).
- Effort: S (guard) / M (conversion)

### 3. `MockApplication` is permissive in ways that hide real OneNote failure modes — MEDIUM
- Confidence: high
- Location: `OneMoreTests/MockApplication.cs:95-133, 254-295`
```csharp
public void UpdatePageContent(string bstrPageChangesXmlIn, DateTime dateExpectedLastModified, ...)
{ StorePageXml(bstrPageChangesXmlIn); }
...
catch { /* malformed XML in tests is a test bug; swallow ... */ }
...
public void FindPages(..., out string pbstrHierarchyXmlOut, ...) { pbstrHierarchyXmlOut = string.Empty; }
```
- Problems:
  1. `dateExpectedLastModified` and `force` are ignored, so the optimistic-concurrency failure (page changed underneath the command) cannot be tested.
  2. `StorePageXml` swallows invalid XML. A command that emits corrupt page XML (the brief's top data-loss risk) still passes any test that only checks "an update happened"; real OneNote rejects it.
  3. Real OneNote validates against its schema (misordered/unknown elements throw `COMException`); the mock accepts anything.
  4. `FindPages`/`FindMeta` return `string.Empty`, which `XElement.Parse` rejects, so code using them is untestable without ad-hoc overrides.
  5. There is no way to inject `COMException`s, so the retry loop in `Helpers/ApplicationFactory.cs` (`hrCOMBusy`, `hrRpcFailed`, …) and `OneNote` error handling have no tests.
- Fix: add `Mock.FailNext(method, exception)`; record calls (update count, last XML) rather than swallowing; make `StorePageXml` throw on malformed XML; optionally a strict mock validating against the schemas in `Reference/`.
- Effort: M

### 4. No tests for the IPC/protocol dispatch surface (`CommandService`, `CommandFactory.Invoke`) — HIGH
- Confidence: high (no test file references `CommandService`; `CommandFactory` appears once, in a comment, `ConvertMarkdownCommandTests.cs:124`)
- Location: `OneMore/Commands/CommandService.cs` (776 lines), `Commands/CommandFactory.cs:389-410`
```csharp
var name = $"River.OneMoreAddIn.Commands.{action}";
var type = Type.GetType(name, false);
...
if (type.GetCustomAttribute(typeof(CommandServiceAttribute), false) == null) ...
```
- Only `ProtocolUri.TryParse` (7 tests) is covered. Untested: the `[CommandService]` allowlist, `onemorecli://` parsing and `ERR:` responses, `InvokeCliCommandForNotebook`/`InvokeCliBackupExport`, cancellation/heartbeat, `MaxBytes=4096` limit, and `CreateSecuredPipe` ACL (`CommandService.cs:160`).
- Why it matters: it is the highest-risk external surface. Removing the attribute check would let any local pipe client instantiate any command and nothing would fail; a namespace rename silently breaks every `onemore://` link because the type name is built by string concatenation.
- Fix: extract pure parse/validate (`IsInvocable(Type)`, CLI request parsing) and test: non-attributed type rejected, unknown type logged not thrown, oversize/malformed messages. Add a reflection test that every command referenced by `onemore://` links carries `[CommandService]`. Add 2-3 real `NamedPipeServerStream` tests with a random pipe name (needs the pipe name to be injectable instead of read from the registry).
- Effort: M

### 5. Updater checksum parsing and architecture selection untested — HIGH
- Confidence: high (no test references `Updater`/`ParseChecksum`)
- Location: `OneMore/Commands/Tools/Updater/Updater.cs:363-397`
```csharp
var pattern = $@"(?im)^-\s*{Regex.Escape(key)}(?:\s+bundle)?\s*=\s*([0-9A-Fa-f]{{64}})\s*$";
```
- Why it matters: this guards an installer run via `msiexec`. A regex regression yields `NotFound`/wrong match and either blocks updates or weakens integrity. The ARM64EC mapping (`Machine.Amd64 when OSArchitecture == Arm64 => ARM64`, per CLAUDE.md) exists in `Updater.cs` and `CheckBitnessAction.cs` with no test pinning it; a wrong pick installs the wrong bitness.
- Fix: make `ParseChecksum`/`VerifyChecksum` `internal static` (`InternalsVisibleTo("OneMoreTests")` already exists) and add table tests: `x64 = <hash>`, `ARM64 bundle = <hash>`, mixed case, 63-char hash, missing section, CRLF, key `x64` vs similar keys. Extract one shared `MapMachine(Machine, Architecture)` and test i386, Arm64, Amd64-on-Arm64, Amd64-on-x64.
- Effort: S–M

### 6. Page-editing core and `OneNote.cs` have thin direct coverage — HIGH
- Confidence: medium-high (`PageEditor.cs` is 1446 lines and referenced by 4 test files; `OneNote.cs` is 2508 lines and `OneNoteTests.cs` has 3 tests)
- Why it matters: every command goes through `OneNote.GetPage`/update and `PageEditor`. Command tests assert on output XML but rarely on edge cases: missing page (the mock returns a `null` string), section-group ancestry, `FindPagesByPath`, hyperlink building, runs split across selection/CDATA.
- Fix: direct tests for `OneNote`'s pure XML/hierarchy logic and for `PageEditor` run splitting using `PageBuilder`/`HierarchyBuilder`.
- Effort: M–L

### 7. Command coverage is thin: ~30 `*CommandTests.cs` for ~211 command classes — MEDIUM
- Confidence: medium (counted files; some commands are tested via helper classes, so real coverage is somewhat higher)
- No tests found for: `RunPluginCommand` (600 lines; runs external scripts with temp files and timeouts), `TelemetryClient`, `ReminderService`, `NavigationService`, `ServiceLoop`, Office import/export, `NavigatorWindow` (UI). Manual `SmokeTestProtocols/*.one` notebooks cover some by hand.
- Fix: prioritize by blast radius: `RunPluginCommand` (timeout/kill, temp cleanup, malformed output), `ServiceLoop`, `ReminderService`.
- Effort: L

### 8. Global statics and real-filesystem access limit isolation and prevent parallel runs — MEDIUM
- Confidence: medium
- Location: `Helpers/Logger.cs:26-31` (static instance), `Helpers/DatabaseProvider.cs:27` (`protected static readonly string path = ResolveDatabasePath()`), `Helpers/ApplicationFactory.cs` static `singleton`; tests touching temp/AppData: `DatabaseBackupTests.cs:24`, `FavoritesFileProviderTests.cs:22`, `PageIdentityProviderTests.cs`, `ConvertMarkdownCommandTests.cs`, `EnexReaderTests.cs`.
- Why it matters: tests pass because they run serially and `Teardown` resets the factory. MSTest parallelism would break on the process-global `ApplicationFactory.RegisterApplication`, and any test that forgets an in-memory connection could touch the developer's real `OneMore.db`. (Good practice present: many DB tests use `Data Source=:memory:`; others use GUID-named temp paths.)
- Fix: in `[AssemblyInitialize]` redirect the DB/AppData path to a per-run temp dir (override/env var) and fail if the real folder is touched; make the application singleton `AsyncLocal` if parallelism is wanted.
- Effort: M

### 9. Failure-path tests are sparse in command tests — LOW–MEDIUM
- Confidence: medium (sampled `AdjustBlankLines`, `RemoveInk`, `RemoveTags`, `ToggleDttm`, `Trim`)
- Strength: these have good "nothing to change, so no `UpdatePageContent` call" tests. Gap: none feed malformed/partial page XML (no outline, missing meta) or assert behavior when the update throws.
- Fix: once the mock supports failure injection (item 3), add one "update throws COMException" test per command family.
- Effort: M

### 10. No test categories to separate fast unit tests from filesystem/DB tests — LOW
- Confidence: medium
- No `[TestCategory]` anywhere. The suite takes 19 s so this is minor today; revisit if integration tests (named pipes, real SQLite files) are added.
- Effort: S

## Worth investigating

- **FTS5/SQLite on ARM64/x86:** CI tests are x64-only (`build.yml:265`). Check whether `IndexProviderTests` asserts FTS5/trigram is actually available; if it silently falls back, an ARM64 regression ships unnoticed.
- **`CommandFactory.RunCore` extra `Task.Run`** ("not sure why this fixes it", `CommandFactory.cs:158`): no test pins the intended threading or that exceptions reach the catch-all handler once.
- **Replay** (`CommandFactory.cs:333-346`, `Type.GetType` + `Enum.Parse` from stored names): check with a fixture MRU file containing an unknown/renamed command type that load does not throw.
- **Telemetry allowlist:** CI diffs `telemetry-events.json` after compile (`build.yml:107-112`); confirm the diff catches removed events, since the generator is additive only.
- **Clean-clone build:** the test csproj hint-paths the OneNote interop to `..\OneMore\bin\Debug\...` so it only builds after the main project; verify on a clean runner.
- **Satellite apps** (`OneMoreTray/ScanningJob`, `OneMoreProtocolHandler` URL to pipe-message formatting, `OneMoreCalendar`) have no tests; extract the pure logic and test it.

## Strengths worth keeping
- `PageBuilder`/`HierarchyBuilder` and `TestBase` keep command tests concise and XML-level.
- Pure-logic units are well covered (Calculator/formulas, TitleQueryParser, TextMatchBuilder, Identity/Pipeline via `FakeHierarchy`, hashtag providers, themes).
- Suite is fast, deterministic (no sleeps), no `[Ignore]`, in-memory SQLite and GUID temp paths.

## Top 5 highest-impact items
1. **Run the tests in CI on every push/PR** (item 1): small effort, turns 1154 existing tests into a real regression gate.
2. **Test IPC dispatch and the `[CommandService]` allowlist** (item 4): highest-risk surface with zero coverage.
3. **Test Updater checksum parsing and the shared ARM64EC mapping** (item 5): small, protects the install path and a load-bearing rule.
4. **Make `MockApplication` strict and failure-injectable** (item 3): unlocks concurrency, COM-error and corrupt-XML tests for the page-editing core (item 6).
5. **Guard against test files missing from the csproj and isolate global state (AppData/DB path, `ApplicationFactory`)** (items 2, 8): prevents silent coverage loss and accidental use of the developer's real database.
