# Assessment: Migrating OneMore from .NET Framework 4.8 to .NET 10

## Context

This is a read-only feasibility assessment — no code changes are proposed. The goal is to answer: what would it take, what are the blockers, what is the risk, and how hard would this be?

The OneMore project is a ~615-file C# class library that acts as an Office COM add-in, hosting 160+ OneNote commands behind a WinForms UI. It lives in a six-project MSBuild solution with a custom PowerShell build system and a WiX installer. The question is whether the main `OneMore.csproj` project alone could be migrated to `net10.0-windows`.

---

## COM Add-in Activation — PROVEN WORKING

The originally-identified CRITICAL UNKNOWN is resolved. The repo https://github.com/stevencohn/OneNoteVanilla5 (by the same author) is a working .NET 5 OneNote COM add-in using exactly this mechanism, validated against a live OneNote install.

The pattern:
- `<EnableComHosting>true</EnableComHosting>` in the SDK-style csproj causes the build to emit `River.OneMoreAddIn.comhost.dll`
- The registry `InprocServer32` points to `comhost.dll` (not `mscoree.dll`) with `RuntimeVersion=v5.0` (or v10.0)
- `IDTExtensibility2` + `IRibbonExtensibility` load and execute correctly

**This is no longer a blocker.** The COM activation question is answered.

Additionally, the vanilla5 repo shows the correct approach for Office interop references on .NET 5+: use **`<COMReference>` with `WrapperTool=tlbimp`** rather than `<Reference>` to pre-built PIA DLLs. This generates interop wrappers from the COM type libraries at build time, avoiding any dependency on .NET Framework PIA assemblies at runtime. This resolves the `EmbedInteropTypes=false` concern for the OneNote PIA.

---

## Issue Inventory

### 1. Project File Format (Required, Moderate Effort)

The current `.csproj` uses the old MSBuild verbose format (`ToolsVersion="12.0"`) with 1,200+ lines explicitly listing every `.cs` file. .NET 10 requires the SDK-style format (`<Project Sdk="Microsoft.NET.Sdk.WindowsForms">`), which auto-discovers `.cs` files.

- Migration tooling (`dotnet-upgrade-assistant`, `try-convert`) can automate most of this.
- Multi-platform PropertyGroups (x86/x64/ARM64) need to be reconciled with the SDK-style approach (`RuntimeIdentifier` vs `PlatformTarget`).
- Binding redirects in `App.config` do not apply in .NET Core — that whole mechanism is gone (most of what's in the file are now built-in to .NET 10 anyway).

### 2. Office Interop PIAs (Low Risk — Solved by COMReference pattern)

The current csproj uses `<Reference>` to pre-built PIA DLLs with mixed `EmbedInteropTypes` settings, which would be problematic (.NET Framework PIA DLLs can't be loaded by a .NET 10 process).

The fix is to replace all Office interop references with `<COMReference>` entries (as demonstrated in OneNoteVanilla5), which invoke `tlbimp` at build time to generate fresh local interop wrappers from the COM type libraries. This produces .NET-compatible interop assemblies without any runtime dependency on PIA DLLs. The `extensibility.dll` reference from VS's `PublicAssemblies` is the one holdout — it needs evaluation (embed it or also convert to COMReference via the extensibility type library).

### 3. `System.Web` (Low Risk, Low Effort)

18 files use `System.Web` (which does not exist in .NET 10). Specific usages:
- `HttpUtility.UrlDecode/UrlEncode/HtmlEncode` → `System.Net.WebUtility` (built-in, drop-in replacement)
- `JavaScriptSerializer` (4 files: Updater.cs, PronunciateCommand.cs, PluginsProvider.cs, Plugin.cs) → Newtonsoft.Json (already a dependency) or `System.Text.Json`

This is purely mechanical refactoring with no architectural impact.

### 4. `System.Speech` (Medium Risk, Low Effort)

Referenced in csproj, used in `PronunciateCommand.cs`. `System.Speech.Synthesis.SpeechSynthesizer` does not exist in .NET 10. Options:
- Use the WinRT `Windows.Media.SpeechSynthesis` API (accessible via CsWinRT) — requires rework but equivalent functionality
- Use `Microsoft.CognitiveServices.Speech` SDK (cloud dependency)

### 5. `Windows.winmd` + `System.Runtime.WindowsRuntime` (Medium Risk, Moderate Effort)

The csproj references `Windows.winmd` directly from the Windows SDK path and `System.Runtime.WindowsRuntime.dll` directly from `Framework64`. These are the .NET Framework's way of projecting WinRT APIs.

In .NET 5+, WinRT access is done differently: via the `Microsoft.Windows.CsWinRT` NuGet package, with generated projections. Any code using Windows Runtime types (e.g., `Windows.Storage`, `Windows.UI`, clipboard APIs, etc.) needs to be identified and migrated to the CsWinRT projection API surface. The API shapes are very similar, but the namespace imports and some async patterns differ.

### 6. `System.Drawing` / `System.Drawing.Imaging` (Low Risk, No Code Changes)

115 files use `System.Drawing`. In .NET 6+, `System.Drawing.Common` is Windows-only and throws `PlatformNotSupportedException` on non-Windows. Since OneMore is inherently Windows-only, this is resolved by:
- Adding `<UseWindowsDesktopRuntime>true</UseWindowsDesktopRuntime>` to the csproj, or
- Adding the `System.Drawing.Common` NuGet package

All existing image manipulation code should work as-is.

### 7. `Aga.Controls.dll` (Medium Risk — External Dependency)

Referenced as `External\Aga.Controls.dll` (a bundled local binary). This is `Aga.Controls.Net` — a .NET Framework TreeViewAdv library. A .NET Framework DLL **cannot be loaded into a .NET 10 process**. Options:
- Find or build a .NET 5+-compatible fork (the project had several community forks)
- Replace the control entirely with a different tree view

### 8. `AppDomain.AssemblyResolve` (Not a Blocker)

`AddIn.cs` uses `AppDomain.CurrentDomain.AssemblyResolve` and `AppDomain.CurrentDomain.UnhandledException`. Both of these APIs **exist in .NET 5+** (`AppDomain` is present but restricted). This is not a breaking change.

### 9. `System.ServiceModel` (Low Risk, Unknown Scope)

Referenced in csproj. WCF client is available in .NET via the `System.ServiceModel.Http` NuGet (CoreWCF). Usage would need to be scoped to determine if server-side WCF is used.

### 10. WIA COM Reference (Low Priority)

The WIA (Windows Image Acquisition) scanner integration is a COM reference with GUID. COM interop with WIA works on .NET 5+ Windows, but the type library reference and generated interop would need to be regenerated or embedded. Low priority (scanner feature is peripheral).

### 11. `System.Configuration` (Low Risk, Low Effort)

Referenced in csproj. Available in .NET 5+ via the `System.Configuration.ConfigurationManager` NuGet package. Straightforward.

### 12. WinForms (Not a Blocker)

~144 WinForms dialogs and 150+ WinForms usages. WinForms is **fully supported** in .NET 5+ on Windows (`net10.0-windows`). The WinForms designer in VS 2022 works with .NET SDK projects. This is not a blocker.

### 13. NuGet Packages

Most NuGet packages are compatible with .NET 5+:

| Package | Status |
|---|---|
| HtmlAgilityPack 1.12.4 | .NET Standard — compatible |
| Markdig.Signed 1.1.1 | .NET Standard — compatible |
| Newtonsoft.Json 13.0.4 | .NET Standard — compatible |
| Microsoft.Web.WebView2 1.0.x | Supports .NET — compatible |
| System.Data.SQLite.Core 1.0.119 | Has .NET 5+ builds — compatible |
| `Chinese` 0.5.0 | **Unknown** — obscure package, may be .NET Framework only |
| InputSimulator 1.0.4 | Stale package, likely needs replacement or update |
| Microsoft.Bcl.AsyncInterfaces, System.Buffers, System.Net.Http, etc. | Not needed — built into .NET 10 |

### 14. Build System (Moderate Effort)

`build.ps1` invokes MSBuild directly with multi-platform configurations. Migrating to SDK-style csproj changes how platform targeting works (`-p:RuntimeIdentifier=win-x64` instead of `PlatformTarget=x64`). The WiX installer projects (separate) are not in scope, but the build script orchestration would need updating regardless.

---

## Blast Radius Summary

| Area | Severity | Files/Scope | Notes |
|---|---|---|---|
| COM add-in activation via comhost.dll | ~~CRITICAL UNKNOWN~~ **RESOLVED** | AddIn.cs + registry/installer | Proven working in OneNoteVanilla5 |
| Office PIA references | ~~HIGH~~ **LOW** | ~5 PIA references | Switch to `<COMReference>` + tlbimp pattern; proven in OneNoteVanilla5 |
| Project file format migration | HIGH | 1 csproj (1,967 lines) | Toolable but requires validation |
| System.Web removal | LOW | 18 files | Purely mechanical |
| System.Speech | MEDIUM | PronunciateCommand.cs | API replacement needed |
| Windows.winmd / WinRT | MEDIUM | Scope unknown until grepped | CsWinRT migration needed |
| System.Drawing.Common | LOW | 115 files | Just add NuGet / flag; no code changes |
| Aga.Controls.dll | MEDIUM | TreeViewAdv usages (HierarchyView) | Needs .NET 5+ compatible version |
| Chinese / InputSimulator NuGets | MEDIUM UNKNOWN | Unknown usage scope | Must verify .NET 10 compatibility |
| System.Configuration | LOW | App.config + usages | NuGet drop-in |
| WinForms (UI layer) | NONE | 144+ forms | Fully supported |
| P/Invoke (Native.cs) | NONE | ~5 files | Fully supported on Windows |
| Registry access | NONE | ~10 files | Fully supported on Windows |
| AppDomain APIs | NONE | AddIn.cs | Still available in .NET 5+ |
| async/await patterns | NONE | 227 files | Fully compatible |

---

## Overall Risk and Effort Assessment

### Is it possible?
**Yes.** The only previously-unresolvable unknown (COM add-in activation on .NET 5+) is now proven working via OneNoteVanilla5. Every remaining obstacle has a known migration path.

### How hard?
**Hard but tractable.** The codebase is large (~615 C# files, 150+ WinForms forms, a 1,967-line csproj), and there's no single tool that handles the whole migration. But none of the remaining issues are architectural dead ends.

**Rough effort estimate for a single developer:**
- Project file migration (old MSBuild → SDK-style) + NuGet cleanup: 3–5 days
- Office interop references → COMReference + tlbimp: 1–2 days
- System.Web removal (18 files, mechanical): 1–2 days
- System.Speech replacement: 1–2 days
- Windows.winmd / WinRT → CsWinRT migration: 2–4 days
- Aga.Controls.dll replacement: 2–4 days
- Installer updates (registry → comhost.dll): 1–2 days
- Full regression testing across x86/x64/ARM64: 5–10 days
- **Total: ~16–31 days of focused work — no binary blockers**

### What's the upside?
- Long-term support: .NET Framework 4.8 is in maintenance mode (security patches only)
- Performance improvements (JIT, GC, LINQ) in .NET 10
- Access to modern C# language features beyond 9.0
- Better tooling integration

### What's the downside?
- .NET Framework 4.8 is supported through at least January 2029 (Windows 11 lifetime), probably longer
- There is no immediate practical gain for users
- The effort is equivalent to a significant feature release cycle with no user-visible changes

### Recommendation
**Migration is viable, but expensive.** With COM activation proven and all other blockers having known solutions, this is now a question of engineering investment, not technical feasibility. The main cost drivers are the QA burden (160+ commands, 3 architectures) and the Aga.Controls replacement.

**OneNoteVanilla5 already serves as the spike** — no additional validation needed before committing.

---

## What CAN Be Migrated Now (Lower Risk)

The companion projects (`OneMoreCli`, `OneMoreTray`, `OneMoreProtocolHandler`) do not have the COM add-in constraint and could be migrated to .NET 10 independently and incrementally with much lower risk. Those would be good candidates if modernization is a goal.
