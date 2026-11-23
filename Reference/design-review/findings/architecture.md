# OneMore — Architecture Review

Lens: structure and boundaries across the solution (add-in, tray, CLI, protocol handler,
calendar, setup, plugins). Line-level defects are out of scope unless they show a structural
problem. Every finding below cites code that was read; nothing is inferred from file names.

Date: 2026-11-23 · Branch: `main` @ `39847cd1`

---

## 0. Structural picture (as built)

```
                     +-------------------------------------------+
                     |  River.OneMoreAddIn.dll  (OneMore.csproj)  |
                     |  COM entry + 213 commands + UI + models +  |
                     |  SQLite services + pipeline + CLI commands |
                     +-------------------------------------------+
                        ^ ProjectReference + InternalsVisibleTo ^
        +---------------+---------------+-----------------------+
        |               |               |                       |
   OneMoreTray     OneMoreCli     OneMoreCalendar          OneMoreTests
 (runs Pipeline   (runs commands  (uses UI controls,
  in-process)      in-process or   ThemeManager, Logger,
                   via pipe)       models)

   OneMoreProtocolHandler  -- no reference; speaks to the add-in only over the pipe
   OneMoreSetupActions     -- no reference; installer side
```

Shared mutable state across processes: `%APPDATA%\OneMore\Settings.xml`, `%LOCALAPPDATA%\…\OneMore.db`
(SQLite/WAL), the hashtag schedule (in the DB), and one machine-wide named pipe whose name is the
add-in's CLSID.

Answers to some "couldn't determine" items in the brief:
- **CI does run the tests:** `.github/workflows/build.yml:267` runs `.\build.ps1 -Test` (vstest + trx upload).
- **Tray and Calendar share settings and the DB with the add-in.** The tray runs the add-in's own
  `PipelineService` + `HashtagStage` in its process (`OneMoreTray/ScanningJob.cs:199-213`) and reads
  `Settings.xml` through the add-in's `SettingsProvider` (`ScanningJob.cs:64,168`). The Calendar reads
  the add-in's `Settings.xml` directly by a hard-coded filename (`OneMoreCalendar/SettingsProvider.cs`,
  `AddInSettingsFilename = "Settings.xml"`).
- **The pipe ACL** grants FullControl to the current user SID only (`Commands/CommandService.cs:160-178`).
- **`OneMoreService/`** has only `obj/` output and is not in `OneMore.sln`.

---

## 1. Confirmed issues

### A1. Two background-lifecycle mechanisms; the pipeline is not stopped by `ServiceLoop.StopAll()` — **High**
**Confidence:** high

**Location:** `OneMore/Services/Pipeline/PipelineService.cs:110-120`, `OneMore/Helpers/ServiceLoop.cs:27-45`, `OneMore/AddIn.cs:331,498-517`

```csharp
// ServiceLoop.cs:28 — the stated reason StopAll exists
/// Cancels every running loop. ApplicationExit is not reliably raised when the add-in
/// is hosted in the dllhost surrogate, ...

// PipelineService.cs:114 — but the pipeline relies on exactly that event
serviceToken = new CancellationTokenSource();
Application.ApplicationExit += OnApplicationExit;
_ = Task.Run(() => Run(serviceToken.Token));
```

`CommandService`, `ReminderService` and `NavigationService` register with `ServiceLoop`. The
`PipelineService`, which is the heaviest background worker (identity, healing, hashtags, FTS index,
DB backup), does not. So `StopServices()` (on shutdown and from the host watcher) and the
`ServiceLoop.StopAll()` at startup (meant to clean up a previous add-in instance in a lingering
dllhost) both leave the pipeline running. After OneNote is killed and relaunched into the same
dllhost, two pipelines can run in one process against the same DB and the same COM server, and the
orphaned one keeps activating OneNote through COM. That is the "relaunching OneNote hidden" failure
the host watcher exists to prevent (`AddIn.cs:520-522`).

**Why it matters:** each new background feature has to pick a lifecycle model, and the one most
likely to be copied (the pipeline) is the one that does not work in this hosting model. Shutdown
correctness cannot be checked in one place.

**Suggested fix:** use one registry of stoppable services. Either run the pipeline's loop through
`ServiceLoop.Start(...)`, or add `ServiceLoop.Register(CancellationTokenSource)` and have
`PipelineService.Startup` register its token. Longer term, make `AddIn` own an `IHostedService`-style list
(`Start`/`StopAsync`) that it builds at startup and tears down in `StopServices`, rather than
fire-and-forget `new X().Startup()` calls whose instances nobody holds (`AddIn.cs:359,362,380-393`).

**Effort:** S (register the token) / M (hosted-service list)

---

### A2. Satellite apps depend on the whole add-in assembly's internals — **High**
**Confidence:** high

**Location:** `OneMore/Properties/AssemblyInfo.cs` (`InternalsVisibleTo` for `OneMoreCalendar`, `OneMoreCli`,
`OneMoreTray`, `OneMoreTests`, `LINQPadQuery`); `ProjectReference ..\OneMore\OneMore.csproj` in
`OneMoreCli.csproj`, `OneMoreTray.csproj`, `OneMoreCalendar.csproj`

```csharp
[assembly: InternalsVisibleTo("OneMoreCalendar")]
[assembly: InternalsVisibleTo("OneMoreCli")]
[assembly: InternalsVisibleTo("OneMoreTray")]
```
```csharp
// OneMoreTray/ScanningJob.cs:7-12 — tray pulls in commands, pipeline, settings, UI
using River.OneMoreAddIn.Commands;
using River.OneMoreAddIn.Identity;
using River.OneMoreAddIn.Pipeline;
using River.OneMoreAddIn.Settings;
using River.OneMoreAddIn.UI;
```

There is no shared "core" library. The COM add-in DLL, with 213 command classes, ribbon, WinForms/WPF
UI, Office interop, Vortice and WebView2, is the library every other executable links. Nearly all types
are `internal`, so the real contract between projects is "anything in the assembly". Signs of strain
are already in the code:
- `OneMoreCli/CommandDiscovery.cs:35-45` has to catch `ReflectionTypeLoadException` because "the OneMore
  assembly has COM/Office interop dependencies that may not resolve in a standalone CLI context".
- `OneMoreCalendar/Things/` re-implements parts of the add-in (`DateTimeExtensions.cs`, `GraphicsExtensions.cs`,
  `BitmapExtensions.cs`, `MoreButton.cs`, `MoreCheckBox.cs`, `ThemeProvider.cs`…) while also using the add-in's
  `River.OneMoreAddIn.UI.MoreButton` / `MoreLinkLabel` in its designers (`AboutDialog.Designer.cs:38-39`).
  So it carries two copies of some controls.

**Why it matters:** any refactor inside the add-in can silently break the tray, CLI or calendar.
Satellite startup cost and failure surface include the add-in's whole dependency graph. A future
move to .NET 10 (see `Reference/net10-migration.md`) has to move everything at once, because nothing
can be split off. Bitness rules also have to hold for every satellite that loads the add-in DLL.

**Suggested fix:** pull a `OneMore.Core` class library out of the add-in with a deliberate `public`
surface: `Logger`, `PathHelper`, `SettingsProvider`, `DatabaseProvider` and its providers, `Pipeline`, `Identity`,
the hashtag scheduler/stage, `OneNote` wrapper, `Models`. Put UI controls/`ThemeManager` in `OneMore.UI`.
The tray and calendar reference Core (+UI). The CLI references Core plus a small `OneMore.Cli.Contracts`
(the `ICliCommand` definitions). Remove `InternalsVisibleTo` for everything except tests. Delete the
duplicated `OneMoreCalendar/Things/*` helpers once Calendar references `OneMore.UI`.

**Effort:** L (can be staged: Core for tray first, then CLI, then Calendar)

---

### A3. Settings store: whole-file read-modify-write shared by several processes, with no locking or atomic write, and a failed read becomes defaults — **High**
**Confidence:** high

**Location:** `OneMore/Commands/Settings/SettingsProvider.cs:25-72,169-173`; writers include
`OneMore/Commands/Tagging/HashtagStage.cs:341-346` (background pipeline, and the tray process)

```csharp
// SettingsProvider.cs:40-67
if (File.Exists(path))
{
    try { root = XElement.Load(path); }
    catch (Exception exc) { Logger.Current.WriteLine($"error reading {path}", exc); }
}
...
if (root == null)
{
    // file not found so initialize with defaults
    root = new XElement("settings", new XElement("images", new XAttribute("width", "500")));
}
...
public void Save()
{
    PathHelper.EnsurePathExists(Path.GetDirectoryName(path));
    root.Save(path, SaveOptions.None);      // truncate + rewrite in place
}
```

There are 116 `new SettingsProvider()` call sites in 77 files. Each one parses the whole file, and each
`Save()` rewrites all of it. Readers and writers include the add-in UI threads, pipeline stages that
re-read on every cycle (`HashtagStage.cs:50,75` and `IsEnabled`), the tray (`ScanningJob.cs:64,168`), the CLI
(in-process commands) and the Calendar (`OneMoreCalendar/SettingsProvider.cs` reads `Settings.xml` directly).
So:
1. **Lost updates:** two providers built at different times each save their own snapshot. The last
   writer wins for *all* collections, not only the one it changed.
2. **Reset to defaults:** a reader that catches the file mid-write (`XElement.Save` truncates first)
   gets a parse error and falls back to defaults. If that instance then calls `SetCollection` + `Save()`,
   it writes a near-empty file over every user setting. `HashtagStage.PrepareRebuild` is such a
   read-modify-save on a background thread, in the tray process.
3. Migrations run in the constructor on every instantiation (`ContextMenuSheet.UpgradeSettings(this)`,
   `SettingsProvider.cs:70`; "TODO: Temporary … Released with 5.1.0"), and there is no settings schema version.
4. Collection keys are a mix of `nameof(SomeSheet)` (57 sites) and string literals (46 sites, e.g.
   `"HashtagSheet"`), so the persisted schema is tied to UI class names.

**Why it matters:** settings are the configuration backbone for 160+ commands and every satellite.
Silent loss of all settings is a top user-visible failure, and the risk grows with every new
background writer.

**Suggested fix:** keep the API and change the engine underneath it.
- Write atomically: save to `Settings.xml.tmp`, then `File.Replace(tmp, path, backup)`.
- On a parse failure, **do not** fall back to an in-memory default that can be saved. Mark the
  provider read-only (or load `Settings.xml.bak`), and refuse `Save()`.
- Serialize writers across processes with a named mutex (`Local\OneMoreSettings`). Re-load inside the
  lock and apply only the changed collection (merge by collection), not the whole snapshot.
- Optionally cache the parsed document per process and invalidate it with a `FileSystemWatcher`/mtime, so
  pipeline `IsEnabled` checks do not re-parse XML every cycle.
- Add a `<settings version="n">` attribute and run migrations once, under the lock.

**Effort:** M

---

### A4. Machine-global, single-instance pipe named by a constant CLSID — **High**
**Confidence:** high on the naming and single instance; medium on multi-session impact (confirm on an
RDS/fast-user-switching box with two signed-in users running OneNote)

**Location:** `OneMore/Commands/CommandService.cs:37,44-59,160-178`; `OneMoreSetup/Registry.wxs:113-115`;
duplicated constants in `OneMoreCli/AddinBridge.cs:25` and `OneMoreProtocolHandler/Program.cs:17`

```xml
<!-- Registry.wxs:113 -->
Key="River.OneMoreAddIn\CLSID"
<RegistryValue Value="{88AB88AB-CDFB-4C68-9C3A-F10B75A5BC61}" Type="string" />
```
```csharp
// CommandService.cs:174 — one server instance
return new NamedPipeServerStream(pipe, PipeDirection.InOut, 1, ...);
```

The pipe name is the same for every user on the machine (the `\\.\pipe\` namespace is not per
session), and `maxNumberOfServerInstances` is 1. Consequences:
- On a multi-user machine (RDS, Citrix, fast user switching) the first user's add-in owns the name.
  The second user's add-in can't create it, and the second user's CLI and protocol handler connect to
  the first user's pipe and are refused by its ACL. `onemore://` links and the CLI silently fail for
  everyone but the first user.
- Inside one session, the listener handles a CLI request inline (`CommandService.cs:121-134` awaits
  `InvokeCliCommand`) before it creates the next server instance. During a long CLI batch such as a
  notebook export, every `onemore://` link click (Refresh links, TOC refresh, embed refresh) fails its 500 ms
  connect (`OneMoreProtocolHandler/Program.cs:80`) and is dropped.
- The registry key path is a magic string copied into three projects.

**Why it matters:** IPC is how links, the CLI and future companions talk to the add-in. A
single-instance, global name puts a hard ceiling on concurrency and on multi-user deployment (enterprise
VDI is a realistic OneNote audience).

**Suggested fix:** derive the name per session and user, e.g.
`$"OneMore-{WindowsIdentity.GetCurrent().User.Value}-{Process.GetCurrentProcess().SessionId}"`, computed by a
shared helper (in the proposed `OneMore.Core`, or a tiny linked source file for the protocol handler).
Allow several server instances: create the next `NamedPipeServerStream` before dispatching, or run an accept
loop that hands each connection to a worker. Keep the registry value only as a feature flag or version
marker, if at all.

**Effort:** M

---

### A5. Two dispatch paths into commands with different allow-rules — **Medium**
**Confidence:** high

**Location:** `OneMore/Commands/CommandFactory.cs:388-411` vs `OneMore/Commands/CommandService.cs:228-241,401-403`

```csharp
// onemore:// path — CommandFactory.Invoke
if (type.GetCustomAttribute(typeof(CommandServiceAttribute), false) == null)
{ logger.WriteLine($"factory failed to invoke {action}; not a protocol service command"); return; }

// onemorecli:// path — CommandService.InvokeCliCommand
var typeName = $"River.OneMoreAddIn.Commands.{commandName}";
var commandType = Type.GetType(typeName, false);
if (commandType == null) { ... Unknown command ... }
// no ICliCommand / attribute check; any Command subclass in the namespace is run
result = await cliFactory.Run(commandType, cts.Token, ..., parameters);
```

The protocol path checks an allowlist attribute. The CLI path resolves *any* type in
`River.OneMoreAddIn.Commands`, including UI-bound commands that expect positional args, show dialogs, or
run plugins, and runs it with a `CliParameterSet` as `args[0]`. The CLI client limits itself to
`ICliCommand` types (`OneMoreCli/CommandDiscovery.cs:53`), but the server does not enforce it. Both
paths rebuild a type from a string by hand. Replay does a third variant (`CommandFactory.cs:332-340`).

**Why it matters:** the set of commands reachable over IPC is whatever happens to live in a namespace.
It grows with every new command and nothing reviews it. (The ACL limits callers to the same user, so
this is a boundary and robustness issue more than a remote exploit. The security pass should judge
exposure.)

**Suggested fix:** one `CommandRegistry` built once at startup by reflection. It maps a name to a
descriptor `{Type, Surfaces: Ribbon|Protocol|Cli|Replay, ParameterDefinition}`. `Invoke`, `InvokeCliCommand`
and `ReplayLastAction` resolve only through it, and each surface checks its own flag. In the CLI path,
require `typeof(ICliCommand).IsAssignableFrom(type)` as an immediate one-line guard.

**Effort:** S (guard) / M (registry)

---

### A6. CLI batch orchestration is duplicated in two processes and has already drifted — **Medium**
**Confidence:** high

**Location:** `OneMore/Commands/CommandService.cs:302-404,438-680` vs `OneMoreCli/Program.cs:317-385,398-470,481-…`

Both contain the same notebook → section → page iteration, the backup-export walk, `--pageId`/`--backup`
validation and `CollectSectionIds`. They already differ:
- The add-in path retries empty hierarchy reads 3× (`CommandService.cs:446-450,470-474`). The CLI direct-COM
  path doesn't (`Program.cs:404-409`).
- The add-in de-duplicates page IDs (`.Distinct()`, `CommandService.cs:509`). The CLI path doesn't
  (`Program.cs:448-451`), so the same notebook can run a page command twice depending on whether OneNote
  was already open.
- Error reporting: the add-in returns `ERR:` strings, the CLI throws or writes warnings.

**Why it matters:** the CLI's behavior depends on which process runs it. Every new CLI feature
(filters, dry-run, new scopes) has to be built twice.

**Suggested fix:** move the orchestration into one class in the shared assembly, e.g.
`CliBatchRunner.RunAsync(commandType, parameters, ICliSink sink, CancellationToken)`. `ICliSink` has
`Progress/Output/Error`. Use a pipe-writing sink in `CommandService` and a console sink in `Program`.

**Effort:** M

---

### A7. No UI-thread model: modal UI is raised from thread-pool and ad-hoc STA threads, including from background services — **Medium**
**Confidence:** high on the pattern; medium on the user impact (z-order/ownership bugs, focus theft)

**Location:** `OneMore/Commands/CommandFactory.cs:164-176` (every command on the thread pool);
`OneMore/Services/DatabaseBackup/DatabaseBackup.cs:271-282,299-323,350-361`;
`OneMore/Commands/CommandService.cs:147-154`; STA-thread boilerplate repeated in `PasteRtfCommand.cs:79`,
`ExportDialog.cs:156`, `ImportEvernoteDialog.cs:166`, `OpenFolderDialog.cs:80`, `BackupSheet.cs:174`,
`FileImportSheet.cs:66`, `ProgressDialog.cs:144`, next to the existing `Helpers/SingleThreaded.cs`

```csharp
// DatabaseBackup.cs — a pipeline stage pops UI
// the background pipeline has no UI thread, so make one for the progress dialog
private static Task<BackupOutcome> RunOnUIThread(...) { var thread = new Thread(...); thread.SetApartmentState(ApartmentState.STA); ... }
...
MoreMessageBox.ShowWarning(owner, Resx.BackupSheet_Failed);   // owner may be null, on a pool thread

// CommandService.cs:147 — async lambda on a dedicated STA thread
var worker = new Thread(async () => await InvokeCommand(data)) { ... };
worker.SetApartmentState(ApartmentState.STA);
```

The second pattern is the one `PipelineService.cs:117-118` itself warns about: an async lambda on a
`Thread` is `async void`, so the thread ends at the first `await`, and the STA apartment covers only the
synchronous prefix. Commands run on MTA pool threads (`CommandFactory.cs:164`) and create WinForms dialogs
there, owned by a OneNote HWND from another process. A service layer (`Services/`) depends on `UI`
(`MoreMessageBox`, `ProgressDialog`).

**Why it matters:** threading rules are re-decided at each call site. Clipboard, WebView2, drag-drop and
some COM objects need STA. Background stages that show UI make headless hosts (tray, CLI) and tests
harder, and allow dialogs to pop up while the user types.

**Suggested fix:** add one dedicated UI dispatcher thread (STA, running `Application.Run` on a hidden
`ApplicationContext`; `HotkeyManager` already does this at `HotkeyManager.cs:85`) and expose
`UiDispatcher.InvokeAsync(Func<T>)`. Route all dialogs through it and delete the per-file STA thread
boilerplate in favor of it or `SingleThreaded`. Services report through an `INotifier` interface (UI
implementation in the add-in, log-only one in the tray/CLI) instead of calling `MoreMessageBox`.

**Effort:** M–L (incremental: add the dispatcher, then migrate call sites)

---

### A8. COM connection model: about 400 independent `new OneNote()` activations with blocking retry and null-on-failure — **Medium**
**Confidence:** high on the code; medium on the cost (measure COM activation counts per command with
`VerboseDispose`)

**Location:** `OneMore/Helpers/ApplicationFactory.cs:18-76`; `OneMore/OneNote.cs:157-176`; 400
`new OneNote(` call sites across the solution; `CommandFactory.cs:79,216` (a throwaway activation just
to read `OwnerWindow`)

```csharp
// ApplicationFactory.cs
catch (COMException exc) when (... busy/RPC ...)
{ retries++; ...; System.Threading.Thread.Sleep(ms); }
...
catch (Exception exc) { logger.WriteLine(...); }
return app;            // may be null; OneNote.cs has 1 null check for 47 uses of `onenote.`

// OneNote.cs:174 — sync-over-async in a constructor
page = Task.Run(async () => { return await GetPage(detail); }).Result;
```

Every command, service tick and helper opens its own OneNote COM proxy. The factory blocks the calling
thread on retry and, on failure, returns `null`. The failure then surfaces later as a
`NullReferenceException` deep in `OneNote` and is reported as a generic command error. The test seam is
a mutable static (`type`/`singleton`). The brief's `#2926` cold-start workaround (`AddIn.cs:346-353`) and the
"not sure why this fixes it" `Task.Run` (`CommandFactory.cs:158-164`) point the same way: connection
lifetime and thread affinity are not modelled.

**Why it matters:** COM busy/RPC failures are the most common environmental failure for an
OneNote add-in. With no single owner of the connection, retry policy, apartment and cleanup are spread
over hundreds of sites, and so is the decision to fail fast.

**Suggested fix:** have `CreateApplication` throw a typed `OneNoteUnavailableException` instead of
returning `null`, and let `CommandFactory`'s catch-all map it to a specific user message. Use
`await Task.Delay` in an async factory instead of `Thread.Sleep`. Over time, give each command
invocation a scoped `OneNote` session owned by `CommandFactory` (it already makes one for
`OwnerWindow`) and pass it in, instead of re-activating inside helpers. Replace the
`OneNote(out Page, …)` constructor with an async factory method.

**Effort:** S (typed exception) / L (scoped session)

---

### A9. Adding a command means editing four or five unconnected places — **Medium**
**Confidence:** high

**Location:** `OneMore/AddInCommands.cs` (1,379 lines, 276 `…Cmd(IRibbonControl)` forwarders);
`OneMore/Ribbon/*.xml` (460 `onAction=` bindings); `[Command(...)]` metadata on the forwarders;
`[CommandService]` on the command class; `ICliCommand` for CLI; resx labels; and the palette reflects
over `AddIn.Self` (`Commands/Tools/CommandPaletteCommand.cs:70`)

```csharp
[Command("ribAddFavoritePageButton_Label", Keys.Control | Keys.Shift | Keys.A, "ribFavoritesMenu")]
public async Task AddFavoritePageCmd(IRibbonControl control)
    => await factory.Run<AddFavoritePageCommand>();
```

Command identity (label key, default hotkey, menu, ribbon control id, palette visibility, protocol/CLI
exposure) is spread over `AddIn` partial methods, ribbon XML strings and attributes on two different types.
Nothing checks that they agree at build time. The add-in class is also the command catalog, so the
palette needs a static `AddIn.Self` back-reference.

**Why it matters:** with 160+ commands and growing, this costs effort on every feature and leads to
drift (missing screentips, the subject of the latest commit `39847cd1`, is this kind of problem).
It is also what keeps the registry in A5 from existing.

**Suggested fix:** put the metadata on the command classes:
`[OneMoreCommand(Id="AddFavoritePage", Label=..., Hotkey=..., Menu=..., Surfaces=Ribbon|Palette)]`.
Generate the `…Cmd` forwarders (a T4 or source-generator step, or one generic `onAction="RunCmd"` callback
that dispatches on `control.Tag`/`control.Id`), and add a unit test that checks the ribbon XML ids against
the registry. The palette and hotkeys read the same registry.

**Effort:** M (generic dispatcher + test) / L (full generation)

---

### A10. The host watcher depends on hotkey registration succeeding — **Low**
**Confidence:** high

**Location:** `OneMore/AddIn.cs:340-356,523-543`

```csharp
var pid = HotkeyManager.OneNoteProcessId;
if (pid == 0) { logger.WriteLine("Startup: host watcher not started, OneNote process unknown"); return; }
```

The protection against relaunching OneNote over and over after a crash only works if the earlier
`hotkeys` startup step filled in `HotkeyManager.OneNoteProcessId`. Because the steps are isolated
on purpose, a hotkey failure (a conflict, or an RDP edge case) silently turns off the host watcher too.

**Suggested fix:** find the OneNote PID on its own, e.g. from the owner HWND of the warm-up step via
`GetWindowThreadProcessId`, or the parent of dllhost's activator, and store it in a small `HostInfo`
that both hotkeys and the watcher read.

**Effort:** S

---

### A11. Layering inversions: Models and Services depend on Commands and UI — **Low**
**Confidence:** high

**Location:** `OneMore/Models/PageHeadings.cs:7` (`using River.OneMoreAddIn.Commands.Snippets.Toc;`);
`OneMore/Services/TextIndex/{IndexBuilder,IndexConsumer,IndexedHits,IndexProvider}.cs:7`
(`using River.OneMoreAddIn.Commands;`); `OneMore/Services/DatabaseBackup/DatabaseBackup.cs:8` (`using …UI;`);
`OneMore/Helpers/Hotkeys/KeyBindings.cs:52`, `Helpers/Office/PowerPoint.cs:28` (`new SettingsProvider()` in helpers)

The folders suggest layers (Helpers → Models → Services → Commands → UI), but the dependencies go both
ways. The pipeline's own stages live under `Commands/` (`Commands/Tagging/HashtagStage.cs`,
`Commands/Workspaces/HealingStage.cs`), next to `Services/*Stage.cs`.

**Why it matters:** this is what blocks extracting `OneMore.Core` (A2). Every cycle has to be cut first.

**Suggested fix:** move `HashtagStage`, `HashtagScheduler`, `HashtagProvider`, `HealingStage` and the TOC
heading model into `Services/` or `Models/`. Make Services depend on an `INotifier` (see A7). Add an
architecture test (reflection over namespaces in `OneMoreTests`) that fails when `Models`/`Services`/`Helpers`
reference `Commands` or `UI`.

**Effort:** S–M

---

### A12. Racy lazy singletons used from several threads at startup — **Low**
**Confidence:** medium (a race is possible; whether it shows depends on timing. Check by looking for
duplicate theme JSON loads or duplicate `HttpClient` headers in logs)

**Location:** `OneMore/UI/ThemeManager.cs:59` (`instance ??= new ThemeManager()`; 96 call sites);
`OneMore/Helpers/HttpClientFactory.cs` (`if (client == null) { … client = new HttpClient(handler); client.DefaultRequestHeaders.Add(...) }`,
and it also sets the process-wide `ServicePointManager.SecurityProtocol` inside the shared dllhost);
`OneMore/AddIn.cs:228,234` (`AddIn.Self`, `AddIn.Telemetry` mutable statics)

The startup task touches `ThemeManager.Instance` (`AddIn.cs:337`) while ribbon/hotkey commands may already
run on other threads, and the update check and telemetry warm-up may call `HttpClientFactory.Create()`
at the same time.

**Suggested fix:** use `Lazy<T>` (thread-safe by default) for both, and turn `AddIn.Telemetry` into a
read-only value set once.

**Effort:** S

---

## 2. Worth investigating

### W1. Pipe protocol is an unframed text stream with a one-read request — **Medium**
**Confidence:** medium. Check: send a request larger than one pipe read (long Unicode `--infile` paths, or
future parameters) and check whether `CommandService.ListenOnce` sees all of it.

`CommandService.cs:97-114` reads once into a 4,096-byte buffer, with no length prefix or terminator.
Responses mix newline-terminated `PROGRESS:`/`HEARTBEAT` lines with an unterminated final
`OK|ERR:|OUTPUT:|CANCELLED:` payload that runs until EOF (`OneMoreCli/AddinBridge.cs:86-128`). Cancellation is
"any inbound byte". Output that itself starts a line with `PROGRESS:` would be misparsed. There's no
protocol version, so an older CLI against a newer add-in (or the reverse) fails in unclear ways.
*Sketch:* length-prefixed JSON messages `{v:1, type:"request|progress|heartbeat|result|cancel", …}`.
The protocol handler keeps sending its single `onemore://` string, wrapped by a shim. Also check
whether the ACL should explicitly deny `NT AUTHORITY\NETWORK` (the security pass should decide).

### W2. Schema migration is per provider, lazy, and not coordinated across processes — **Medium**
**Confidence:** low–medium. Check: start the tray and the add-in on a fresh profile with an old DB and look
for `error building … schema` / `SQLITE_BUSY` in both logs.

`DatabaseProvider.RefreshDataSchema` (`Helpers/DatabaseProvider.cs:470-511`) runs DDL in a deferred
transaction on first use by each provider (hashtags, identity, FTS, favorites…). Each domain keeps its own
version table (`ReadSchemaVersion`/`UpsertSchemaVersion`). Write paths use `BEGIN IMMEDIATE` specifically
because the tray and the add-in race (`DatabaseProvider.cs:422-427`), but schema creation and `DropCatalog`
(which also `VACUUM`s, `:372-375`) don't. There's no single "open and migrate OneMore.db" step at process
start, so a migration can run in the tray while the add-in is querying.
*Sketch:* a `DatabaseMigrator.EnsureCurrent()` called once per process under the existing
`Global\OneMoreDbMigration` mutex (already used for the file move at `:93`), with ordered, numbered migrations.

### W3. `Environment.Exit(0)` in `OnDisconnection` — **Low**
**Confidence:** low. Check: is the OneMore AppID's surrogate ever shared (other add-ins with the same
`DllSurrogate`)? If so, this terminates someone else's code.

`AddIn.cs:607-608`: "this is a hack, modeless dialogs seem to keep OneNote open". It hides lifecycle bugs
(undisposed modeless forms, threads that aren't background) that A1/A7 would otherwise bring out, and it
skips finalizers and any pending telemetry flush.

### W4. Replay persists CLR type names — **Low**
**Confidence:** medium. `CommandFactory.cs:320-376` rebuilds commands and argument types from stored
`Type.GetType` strings, with `Convert.ChangeType` for values. Renames are handled by clearing the MRU,
but the persisted format is the class name, so it ties user data to internal refactors.
*Sketch:* persist the registry id from A9/A5, not the CLR type name.

### W5. The plugin contract is implicit and unversioned — **Low**
**Confidence:** medium. `RunPluginCommand.cs:131-171,306-334` defines the plugin ABI as "OneNote XML in
`%TEMP%\{guid}.xml` + argv + `PLUGIN_*` environment variables + read back the same file". Returned XML is
checked against the schema (`:408,453`), which is good. But there's no `PLUGIN_API_VERSION`, no manifest of
what the plugin may change (page vs hierarchy), and the variable set is defined only in this method.
Adding a variable or changing temp-file naming has no way to stay compatible. *Sketch:* add
`PLUGIN_API_VERSION=1` now, and document the contract next to `Plugins/` samples.

### W6. Doc and repo drift — **Low**
**Confidence:** high, but low impact. `OneMoreService/` (only `obj/`, not in the solution) and the root
`CLAUDE.md` topology table, which omits `OneMoreCli` and `OneMoreBundle`. Both mislead new contributors
and automated agents about the process model. Remove the folder, and update the table.

---

## 3. Top 5 highest-impact items

| # | Item | Why first | Effort |
|---|------|-----------|--------|
| 1 | **A3 — Make settings writes atomic, locked and merge-based; never save defaults after a failed read** | Prevents silent loss of all user settings; several processes already write | M |
| 2 | **A1 — Put `PipelineService` under the same stop mechanism as `ServiceLoop`** | Small change; closes an orphaned-pipeline / OneNote-relaunch path in the exact scenario the code was built to handle | S |
| 3 | **A4 + W1 — Per-session pipe name, several instances, framed versioned protocol** | Unblocks multi-user/VDI, stops link clicks being dropped during CLI batches, makes the CLI ↔ add-in protocol safe to change | M |
| 4 | **A5 + A9 — One command registry (metadata on command classes) used by ribbon, palette, protocol, CLI, Replay** | Removes the four-place command wiring, enforces IPC exposure in one spot, and allows a build-time consistency test | M |
| 5 | **A2 (+A11) — Extract `OneMore.Core` / `OneMore.UI`; drop `InternalsVisibleTo` for satellites; de-duplicate Calendar helpers and the CLI batch runner (A6)** | The main structural enabler: separate evolution, smaller satellites, and a feasible staged .NET 10 path | L |
