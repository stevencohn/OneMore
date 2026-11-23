# OneMore robustness review

Lens: error handling, edge cases, retries and timeouts, races, resource leaks, partial failure, idempotency, input bounds.
Scope: I read the code listed below. The rest of the repo is unreviewed. Read-only; no source files were changed.

Files read: `CommandService.cs`, `CommandFactory.cs`, `ServiceLoop.cs`, `PipelineService.cs`, `PageContentPass.cs`, `DatabaseProvider.cs`, `DatabaseBackup.cs`, `BackupStage.cs`, `Updater.cs`, `HttpClientFactory.cs`, `TelemetryClient.cs`, `RunPluginCommand.cs`, `SettingsProvider.cs`, `ApplicationFactory.cs`, `ProgressDialog.cs`, parts of `OneNote.cs`, `AddIn.cs`, `HashtagProvider.cs`, `HashtagScheduler.cs`, `OneMoreProtocolHandler/Program.cs`, `OneMoreCli/AddinBridge.cs`, and spot checks of a few commands.

Not reviewed: `OneMoreTray`, `OneMoreCalendar`, `ReminderService`, `NavigationService`, `IdentityPass`/`IndexProvider` internals, `PageEditor`, the setup actions.

---

## Confirmed issues

### R1. Worker callbacks run on a raw `async` thread with no catch, so an exception can kill dllhost and leave the dialog stuck — HIGH
- **Confidence:** High on the mechanism (the code is visible). Medium on how often it happens: each callback has to throw, and many callbacks do catch.
- **Location:** `OneMore/UI/ProgressDialog.cs:144-163`
  ```csharp
  var thread = new Thread(async () =>
  {
      var ok = await action(this, source.Token);   // no try/catch
      DialogResult = ...;
      Close();
  })
  ...
  if (result == DialogResult.Cancel) { source.Cancel(); thread.Abort(); }
  ```
- **Why it matters:**
  - The lambda is `async void`. A thread running it ends at the first `await`, and the rest runs on the thread pool. This is the same trap the `PipelineService.Startup` comment describes.
  - An exception from `action` therefore becomes an unhandled thread-pool exception. On .NET Framework 4.5+ that terminates the process. The process is dllhost, which hosts the add-in. `AddIn.CatchUnhandledException` only logs and calls shutdown.
  - If the process survives, `Close()` is never reached. A `TopMost` modal progress dialog then stays on screen.
  - `thread.Abort()` does nothing useful because the thread has already exited. Cancel works only if the callback polls the token.
  - Some callbacks catch internally (`RecalculateFormulaCommand.cs:62-79`, `ResetCursorCommand`, `RunPluginCommand.ExecuteWorker`, `DatabaseBackup.Backup`). Others do not:
    - `PageColorCommand.cs:95-116`
    - `SearchAndReplaceCommand.cs:313-349` (`GetPage` returns null or throws, then `editor.SearchAndReplace(page)` throws, or `Update` throws)
    - The five `CompareDialog.cs` call sites and others, not checked.
- **Failure scenario:** The user runs Search and Replace across a notebook. One page is locked or in a corrupted state, so `one.GetPage` throws `COMException` or returns null. The add-in host process terminates mid-batch, or the dialog hangs. Pages processed so far stay modified.
- **Fix:** Wrap the body in `ProgressDialog` once.
  ```csharp
  bool ok; try { ok = await action(this, source.Token); }
  catch (Exception exc) { logger.WriteLine("progress action failed", exc); ok = false; }
  ```
  Then set `DialogResult` and call `Close()` through `BeginInvoke` if `InvokeRequired`. Drop `thread.Abort()`.
- **Effort:** S. One fix covers about 20 call sites.

### R2. `SettingsProvider` writes non-atomically and silently falls back to defaults when the file can't be read — HIGH
- **Confidence:** High on the code. Medium on frequency.
- **Location:** `OneMore/Commands/Settings/SettingsProvider.cs:40-50, 59-66, 169-173`
  ```csharp
  try { root = XElement.Load(path); } catch (Exception exc) { Logger...WriteLine(...); }
  if (root == null) root = new XElement("settings", ...defaults...);
  ...
  root.Save(path, SaveOptions.None);   // overwrites in place
  ```
- **Why it matters:**
  - Every call site builds its own `SettingsProvider`, which loads the whole file. It then changes one collection and saves the whole file.
  - Two threads or processes can run this at once. Examples are the pipeline stage `BackupStage` via `RecordSuccess`, the `Updater`, `HashtagStage` reading settings, the tray, and the settings dialog.
  - A crash, power loss or kill during `Save` leaves a truncated file.
  - A reader that opens the file mid-write gets an `XmlException`. The constructor then swallows it and uses defaults. If that instance later calls `Save`, it overwrites the real settings with defaults. Nothing is backed up, so favorites, plugin definitions, backup options and so on are lost.
  - Concurrent read-modify-write also loses updates (A loads, B loads, B saves, A saves).
- **Failure scenario:** `DatabaseBackup.RecordSuccess` runs on a pool thread at the same moment the user clicks OK in Settings. One of the two saves is lost, or one reads a half-written file and later saves defaults.
- **Fix:**
  - Write to `path + ".tmp"`, then `File.Replace` or `File.Move`.
  - Keep a `.bak` of the last good file, and restore from it when the load fails.
  - Guard load and save with a cross-process mutex (as `DatabaseProvider` already does for migration).
  - Never save a provider whose load failed.
- **Effort:** S–M.

### R3. Plugin timeout does not kill the process, and the temp file is deleted under it — MEDIUM
- **Confidence:** High.
- **Location:** `OneMore/Commands/File/Plugins/RunPluginCommand.cs:352-363, 74-77, 369-374`
  ```csharp
  await process.WaitForExitAsync(token);
  ok = !token.IsCancellationRequested;
  ...
  finally { process?.Dispose(); }     // no process.Kill()
  ```
- **Why it matters:**
  - On timeout or Cancel the token fires and `Dispose()` runs. `Dispose` only releases the handle. The plugin (PowerShell, python, etc.) keeps running.
  - `Cleanup(workpath)` then deletes the working file. A late-writing plugin recreates it or fails.
  - The orphan can run forever, holding CPU.
  - `Process_Exited` dereferences `progress` and calls `progress.Close()` from a thread-pool thread. That races with disposal of the dialog.
- **Failure scenario:** A plugin has a 60 s `Timeout` and hangs on network I/O. OneMore shows "unsuccessful" and a zombie `powershell.exe` stays running per attempt.
- **Fix:** On cancellation call `process.Kill()` (or kill the process tree), guarded by `HasExited`. Unhook `Exited` before disposing the dialog.
- **Effort:** S.

### R4. `OneNote(out Page, ...)` constructor blocks with `.Result` and leaks the COM object on failure — MEDIUM
- **Confidence:** High.
- **Location:** `OneMore/OneNote.cs:170-176`
  ```csharp
  : this()
  {
      page = Task.Run(async () => { return await GetPage(detail); }).Result;
  ```
- **Why it matters:**
  - If `GetPage` throws, the constructor never completes, so `Dispose` is never called. The `IApplication` RCW created by `this()` is left to the finalizer. The code comments elsewhere warn that finalizer release can stall OneNote.
  - `.Result` wraps the error in `AggregateException`. Callers that catch `COMException` miss it and fall into the generic catch-all.
  - Other `.Result` / `.Wait()` uses (`FootnoteEditor.cs:44`, the ribbon galleries `AddInRibbonStyleGallery.cs:63,74`, `AddInRibbonTableGallery.cs:58`, `QuickNotesSheet.cs:65,74`) block UI or ribbon callbacks on a COM round trip.
- **Fix:** Wrap the body in `try { ... } catch { Dispose(); throw; }` and use `GetAwaiter().GetResult()` to unwrap the exception. Prefer an async factory over the constructor.
- **Effort:** S.

### R5. `ApplicationFactory.CreateApplication` returns null silently after retries fail — MEDIUM
- **Confidence:** High on the code. Medium on how the null propagates.
- **Location:** `OneMore/Helpers/ApplicationFactory.cs:28-76`; `OneMore/OneNote.cs:159`
- **Why it matters:**
  - When three busy or RPC retries are exhausted, the `while` loop just exits and `app` stays null. There is no final log line saying all retries failed.
  - `OneNote()` stores null in `onenote`. The first `onenote.Something` call throws `NullReferenceException`, far from the cause.
  - The user sees the generic "error running command X" message. Telemetry records an NRE with no hint that OneNote was unavailable.
  - `Thread.Sleep` (250 + 500 + 750 ms) blocks the caller, which can be the ribbon or UI thread.
- **Fix:** After the loop, throw a descriptive exception (`InvalidOperationException("OneNote is not available", lastExc)`), or return null and make `OneNote` guard against it. Make the retry async.
- **Effort:** S.

### R6. Updater download: no status check, no truncation, buffered under a 100 s timeout, unquoted script path — MEDIUM
- **Confidence:** High for the first three. Medium for the path quoting.
- **Location:** `OneMore/Commands/Tools/Updater/Updater.cs:292-296, 319-350`
  ```csharp
  using var response = await client.GetAsync(asset.browser_download_url);   // no EnsureSuccessStatusCode
  using var stream = await response.Content.ReadAsStreamAsync();
  using var file = File.OpenWrite(installerPath);                           // does not truncate
  await stream.CopyToAsync(file);
  ...
  await writer.WriteLineAsync(installerPath);                               // unquoted
  Arguments = $"/c {path}",                                                 // unquoted
  ```
- **Why it matters:**
  - A 403/404/rate-limit HTML body is written to `%TEMP%\<asset>`. The checksum then reports Mismatch. The user is shown the `confirmProceed` prompt and could accept it.
  - `File.OpenWrite` opens existing files without truncating. If an earlier, larger download is still in `%TEMP%` (an aborted or older release), the new file has stale trailing bytes. The checksum fails and an MSI with trailing junk may be run.
  - The default `HttpClient.Timeout` is 100 s and `GetAsync` buffers the whole response. A slow link fails with `TaskCanceledException` and a vague "false" result.
  - If `%TEMP%` contains a space (some profiles with 8.3 names disabled), `cmd /c C:\Users\Jane Doe\...\OneMoreInstaller.cmd` and the `installerPath` line in the script both break.
  - Also: `release.assets` can be null and `new Version(plainver)` (lines 224-225) can throw `FormatException`. Both sit outside the try block in `FetchLatestRelease`/`Update`. The `productCode` parse (line 79, `cmd.Substring(cmd.IndexOf('{'))`) throws if there is no `{`, and it runs in the constructor.
- **Fix:**
  - `response.EnsureSuccessStatusCode()`.
  - Use `FileMode.Create`, `HttpCompletionOption.ResponseHeadersRead`, and a per-call `CancellationTokenSource` with a larger timeout.
  - Download to a `.part` file and move it after the checksum passes.
  - Quote paths in the script and in `Arguments`.
  - Guard the version parse and the `IndexOf`.
- **Effort:** S–M.

### R7. `CommandService` blocks on `WaitForPipeDrain` and truncates long requests silently — MEDIUM
- **Confidence:** High for the blocking call. Medium for the truncation impact.
- **Location:** `OneMore/Commands/CommandService.cs:732-746, 97-114`
  ```csharp
  await pipe.FlushAsync();
  pipe.WaitForPipeDrain();      // synchronous, no timeout, holds pipeWriteLock
  ...
  var buffer = new byte[MaxBytes];   // 4096
  bytesRead = await server.ReadAsync(buffer, 0, MaxBytes, cts.Token);   // single read
  ```
- **Why it matters:**
  - `WaitForPipeDrain` blocks until the client reads everything. A CLI client that connects and then stalls or is suspended blocks the one listener thread forever. All Refresh links and CLI calls then hang, because `ListenOnce` never returns.
  - `ServiceLoop`'s cancellation cannot interrupt it. A leftover dllhost holds the pipe and blocks the next OneNote instance, which is the case `StopAll` was written to handle.
  - The request is read once into 4 KB. `onemorecli://` URIs are percent-encoded, and each non-ASCII char costs 9 bytes. A long `outpath`, `infile` or CJK notebook/section name is cut off with no error. The command then runs against a truncated path.
  - The `onemore://` branch spawns a new thread per message with no cap (line 147). That is only reachable by the same user (the pipe has a user-only ACL), so it is low risk.
- **Fix:** Drop `WaitForPipeDrain` or run it under a timeout (for example `Task.Run` with `Task.WhenAny` and a delay). Read until the pipe closes or a delimiter, with a length cap and an explicit "request too large" error.
- **Effort:** S–M.

### R8. Pipeline stages are permanently switched off after five consecutive failures, with no timeout per stage — MEDIUM
- **Confidence:** High on the code. Medium on real-world frequency.
- **Location:** `OneMore/Services/Pipeline/PipelineService.cs:307-319` and `RunStage` at 285-324
  ```csharp
  if (state.Failures >= MaxFailures) { state.Off = true; ... }   // never re-enabled
  ```
- **Why it matters:**
  - `IdentityStage`, `HealingStage` and the content stage run every 2 minutes (`IdentityStage.PassInterval`). Five failures is about 10 minutes of OneNote being busy, a sync, or a modal dialog. After that the stage is off until OneNote restarts.
  - `IdentityStage` feeds the hashtag, index and favorites healing. Later stages that depend on `IdentitySnapshot` silently stop (`HealingStage.Run` returns when `snapshot` is missing).
  - The log is the only signal. There is no UI.
  - There is also no per-stage timeout. A COM call that hangs wedges the whole cycle. The re-entrancy guard (`running`) then makes every later cycle skip with only a log message.
- **Fix:** Use a cool-down instead of a permanent off. For example after five failures wait 30 minutes and retry once, then double. Wrap `stage.Run` in a linked token with a deadline.
- **Effort:** S.

### R9. Background backup shows modal dialogs from the pipeline thread — MEDIUM
- **Confidence:** High.
- **Location:** `OneMore/Services/DatabaseBackup/DatabaseBackup.cs:279-282, 343-368`; `BackupStage.cs:42-52`
  ```csharp
  else if (outcome == BackupOutcome.Failed)
      MoreMessageBox.ShowWarning(owner, Resx.BackupSheet_Failed);   // owner == null in the pipeline
  ...
  thread.Start(); thread.Join();   // Warn(): blocks the pipeline thread until the user dismisses
  ```
- **Why it matters:**
  - `Warn` joins the pipeline thread until the user closes the box. Nothing else in the pipeline runs, because the cycles are sequential, so identity, hashtag and index updates stall.
  - The `Failed` branch has no "once per session" guard (`warned` applies only to `Inaccessible`). A full disk or a locked file produces a modal warning every hour, and a progress dialog pops up unprompted each time a backup runs.
  - If `Backup()` copied the file but `Prune` failed (or listing files threw), the outcome is `Failed`. `RecordSuccess` is skipped, so the next hourly cycle writes another full copy.
- **Fix:** In the background path, log and show a non-blocking toast or tray balloon at most once per session. Don't `Join`. Record success once the copy is moved into place and treat `Prune` errors as warnings.
- **Effort:** S.

### R10. Static `ResolveDatabasePath` can leave every database provider unusable for the session — LOW
- **Confidence:** High.
- **Location:** `OneMore/Helpers/DatabaseProvider.cs:27, 77-117`, specifically `mutex.WaitOne()` at line 94, outside the `try`.
- **Why it matters:** `mutex.WaitOne()` throws `AbandonedMutexException` if another OneMore process (tray, a previous dllhost) died holding `Global\OneMoreDbMigration`. `PathHelper.EnsurePathExists` can throw too (access denied). Both run in a static field initializer. The result is a `TypeInitializationException` for `DatabaseProvider`, and the type stays broken until the process restarts. Hashtags, identity, index and backup are all dead for the session. Pipeline stages then fail five times and turn off (R8).
- **Fix:** Move `WaitOne` into the `try`, catch `AbandonedMutexException` (the lock is still acquired in that case), and make path resolution lazy and retryable.
- **Effort:** S.

### R11. About 196 `await one.Update(page)` calls ignore a `false` result — LOW to MEDIUM
- **Confidence:** High that the result is ignored. Medium on how often `false` occurs.
- **Location:** `OneMore/OneNote.cs:1977-2033` (returns false), for example `Commands/Clean/RemoveHyperlinksCommand.cs:52`.
- **Why it matters:** `Update` returns false for active media (it shows a box), schema-validation failure, XML-invalid (`0x80042001`), an RPC failure, or a missing object. It returns false after three retries. Most commands discard it, then log "removed N hyperlinks" or report success. The user sees nothing change and no error. Telemetry shows a successful run.
- **Fix:** Throw from `Update` for unexpected failures (keeping the false-for-user-cancelled cases), or add a helper that shows `Resx.Command_ErrorMsg`. A mechanical pass over callers is possible.
- **Effort:** M.

### R12. `HttpClientFactory` lazy init is not thread-safe, and the update/download client has no per-call timeout — LOW
- **Confidence:** High.
- **Location:** `OneMore/Helpers/HttpClientFactory.cs:34-56`. `client` is assigned before headers are added. Two threads (telemetry plus a command) can race, or one can send before the headers are set. `Warmup` and the other callers rely on the 100 s default.
- **Fix:** `Lazy<HttpClient>` with the headers set inside the factory. Pass explicit timeouts at call sites.
- **Effort:** S.

### R13. `RunPluginCommand.PrepareHierarchyCache` fallback path throws — LOW
- **Confidence:** High.
- **Location:** `RunPluginCommand.cs:231-238`
  ```csharp
  name ??= Guid.NewGuid().ToString("D");               // "D" format has no braces
  name = name.Substring(1, name.IndexOf('}') - 1)      // IndexOf == -1 -> Substring(1, -2) throws
  ```
- **Why it matters:** The "shouldn't happen" fallback (for example an empty `Notebooks` listing) throws `ArgumentOutOfRangeException` and aborts with the generic error. `PreparePageCache` has a similar assumption that the last `{...}` group is at least 32 characters (line 143).
- **Fix:** Strip braces and hyphens from the fallback GUID, and validate lengths.
- **Effort:** S.

---

## Worth investigating

### W1. Retry of `UpdatePageContent` may duplicate content — MEDIUM (confidence low)
`OneNote.InvokeWithRetry` (`OneNote.cs:400-489`) retries any `COMException` other than the four it recognizes (including RPC failures where OneNote may have applied the update before the connection dropped). Replacing the page is idempotent for elements that carry an `objectID`. New paragraphs without an `objectID` get new IDs on each attempt, so a replay could insert them twice. To confirm: simulate a COM timeout after `UpdatePageContent` succeeds, using a mock `IApplication` that throws after applying, and compare the page. Fix: after an RPC failure, re-read the page and compare before retrying.

### W2. Upgrade steps in `HashtagProvider` use deferred transactions while two processes can run them — LOW–MEDIUM (confidence medium)
`HashtagProvider.cs:134-380` upgrades 1→6 with `con.BeginTransaction()`. `DatabaseProvider` explains that deferred transactions can race (lines 423-426) and offers `BeginImmediate`. If the add-in and the tray both open the database after an update and both read version 3, the second `ALTER TABLE ... ADD COLUMN` fails with "duplicate column". That is swallowed (`return 0`), and `UpgradeCatalog()` finishes quietly. Later queries can fail on a half-upgraded schema. Check by starting the tray and the add-in against a v3 database at the same time. Fix: use `BeginImmediate` and re-read the version inside the transaction.

### W3. `IndexProvider.LoadFts5` uses `RuntimeInformation.ProcessArchitecture` — LOW (confidence medium)
`IndexProvider.cs:316`. The add-in runs in dllhost, so this is dllhost's bitness, which is the same as the add-in's DLL. It is correct for the DLL itself, but it is the pattern `CLAUDE.md` warns about for OneNote's architecture. Check that it is not reused for that purpose. The candidates tried first (`SQLite.Interop.dll` by name) load whichever interop is already mapped, which is the right one. The ARM64/ARM64EC case is unverified, as noted in the memory file.

### W4. Telemetry sends are fire-and-forget with no cancellation, and responses are not disposed — LOW (confidence medium)
`TelemetryClient.cs:166-183` calls `client.SendAsync` and drops the task. The `HttpResponseMessage` is never disposed, and faulted tasks are only observed via `ContinueWith`. If the API is slow, requests pile up with the 100 s default timeout, with no cap on how many are in flight. In the CLI path the process exits straight away (handled by awaiting in `CommandFactory`). Check by blocking `execute-api.*.amazonaws.com` and running a batch command.

### W5. Settings and DB files on redirected/network profiles — LOW (confidence low)
`PathHelper.GetAppDataPath()` is roaming. `SettingsProvider`, `SnippetMacros.CommitCounters` (`SnippetMacros.cs:311-317`, a read-modify-write with `File.WriteAllText`), `TableThemeProvider`, `ThemeProvider` and `RecentColors` all use plain overwrite writes. Two OneNote windows (or users on a terminal server) can corrupt or lose updates. Related to R2. A shared helper for atomic writes would cover them all.

### W6. `InvokeCliCommand` response writes after the client disconnects — LOW (confidence medium)
In `CommandService.InvokeCliCommand`, `WatchForClientCancellation` cancels the token when the client drops, but the final `WriteCliResponse` at line 423 is not wrapped in its own try. The outer `ListenOnce` try/catch in `ServiceLoop` logs the failure and applies the exponential back-off (up to 60 s, `ServiceLoop.cs:105-107`) to the listener. A CLI user pressing Ctrl+C repeatedly can therefore delay later Refresh links by up to a minute. Confirm by cancelling a long CLI run and timing the next call.

### W7. `RunPluginCommand` loads plugin output into a hierarchy update without validation — LOW (confidence low)
`SaveHierarchy` (`RunPluginCommand.cs:553-571`) passes the plugin's XML straight to `one.UpdateHierarchy(root)`. `SavePage` validates the schema first. A buggy hierarchy plugin can send OneNote a damaged section or notebook tree. Check what OneNote does with an invalid hierarchy; if it accepts partial changes, add `ValidateSchema` or a diff check.

### W8. Replay uses `Convert.ChangeType` on stored values — LOW
`CommandFactory.ReplayLastAction` (lines 343-361) handles unknown types by clearing the history only through the outer catch. A stored value that no longer parses (for example after a locale change from `1,5` to `1.5`) clears the whole MRU rather than skipping one entry. Minor.

---

## Top 5 by impact

1. **R1: `ProgressDialog` worker thread has no catch.** One fix removes a process-crash path across about 20 commands (S).
2. **R2: `SettingsProvider` non-atomic save and silent defaults.** Real user data loss; fix is a small helper (S–M).
3. **R8: Pipeline stages switch off permanently after five failures (plus no per-stage timeout).** A short OneNote outage silently disables identity, hashtags, index and favorites healing until restart (S).
4. **R6: Updater download and script bugs.** A bad or stale installer file can be run; failure messages are unhelpful (S–M).
5. **R7: `CommandService` blocks on `WaitForPipeDrain` and truncates requests.** One stalled client hangs every Refresh link and CLI call, and long arguments are cut without notice (S–M).

Close runners-up: R11 (ignored `Update` results), R3 (plugin processes are never killed), R9 (modal dialogs from the background pipeline).
