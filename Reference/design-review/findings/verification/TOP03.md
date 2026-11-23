Title: ProgressDialog worker lambda has no catch (async void on a raw Thread)
Verdict: confirmed

# TOP03 — Catch exceptions inside the ProgressDialog worker

Source: synthesis.md Top 10 #3; robustness.md R1 (related: architecture.md A7).

## Claim

`ProgressDialog.ShowDialogWithCancel` runs the caller's callback inside `new Thread(async () => ...)`
with no try/catch. An exception from the callback becomes an unhandled exception that can end the
dllhost process. If the process survives, the modal TopMost dialog is never closed.

## Evidence (attempts to disprove)

1. **The lambda is `async void`, and nothing guards its body.** `OneMore/UI/ProgressDialog.cs:144-163`.
   `Thread`'s constructor takes `ThreadStart` (void), so the async lambda compiles to `async void`.
   The `try` at line 140 wraps only the construction, `Start()` and `ShowDialog()` on the calling
   thread. It cannot see exceptions raised in the lambda body, and especially not after the first
   `await` (line 149), when the rest of the body runs on a thread-pool continuation.

2. **Unhandled means process termination.** The new thread has no `SynchronizationContext` (it never
   creates a control), so `AsyncVoidMethodBuilder` rethrows the exception on the thread pool. On
   .NET Framework 4.5+ that ends the process unless `legacyUnhandledExceptionPolicy` is set. A grep
   of the repo's `*.config` and `*.cs` files finds no such setting, and the host is `dllhost.exe`,
   whose config OneMore does not control. `AddIn.CatchUnhandledException` (`AddIn.cs:151-181`) only
   writes to the event log and the logger and calls `OnBeginShutdown`. It cannot stop termination
   (`IsTerminating` is reported, not changed).

3. **If the process survives, the dialog hangs.** `DialogResult = ...` and `Close()` (lines 153-162)
   come after the `await`. When the callback throws they are never reached, so `ShowDialog()` (line 172)
   never returns, unless the user clicks Cancel, which then calls `thread.Abort()` (line 178) on a
   thread that has already exited.

4. **Unguarded callbacks can throw.** `OneNote.GetPage` (`OneNote.cs:1079-1110`) catches COM errors
   and returns **null**. Callers then dereference it:
   - `SearchAndReplaceCommand.cs:334-337`: `editor.SearchAndReplace(page)` calls
     `GetRuns(page)`, which runs `new SelectionRange(page)` and `page.Root...`
     (`SearchAndReplaceEditor.cs:152-162`), so a null page gives a `NullReferenceException`.
   - `PageColorCommand.cs:99-102`: `progress.SetMessage(page.Title)` on a null page gives a `NullReferenceException`.
   Neither lambda has a try/catch. There are 21 call sites of `ShowDialogWithCancel`/`ShowTimedDialog`
   (CompareDialog x5, IndexSheet, LegacyTaggingConverter, RemoveDuplicates, ImportWeb, AddFormula,
   AddTopSectionGroup, OneNoteExtensions, etc.). Some catch internally (RecalculateFormula, ResetCursor,
   RunPlugin, DatabaseBackup), which is what the report says.

5. **The class already knows the right pattern.** The third mode, `OnShown` (lines 277-301), wraps
   `execute` in try/catch, sets `DialogResult = Cancel` and always calls `Close()`. The
   `ShowDialogWithCancel` path was simply never given the same guard.

## Corrections to the suggested fix

- "Marshal `Close()` back to the dialog's thread" is **already done**. `ProgressDialog.Close()` is
  overridden (lines 424-445) to `Invoke` when `InvokeRequired`. Setting `DialogResult` off-thread is
  only a field write when the form is not yet closing. The remaining work is the try/catch (with
  `ok = false` on exception) plus a `finally` that always runs the `DialogResult`/timer/`Close()` tail.
- Dropping `thread.Abort()` is reasonable cleanup but not load-bearing.

## Conclusion

The issue is reachable through ordinary failures (GetPage returning null on a locked or corrupted page)
in at least two commonly used commands. Nothing upstream handles it. The effort estimate (S, one edit
in `ProgressDialog.cs`) holds. Verdict: **confirmed**.
