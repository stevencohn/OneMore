Title: Fail fast when OneNote is unavailable; stop ignoring Update failures
Verdict: confirmed

Finding: synthesis.md Top 10 #9 (sources ARCH A8, ROB R5, ROB R4, ROB R11).

The three main claims hold. The mechanism behind the first one differs a little from what the reports say. That difference makes the failure quieter than they describe, not louder. Only R4's "leaks the RCW when it throws" sub-claim is weaker than stated: the path exists, but it is narrow.

## 1. `CreateApplication` returns null after retries: CONFIRMED

`OneMore/Helpers/ApplicationFactory.cs:28-76`
- When the 3 busy/RPC retries run out, the `while` loop ends and `return app` returns `null`. No log line says that all retries failed. The only log is the per-retry "retyring in {ms}ms" line.
- Any other exception, such as class-not-registered or access denied, is caught by the outer `catch (Exception)` at line 70. The factory logs it and still returns `null`.
- `Thread.Sleep(ms)` (line 66) blocks the caller synchronously for 250, 500 and then 750 ms.
- `OneNote()` (`OneNote.cs:157-161`) stores the null with no guard. `ReplaceApplication()` (`OneNote.cs:495-508`) can also replace a live proxy with null in the middle of a retry.

How the null propagates (this corrects the "NRE" description):
- The window properties go through `WithCurrentWindow`, which returns the fallback when `onenote is null` (`OneNote.cs:352`). So `CurrentPageId` is `null`.
- `GetPage(string, ...)` returns `null` when `pageId` is empty (`OneNote.cs:1081`). So `new OneNote(out page, ...)` succeeds quietly with `page == null`, which looks the same as "empty section" (see the comment at line 173).
- Most callers dereference `page` right away, for example `page.IsValid` in `ClearFormattingCommand.cs:41` and `RemoveIndentsCommand.cs:31`, or `new SelectionRange(page)`. The NRE happens there, far from the cause.
- Other calls go through `InvokeWithRetry`. Its `catch (Exception)` (line 475) turns the NRE from `onenote.X` into a logged `false`. The failure is then hidden completely.
- `Update` calls `Marshal.IsComObject(onenote)` (line 1997), which throws `ArgumentNullException` on null.

`CommandFactory.cs:271-299` has only a generic catch-all that shows `Resx.Command_ErrorMsg` ("error running command X"). Nothing there identifies "OneNote unavailable". The proposed typed exception plus a mapping in `CommandFactory` is valid.

## 2. The `out Page` constructor: CONFIRMED (sync-over-async); PARTIAL (RCW leak)

`OneNote.cs:170-176`: `page = Task.Run(async () => await GetPage(detail)).Result;`
- `.Result` blocks, and it wraps any exception in `AggregateException`. The only `catch (AggregateException)` in the add-in is at `Updater.cs:160`, which is unrelated.
- There is no try/catch around the body. If it throws, `Dispose` never runs on the instance that is half built, so the `IApplication` RCW from `this()` is left to the finalizer.
- This is reachable but narrow:
  - `GetPage(string, ...)` catches all exceptions except `ThreadAbortException` unless `FallThrough` is set. `FallThrough` is always set through an object initializer, which runs after the constructor, so it can never apply here.
  - `WithCurrentWindow` catches only `COMException`. An `InvalidComObjectException`, which is not a `COMException`, or another non-COM exception from `onenote.Windows`/`CurrentWindow` would propagate through `.Result`.
- There are 129 `new OneNote(out ...)` call sites, so the pattern is used widely.
- The proposed fix (`try { ... } catch { Dispose(); throw; }` plus `GetAwaiter().GetResult()`) is correct and cheap.

## 3. Callers ignore a `false` from `Update`: CONFIRMED

`Update` (`OneNote.cs:1977-2033`) returns `false` in these cases:
- active media, which is the only case that shows a message box (line 1981)
- schema validation failure (line 1999)
- `InvokeWithRetry` failures: `hrXmlIsInvalid`, `hrRpcFailed2`, `hrObjectMissing`, exhausted retries, or any other exception (lines 435-488)

None of these throw, and none except active media tell the user.

Grep over `OneMore/**/*.cs`:
- 197 bare-statement calls match `^\s*await \w+\.Update\(\w+` and discard the result.
- Only a handful check it: `HierarchyDiffSync.cs:432`, `HierarchyLinkReconciler.cs:232`, `CopyFolderCommand.cs:338`, `RepairUrlsResultsDialog.cs:282`, and the `UpdateWithProgress` wrapper (`OneNoteExtensions.cs:31-59`). That wrapper also returns bool, and its callers mostly discard it too.

The figure of about 196 is accurate. Because no exception escapes, `CommandFactory` records the run as a success in telemetry. The aggregate `updateCount` telemetry (`FlushUpdateTelemetry`) counts attempts and does not separate failures.

## Attempts to disprove
- Is a null app handled elsewhere? Only `WithCurrentWindow` and `Dispose`/`ReplaceApplication` check for null. Nothing upstream, in `CommandFactory` or `AddIn`, checks for OneNote availability before a command constructs `OneNote`.
- Is it reachable? Yes. COM busy and RPC-unavailable are exactly the HRESULTs the factory retries on, so exhausting the retries is an expected environmental failure.
- Does `Update` report failure some other way? No, except for active media.
- Is "replace `Thread.Sleep` with `Task.Delay`" valid? Yes. `CreateApplication` is synchronous and is called from a constructor, so the change needs an async factory. That matches the M/L effort the reports give.
