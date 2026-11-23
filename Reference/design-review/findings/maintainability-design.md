# OneMore — Maintainability & Design Review

Lens: how easily a new contributor can change this code safely, and where the structure makes change risky or slow.
Scope read: `Commands/CommandFactory.cs`, `Command.cs`, `CommandProvider.cs`, `CommandService.cs`, `AddInCommands.cs`, `OneNote.cs`, `ApplicationFactory.cs`, `Models/PageEditor.cs`, `Commands/Settings/SettingsProvider.cs`, `Commands/Tagging/HashtagProvider.cs`, `Commands/Styles/ApplyStylesCommand.cs` and `PageColorCommand.cs`, `Helpers/Hotkeys/HotkeyManager.cs`, `Cli/CliNotebookResolver.cs`, plus the test project layout. I read the code listed here. I did not read all 890 `.cs` files, so counts come from grep and are labelled as such.

Counts used below (grep over `OneMore/**/*.cs`): `new OneNote(` 382, `Logger.Current` 206, `new SettingsProvider()` 113, `catch (Exception` 436, `async void` 97 (most are WinForms event handlers), `Task.Run` 61, `Thread.Sleep`/`Task.Delay` 51.

The mocking seam for OneNote exists and works: `ApplicationFactory.RegisterApplication` plus `MockApplication`. Several command tests use it. That is a real strength. The findings below are mostly about what is not behind a seam.

---

## Confirmed issues

### 1. Command discovery, palette, MRU and Replay are tied together by method-name string conventions — High (confidence: high)

**Location**
- `Commands/CommandProvider.cs:59-63` — palette = every `AddIn` public method whose name ends in `Cmd`
- `Commands/CommandProvider.cs:68-75` — label = resource key `rib{Name}Button_Label`
- `Commands/CommandProvider.cs:196-208` — MRU records the caller by walking the stack
- `Commands/CommandProvider.cs:283` — palette-invoked MRU entries are faked with `Method.Name.Replace("Cmd", "Command")`
- `Commands/CommandFactory.cs:333` and `:390` — `Type.GetType` on stored or incoming type names

```csharp
// CommandProvider.cs:196
var trace = new System.Diagnostics.StackTrace();
var runner = trace.GetFrames()
    .Where(f => f.GetMethod().Name.EndsWith("Cmd"))
    ...
// if not found in stack trace then must be from an async thread so derive the name
if (runner == null)
{
    runner = command.GetType().Name.Replace("Command", "Cmd");
    if (typeof(AddIn).GetMethod(runner) == null) { logger.WriteLine($"SaveToMRU skipping {runner}, method not found"); return; }
```

**Why it matters**
- A command only gets Replay support if the `AddIn.XxxCmd` name, the `XxxCommand` class name and the `ribXxxButton_Label` resource key all line up. Nothing enforces that at compile time. A mismatch gives a silent log line ("method not found") and a broken feature, not an error.
- The stack-trace walk depends on JIT inlining and on whether an `await` moved the code to another thread. The fallback branch exists because the first branch fails after async hops.
- Renaming a `Command` class (a normal refactor) breaks stored MRU entries, since `type` is stored as `FullName`. `ReplayLastAction` handles the missing type by logging, but the user sees Replay do nothing.
- A subtle bug lives here already. `CommandProvider.cs:74`: `Resources.ResourceManager.GetString($"om{name}Button_Label", ...)` uses `name`, which was just found to be empty. The intended variable is `nam`. The `om…` fallback can therefore never match.
- The code comment says it "heavily relies on naming convention".

**Suggested fix**
- Put the metadata on the command: `[Command(Id, LabelKey, Keys, Menu)]` on the `Command` class, or a static registry `Register<AddCommand>("AddFootnote", "ribAddFootnoteButton_Label", Keys…)`.
- Have `AddIn.XxxCmd` pass an explicit id to `factory.Run`, and key the MRU on that id.
- Add a unit test that every `AddIn.*Cmd` has a resolvable label and Command type. That test is a cheap first step before any redesign.
- Fix line 74 now.

**Effort:** M for the test plus the `nam` fix (S). L for the full metadata move, since it touches roughly 275 `AddInCommands` entries.

---

### 2. `CommandService.InvokeCliCommand` is a 200+ line transport/dispatch/business-logic blob — High (confidence: high)

**Location**: `Commands/CommandService.cs:220-426`, with `InvokeCliCommandForNotebook` (`:438`) and `InvokeCliBackupExport` (`:557-690`) in the same class. The file is 776 lines.

```csharp
// :302
if (typeof(ICliPageCommand).IsAssignableFrom(commandType))
{
    if (parameters.TryGet<string>("pageId", out var directPageId) && ...)
    {
        parameters.TryGet<bool>("backup", out var isBackupWithPageId);
        if (isBackupWithPageId) { StopWatcher(); await WriteCliResponse(pipe, "ERR:--pageId cannot be used with --backup"); return; }
```

**Why it matters**
- One method does five jobs:
  1. Parse the URI and query string.
  2. Validate parameters (including mutually exclusive flags like `pageId` and `backup`).
  3. Resolve notebook, section and page.
  4. Dispatch to one of three execution shapes, with heartbeats, cancellation and progress.
  5. Encode the wire protocol with magic strings (`"ERR:"`, `"OK"`, `"OUTPUT:"`, `"CANCELLED:"`, `"PROGRESS:"`).
- `StopWatcher(); await WriteCliResponse(...); return;` is hand-repeated at each exit. A new early return that forgets `StopWatcher()` leaves the watcher alive.
- Backup/export logic (`InvokeCliBackupExport`) is domain code that happens to run behind a pipe. It can only be exercised through a real `NamedPipeServerStream`, so it has no unit test (the only IPC-adjacent test is `ProtocolUriTests`).
- Adding a new CLI parameter combination means editing a method whose control flow is hard to hold in your head.

**Suggested fix**
- Split into: `CliRequest.Parse(data)` (pure, testable), `CliTargetResolver` (notebook/section/page to page IDs), `CliRunner` (runs a command per page given a token and an `IProgress<string>`), and a thin pipe layer that maps results to the wire strings.
- Give the wire protocol a small enum or record (`CliResponse.Err(msg)`, `.Ok()`, `.Output(text)`) instead of string concatenation at 15+ sites.
- Move the backup export to its own class next to `ArchiveCommand`.

**Effort:** L

---

### 3. Hierarchy lookup and retry logic is copy-pasted in at least 10 places — High (confidence: high)

**Location**: the by-name notebook comparison appears at
`OneNote.cs:1601` and `:1749`, `CommandService.cs:460` and `:580`, `Cli/CliNotebookResolver.cs:52`, `Cli/Commands/GetHierarchyCommand.cs:173`, `Commands/File/ArchiveCommand.cs:183`, `Commands/Page/ResetCursorCommand.cs:197`, `Commands/Search/SearchCommand.cs:296`, `Commands/Tagging/SearchHashtagsCommand.cs:90`, `Commands/Tools/SyncCommand.cs:63`.

The retry-until-OneNote-is-ready loop `for (attempt…) await Task.Delay(500 * attempt)` appears 15 times (grep).

```csharp
// OneNote.cs:1573 FindPagesByPath, and again OneNote.cs:1729 FindSectionIdByPath
var notebooks = await GetNotebooks();
for (int attempt = 1; attempt < 4 && (notebooks == null || !notebooks.HasElements); attempt++)
{ await Task.Delay(500 * attempt); notebooks = await GetNotebooks(); }
...
var notebookSections = await GetNotebook(notebookId, Scope.Sections);
for (...) { ... }
// then the same section-group walk, with subtly different trimming
```

**Why it matters**
- The two `OneNote.cs` methods already diverge. `FindPagesByPath` trims names (`part.Trim()`, `notebook.Trim()`). `FindSectionIdByPath` does not. `FindSectionIdByPath` also skips the repo's brace convention. A user typing `"Work /Sub"` resolves in one path and not the other.
- A bug fix such as "OneNote returns an empty hierarchy during startup" has to be applied in each copy. Someone will miss one.
- `FindPagesByPath` also embeds page-name matching rules (the `?` wildcard, the stripped-illegal-chars fallback). Those rules are CLI/URI-link policy sitting in the COM wrapper.

**Suggested fix**
- Add one `HierarchyResolver` (or private helpers on `OneNote`): `FindNotebook(name)`, `FindSectionNode(notebook, path)`, `WithHierarchyRetry(Func<Task<XElement>>)`. Make `FindSectionIdByPath` and `FindPagesByPath` thin callers.
- Move the page-name matching rules into a small pure class with unit tests.

**Effort:** M

---

### 4. `OneNote` is a 2,128-line catch-all that every command constructs ad hoc — High (confidence: high)

**Location**: `OneNote.cs` (61 public/private members). 382 `new OneNote(` sites.

```csharp
// OneNote.cs:157-176
public OneNote() { onenote = ApplicationFactory.CreateApplication(); logger = Logger.Current; }

public OneNote(out Page page, out XNamespace ns, PageDetail detail = PageDetail.Selection) : this()
{
    page = Task.Run(async () => { return await GetPage(detail); }).Result;
```

**Why it matters**
- It mixes at least five concerns: COM lifetime and retry (`InvokeWithRetry`, `ReplaceApplication`, `WithCurrentWindow`), hierarchy queries, page get/update, window and handle discovery (`GetWindows`, `GetTopLevelWindow`), and path resolution, export/import and search.
- Every `new OneNote()` creates a fresh COM `Application` via `ApplicationFactory`. There is no way to pass a fake `OneNote` to a command, only a fake COM `IApplication` underneath. The test seam is therefore one level too low for logic like path resolution: tests need a fully emulated OneNote XML hierarchy to cover it.
- The `out Page page` constructor does sync-over-async (`Task.Run(...).Result`) inside a constructor. If it is called on a UI thread with a sync context it can block, and the exception surfaces as an `AggregateException`.
- `DisposeAsyncCore` only does `Task.Yield()` (`:256`), so the 382 `await using` sites pay for an async dispose that does nothing.
- Public methods are not in alphabetical order, contrary to the project's own rule, which makes a 2k-line file harder to navigate.

**Suggested fix**
- Do not do a big-bang rewrite. First extract an `IOneNote` interface for the 15 or so calls that commands actually use. Alternatively extract `HierarchyResolver` (finding 3) and `WindowInfoReader` as separate classes that `OneNote` delegates to.
- Replace the `out Page` constructor with `static Task<(OneNote, Page)> OpenWithPage(...)`.
- Drop the async dispose or make it real.

**Effort:** L (extraction in steps: M each)

---

### 5. Settings: every `new SettingsProvider()` re-reads the file and saves without locking — Medium-High (confidence: high on the code; medium on how often it races)

**Location**: `Commands/Settings/SettingsProvider.cs:25-75` (constructor reads the XML file), `:169-173` (`Save`). 113 construction sites. `CommandProvider.SaveToMRU` is called after every command (`CommandFactory.cs:174`) from a threadpool thread.

```csharp
public void Save()
{
    PathHelper.EnsurePathExists(Path.GetDirectoryName(path));
    root.Save(path, SaveOptions.None);   // no lock, no temp-file-then-rename
}
```

**Why it matters**
- Each provider is a snapshot. The pattern "construct, modify one collection, `Save`" overwrites the whole file with the stale snapshot. Two overlapping commands (or a command and a background service such as the pipeline or navigator) can silently drop each other's changes. That is a lost-update bug that is hard to reproduce.
- A crash or `IOException` mid-write truncates `settings.xml`. The constructor then logs the read error and falls back to defaults, so a corrupted file looks like "all settings reset".
- The constructor also carries a one-off Italian-filename fix (`:34-45`) that runs on every read. The comment says it was "Fixed in 7.0.0 on May 23, 2026", so the fallback may now be removable.
- Any change to settings layout touches 113 call sites, so it is slow.

**Suggested fix**
- Use a process-wide lock around load/modify/save with write-to-temp-then-`File.Replace`.
- Longer term, make `SettingsProvider` a cached singleton with a `Update(collection, Action<…>)` API.
- Confirm the lost-update case with a test that runs two providers interleaved.

**Effort:** S for atomic write and lock, M for the update API.

---

### 6. `PaletteTrace` "TEMPORARY" diagnostics are woven through the command core — Medium (confidence: high)

**Location**: `Commands/CommandFactory.cs:162-166, 201-204, 215-219, 253` (8 call sites in the factory); `Helpers/Hotkeys/HotkeyManager.cs:543, 582-665` (the class is defined at the bottom of the hotkey file); `Commands/Tools/CommandPaletteCommand.cs:45`.

```csharp
// CommandFactory.cs:200
// TEMPORARY diagnostics for #2926
if (command is Commands.CommandPaletteCommand) { PaletteTrace.Arm(); }
```

**Why it matters**
- Core execution path code knows about one specific command (`CommandPaletteCommand`) and one specific bug number.
- `PaletteTrace` is static, global, lock-guarded state. A second hotkey press while a command runs calls `Begin` and clears `pending`, and `Mark` calls are attributed to the wrong chord.
- It is defined inside `HotkeyManager.cs`, so a reader looking for it will not find it by class name.
- If #2926 is closed, someone must remember to remove calls in three files. Until then the factory is harder to read: `RunCore` is mostly `Mark` lines.

**Suggested fix**
- Move `PaletteTrace` to its own file now.
- Put the calls behind `[Conditional("PALETTE_TRACE")]` so they compile out.
- Delete when #2926 closes (add the issue number to a removal TODO).

**Effort:** S

---

### 7. `Command` base class: setter-injected state, half-initialised instances, and an inconsistent CLI/UI contract — Medium (confidence: high)

**Location**: `Commands/Command.cs:29-36, 146-204, 223-248`; `Commands/Styles/PageColorCommand.cs:59-60`; `Commands/CommandFactory.cs:72-89` vs `:182-236`.

```csharp
// PageColorCommand.cs:59
styler = new ApplyStylesCommand();
styler.SetLogger(logger);              // owner, ribbon, factory, trash all null

// Command.cs:241  - ShowMessage builds a box and throws it away
var box = new UI.MoreMessageBox();
box.SetButtons(MessageBoxButtons.OK);
box.SetIcon(MessageBoxIcon.None);
UI.MoreMessageBox.Show(owner, message);
```

**Why it matters**
- Dependencies arrive via six chained setters after construction. A command created outside the factory is silently half-configured. `PageColorCommand` does that and works only because `ApplyTheme` happens not to touch `owner`. A later change to `ApplyStylesCommand.ApplyTheme` that shows a dialog would NRE or show an ownerless box.
- `ShowMessage` is dead code in effect. `box` is configured and never shown. The configured `OK`/`None` settings are ignored, and the call is just `ShowInfo` again. 5+ callers (for example `ExportCommand.cs:303`, `DeleteFormulaCommand.cs:63`) believe they get an icon-less OK box.
- `ShowError` respects `runningFromCli`, but `ShowInfo` and `ShowMessage` do not. A CLI-run command that reaches those pops a modal box in the user's OneNote session.
- The owner-window lookup (`new OneNote()` plus `OwnerWindow`) is duplicated in `Make<T>` (`:79`) and `Run` (`:216`), with the CLI skip only in one of them.
- `Command` has three "Skip…" flags plus `IsCancelled`, all `protected set` booleans read by the factory. Flags like this accumulate with each new special case.

**Suggested fix**
- Remove the dead `box` lines, or make `ShowMessage` do what its setup says.
- Make `ShowInfo`/`ShowMessage` no-op or log under CLI, like `ShowError`.
- Replace the setters with one `CommandContext` object passed to a single `Initialize(context)`. Give `PageColorCommand` a `styler` built with that context, or have `ApplyTheme` become a static or helper method that does not need a `Command` instance.
- Make `Make<T>` share one path with `Run` for owner discovery.

**Effort:** S (dead code and CLI guards), M (context object).

---

### 8. Page editing engine: mirrored forward/backward blocks and a hidden mode flag — Medium (confidence: high)

**Location**: `Models/PageEditor.cs:746-999` (`EditNode`, about 250 lines); `:67, 245-252, 759, 827` (`ReverseScanning`).

```csharp
public bool ReverseScanning { get; set; }          // :67
...
ReverseScanning = true;                            // :759, set inside EditNode
... prev-sibling scan ...
ReverseScanning = false;                           // :827
... next-sibling scan, a near mirror image ...
```

**Why it matters**
- `EditNode` has two near-identical halves, one scanning backward from the cursor and one forward. Each handles `XText` and `XElement span` separately, so four copies of "extract a word and run it through `edit`". Several branches use brace-less `if`/`break`/`continue`, which hides the structure.
- `ReverseScanning` is a public mutable flag that changes the behaviour of another public method (`GetSelectedText`). Any caller (or the `edit` delegate itself) that calls `GetSelectedText` during `EditNode` gets different results. After an exception inside `edit`, the flag is left `true` (no `try/finally`).
- The word-boundary rules (`WordChars`, `SplitAtLastWord`, `SplitAtFirstWord`, `ExtractLastWord`, `ExtractFirstWord`) are pure text logic that could be tested in isolation. As written they can only be tested by building page XML and a `Page`.
- This is the "page-editing core" the brief flags as highest risk: corrupt XML means lost user content. Duplicated logic makes it likelier that a fix lands in only one direction.

**Suggested fix**
- Extract a `WordSpan` helper that does "find the word touching the cursor in this node list, in direction d" once, parameterised by direction.
- Replace the public flag with a parameter, or reset it in `finally`.
- Add direct tests for the extracted pure pieces (punctuation, spans, `<br>`, empty nodes).

**Effort:** M

---

### 9. Overgrown classes with no internal seams: `HashtagProvider` (1,157 lines), `ApplyStylesCommand` (1,113 lines), `RepairUrlsCommand` (1,296), `EmbedCommand` (1,108), `NavigatorWindow` (1,505) — Medium (confidence: high on size and mixing, medium on how much each hurts)

**Location**
- `Commands/Tagging/HashtagProvider.cs:77-472` — six chained schema migrations (`Upgrade1to2` … `Upgrade5to6`, about 400 lines) in the same class as notebook, tag and page-info reads and writes (`:500-1365`)
- `Commands/Styles/ApplyStylesCommand.cs` — about 35 private methods covering theme application, title, selection, lists, hyperlinks, inline-code detection and CLI parameter parsing (`:61, :110`)

**Why it matters**
- A contributor changing "how inline code is detected" must read a command that also owns list numbering and hyperlink colour. That class is also reused by `PageColorCommand` (finding 7).
- `HashtagProvider`'s migrations cannot be tested or reasoned about without the provider. A change to tag queries sits next to DDL history.
- Tests exist for both (`HashtagProviderTests`, `ApplyStylesCommandTests`), which is good. They are probably coarse, since there is no seam below the whole class.

**Suggested fix**
- `HashtagProvider`: move the migrations into `HashtagSchema`/`HashtagMigrator`. Keep the same public API.
- `ApplyStylesCommand`: split inline-code handling (`MarkInlineCodeSpans`, `RestoreInlineCodeSpans`, `IsAllMonospaceParagraph`, `CollectMonospaceNormalOEs`, `ApplyToMonospaceNormalParagraphs`, about 300 lines) into a `CodeSpanStyler`. Move `ApplyTheme(Page)` to a `PageThemer` that is not a `Command`.

**Effort:** M each.

---

### 10. Process-wide static access defeats injection: `Logger.Current` (206), `Loggable` base, `AddIn.Self`, `AddIn.Telemetry` — Medium (confidence: high)

**Location**: `Helpers/Loggable.cs:9-12`, `OneNote.cs:160`, `SettingsProvider.cs:50`, `CommandFactory.cs:304` (`Logger.SetMirror`), `AddIn.cs:228-234`.

```csharp
protected Loggable() { logger = Logger.Current; }
```

**Why it matters**
- Commands receive an injected `ILogger` (`Command.SetLogger`), yet helpers they call grab `Logger.Current` themselves. During a CLI run `Logger.SetMirror(cliLog)` is a global (`CommandFactory.cs:228`), so two CLI commands running at once would interleave each other's mirrored output. `RunCore` clears it in `finally`, which also affects the other run.
- `AddIn.Telemetry` is a mutable public static bool read inside the factory (`:249, :277`). Unit-testing "does the factory report telemetry" needs global state set and reset.
- `Loggable` hides the dependency, so the class shows no sign that it writes to a global log.

**Suggested fix**
- Prefer constructor-injected `ILogger` for new classes.
- Keep `Logger.Current` as the default but allow an override in the constructor (`Loggable(ILogger logger = null)`).
- Make the CLI mirror per-call (async-local) rather than process-global.

**Effort:** S for the constructor overload, L if done everywhere (do it opportunistically).

---

## Worth investigating (I could not fully confirm from the code I read)

### W1. Threading and re-entrancy in `CommandFactory.RunCore` — Medium (confidence: low-medium)

`Commands/CommandFactory.cs:158-176` wraps the whole run in `await Task.Run(...)` with the comment "not sure why this fixes it! needs more research". This is the brief's known oddity. From a maintainability angle, it means nobody knows which thread a command is expected to run on: the add-in runs under the dllhost MTA, `OneNote` COM objects are created per command, and dialogs need an STA message loop. Commands such as the search ones need `RunModeless` workarounds (see the `EnterOnce` comment in `Command.cs:276-285`).
- To confirm: find the issue behind the batched File/Import symptom, and check whether `RunCore` could use the caller's context instead. Write one test that runs a command through `RunCore` and asserts the thread's apartment state.

### W2. `CommandService` accepts any `Commands.*` type by name for CLI — Medium (confidence: low)

`Commands/CommandService.cs:234-240` resolves `River.OneMoreAddIn.Commands.{commandName}` through `Type.GetType` for CLI calls, guarded only by `ProtocolUri.IsValidAction` (`:228`). The protocol-handler path checks `[CommandService]` (`CommandFactory.cs:398`), but this one does not appear to check for a CLI marker. The code that actually requires a command to opt in to CLI is in `ICliCommand`/`ICliPageCommand` handling lower down.
- To confirm: check `ProtocolUri.IsValidAction`'s rules and whether a non-CLI command can be run with a `parameters` object it does not expect. This is also a security concern, so treat it as a handoff to the security review rather than a maintainability finding.

### W3. `async void` outside event handlers — Low (confidence: medium)

97 `async void` methods (grep). Most are WinForms handlers, which is acceptable. `Commands/Edit/CopyAsMarkdownCommand.cs:25` is a local `async void WriteMarkdown(...)`, which hides exceptions from the caller.
- To confirm: read that method and its callers to see whether failures are swallowed or crash the process.

### W4. Tray, Calendar and CLI reuse of the main assembly through `InternalsVisibleTo` — Low-Medium (confidence: low)

`Properties/AssemblyInfo.cs:29-33` exposes internals to `LINQPadQuery`, `OneMoreCalendar`, `OneMoreCli`, `OneMoreTests` and `OneMoreTray`. So any rename of an `internal` type or member in the main project can break four other projects at build time or, worse, through reflection. `LINQPadQuery` is not in the solution.
- To confirm: list which internal types each of those projects uses. Decide whether a small shared `OneMore.Core` assembly would be cleaner, and drop `LINQPadQuery` if it is unused.

### W5. Dead code candidates — Low (confidence: low-medium)

- `OneNote.DisposeAsyncCore` (`OneNote.cs:256`) only yields.
- `#define xVerboseDispose` plus `#if VerboseDispose` blocks (`OneNote.cs:10, 219-249`) are permanently compiled out.
- `Command.ShowMessage` (see finding 7) does nothing beyond `ShowInfo`.
- `OneMoreService/` contains only `obj/` and is not in the solution (from the brief).
- To confirm: build with unused-member analyzers (IDE0051/IDE0052/CA1822 are partly enabled in `.editorconfig`) and run a reference search per symbol.

### W6. Test coverage gaps at the seams that matter — Medium (confidence: medium)

From the file listing, `OneMoreTests` has no test files named for `CommandFactory`, `CommandProvider`, `CommandService`, `SettingsProvider`, `PageEditor` or `OneNote.FindPagesByPath` (`OneNoteTests.cs` exists, but I did not read it). Many command tests exercise `PageEditor` indirectly, but nothing pins the MRU/Replay naming contract (finding 1) or the CLI protocol (finding 2).
- To confirm: grep the tests for those class names and read `OneNoteTests.cs`.

---

## Top 5 highest-impact items

1. **Finding 1 — Naming-convention coupling of palette/MRU/Replay.** Fix the `nam` bug at `CommandProvider.cs:74` now, add the "every `Cmd` has a label and Command type" test, then move to explicit command metadata. The fix is cheap, the failure is silent, and every one of the 160+ commands is affected.
2. **Finding 3 — Duplicated hierarchy lookup and retry logic (10+ copies).** Extract one `HierarchyResolver`. The copies have already diverged, and the extraction also unblocks finding 2.
3. **Finding 2 — `CommandService.InvokeCliCommand` doing everything.** Split parse, resolve, run and respond. It is the least testable large chunk of logic in the add-in, and the CLI is growing.
4. **Finding 5 — Settings lost-update and non-atomic write.** It is a small change (lock plus temp-and-replace), and the risk is user-visible data loss across 113 call sites.
5. **Finding 7 — `Command` base-class contract (dead `ShowMessage`, CLI-unsafe dialogs, half-initialised nested commands).** Cheap fixes for the dead box and the CLI guards. Together with finding 4's `IOneNote` extraction, it gives future contributors a safer way to write commands.

Next in line: finding 8 (the mirrored `EditNode` code, because it sits in the highest-risk area) and finding 6 (removing the `PaletteTrace` noise from the factory when #2926 is done).
