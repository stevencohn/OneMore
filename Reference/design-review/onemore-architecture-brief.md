# OneMore architecture brief

## What it is
OneMore is a OneNote desktop add-in with more than 160 commands. It is C# 9 on **.NET Framework 4.8**, uses WinForms with some WPF/WebView2, and is Windows-only. It works through Office COM interop. The license is MPL 2.0.

- **Build:** `.\build.ps1 -fast`, or `-fast -main` for the add-in only, or `-kit` for the installer only. It uses MSBuild and WiX. CI is `.github/workflows/build.yml`.
- **Run:** install, or register with `register-onemore.reg` / `setregistry.ps1`. OneNote then loads it as a COM add-in. It runs inside **dllhost.exe** (the COM surrogate), not ONENOTE.EXE.
- **Test:** MSTest in `OneMoreTests/`, about 105 `*Tests.cs` files, plus a `MockApplication.cs` and `SmokeTestProtocols/` for manual tests.

## Module map (`OneMore.sln`)
| Path | Responsibility |
|---|---|
| `OneMore/AddIn.cs`, `AddInCommands.cs`, `AddInHotkeys.cs` | COM entry point (`IDTExtensibility2`, `IRibbonExtensibility`), ribbon callbacks, hotkeys |
| `OneMore/OneNote.cs` (2.5k lines) | Wrapper over the OneNote COM API: hierarchy, page XML get/update, navigation |
| `OneMore/Commands/<Area>/` | About 19 feature areas (Clean, Edit, Images, Navigator, References, Search, Tables, Tagging, Workspaces…) |
| `OneMore/Models/` | Page XML object model (`Page`, `PageEditor`, `Paragraph`, `Table`, `Selection*`) |
| `OneMore/Services/` | Background pipeline: `Pipeline/`, `Identity/`, `TextIndex/` (FTS), `DatabaseBackup/` |
| `OneMore/Ribbon/` | `Ribbon.xml`/`RibbonTab*.xml`, galleries |
| `OneMore/UI/` | Themed `More*` WinForms controls, `ThemeManager`, `Scaling` (DPI) |
| `OneMore/Cli/` | CLI command definitions run inside the add-in |
| `OneMore/Helpers/` | Logger, registry, `DatabaseProvider`, `HttpClientFactory`, native interop |
| `OneMoreCli/` | `OneMoreCli.exe`: sends commands into the running add-in |
| `OneMoreProtocolHandler/` | Handles `onemore://` links and forwards them over a named pipe |
| `OneMoreTray/` | Tray app (`ScanningJob.cs`) |
| `OneMoreCalendar/` | Standalone calendar view of pages |
| `OneMoreSetup`, `OneMoreBundle` (WiX), `OneMoreSetupActions` | MSI, Burn bootstrapper, custom actions (bitness, ASR, WebView2, registry) |
| `Plugins/` | Sample PowerShell plugins |
| `packers/chocolatey` | Chocolatey packaging |
| `Reference/`, `specs/`, `docs/` | Schemas, design notes, wiki and telemetry docs |

## Entry points and data flows
1. **Startup:** `AddIn.OnStartupComplete` (`AddIn.cs:309`) starts a background task. It builds the `CommandFactory`, then runs isolated `StartupStep`s: theme, hotkeys, COM warm-up, host watcher, `CommandService`, `ReminderService`, `NavigationService`, the `PipelineService`, update check and telemetry.
2. **Commands:** the ribbon, hotkeys, command palette, Replay, pipe or CLI all go to `CommandFactory.Run` (`Commands/CommandFactory.cs:151`). The command runs on the threadpool. Its usual pattern is: get page XML from OneNote, change it through `Models/Page`, then `UpdatePageContent`. One catch-all handler logs and reports errors. Successful runs are saved to an MRU list for Replay.
3. **IPC:** `CommandService` (`Commands/CommandService.cs`) listens on a named pipe secured with an ACL. The pipe name comes from `HKCR\River.OneMoreAddIn\CLSID`. It accepts `onemore://` from the protocol handler and `onemorecli://` from the CLI. `Invoke` only runs types marked `[CommandService]`.
4. **Background pipeline:** `IdentityStage`, then `HealingStage` (favorites), then `HashtagStage` (which feeds `TextIndex.IndexConsumer`), then `BackupStage`.

## Storage and config
- **Settings:** XML in AppData via `PathHelper.GetAppDataPath()` (`Commands/Settings/SettingsProvider.cs:29`). There is a legacy `Impostazioni.xml` fallback.
- **SQLite:** `OneMore.db` under local AppData (`Helpers/DatabaseProvider.cs:77`) holds hashtags, page identity and the FTS5 trigram index. FTS5 loads through `LoadExtension`. Schemas are in `Services/*/*.sql`.
- **Registry:** COM registration, pipe name, bitness paths.
- **Secrets:** the telemetry API key is **hard-coded** in `Commands/Tools/Telemetry/TelemetryClient.cs:25` (the analyzer warning is suppressed on purpose). I found no other secret storage.

## External services
- **Telemetry:** AWS API Gateway (`TelemetryClient.cs:23`). It is opt-in and the user is asked on first run.
- **Updater:** the GitHub releases API. It downloads the installer, checks a SHA-256, then starts `msiexec` (`Commands/Tools/Updater/Updater.cs:27,304,340`).
- **Diagrams:** kroki.io / PlantUML (`Commands/Images/Diagrams/KrokiDiagramProvider.cs`).
- **Other web calls:** dictionaryapi.dev, qrserver.com, OneDrive/`d.docs.live.net` link resolution, WebView2 dialogs.
- **Office interop:** Word, Excel, PowerPoint and Outlook (import/export), WIA (scanning), System.Speech.

## Key dependencies
System.Data.SQLite, Newtonsoft.Json, HtmlAgilityPack, Markdig, WebView2, Svg, Vortice (Direct2D/D3D11/DXGI), InputSimulator, Chinese (a pinyin package).

## Unusual or surprising
- Bitness is load-bearing. The installer must match **OneNote's** bitness, including the ARM64EC heuristic (see `CLAUDE.md`, `OneMoreSetupActions/`).
- Because the add-in lives in dllhost, `ServiceLoop.StopAll()` is needed to clean up loops left by a previous host. A host watcher also kills work if OneNote dies.
- `CommandFactory.RunCore` has an extra `Task.Run` with the comment "not sure why this fixes it" (`CommandFactory.cs:158`). There is also temporary `PaletteTrace` diagnostics for issue #2926.
- Replay rebuilds commands from stored type names using `Type.GetType` and `Activator` (`CommandFactory.cs:320`).
- `OneMoreService/` is on disk but holds only `obj/`, and it is not in the solution.
- The project's `CLAUDE.md` topology leaves out `OneMoreCli` and `OneMoreBundle`.

## Highest-risk areas to review first
1. **IPC surface:** the pipe ACL, message parsing and how it routes to commands (`CommandService.cs`, `OneMoreProtocolHandler/`, `OneMoreCli/`).
2. **Updater:** the download, checksum source and integrity, and the elevated `msiexec` (`Updater.cs`).
3. **Plugins:** they run user scripts against page XML written to `%TEMP%`, with timeouts (`Commands/File/Plugins/RunPluginCommand.cs`).
4. **Page-editing core:** `OneNote.cs`, `Models/PageEditor.cs`, `ApplyStylesCommand.cs`. Corrupt XML here means lost user content.
5. **Concurrency and lifecycle:** startup ordering, `ServiceLoop`, COM objects across threads, shutdown (`AddIn.cs:478–566`).
6. **SQLite pipeline:** `Services/Pipeline`, `Identity`, `TextIndex`, `HashtagProvider.cs` (1.4k lines). Watch locking, schema migration and FTS5 extension loading, especially on ARM64.
7. **Large UI files:** `NavigatorWindow.cs` (1.9k lines), `RepairUrlsCommand.cs`, `EmbedCommand.cs`, plus DPI/theming.
8. **Installer custom actions:** `OneMoreSetupActions/`.

## What I couldn't determine
- Test coverage and whether CI runs the tests.
- How the ACL in `CreateSecuredPipe` is actually set up.
- Where the updater's checksum comes from.
- Whether `OneMoreTray` and `OneMoreCalendar` share the SQLite database or settings with the add-in.
- What `OneMoreService/` was meant to be.
- How the reminder and tray scanning jobs work.