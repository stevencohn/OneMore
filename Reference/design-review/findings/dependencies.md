# Dependencies review: OneMore

Scope: every project in `OneMore.sln` (csproj/wixproj), `OneMore/App.config`, `build.ps1`, `.github/workflows/build.yml`, and the installer sources. No advisory lookup was possible (read-only, nothing restored, no network check), so every CVE statement below comes from memory and must be verified against OSV/NVD before acting.

## Inventory

| Project | Target | NuGet packages |
|---|---|---|
| OneMore | .NET Framework 4.8 (`OneMore.csproj:15`) | Chinese 0.5.0, HtmlAgilityPack 1.12.4, InputSimulator 1.0.4, Markdig.Signed 1.1.1, Microsoft.Bcl.AsyncInterfaces 9.0.9, Microsoft.Web.WebView2 1.0.3351.48, Newtonsoft.Json 13.0.4, Svg 3.4.7, System.Buffers 4.6.1, System.Data.SQLite.Core 1.0.119, System.Net.Http 4.3.4, System.Runtime.CompilerServices.Unsafe 6.1.2, System.Threading.Tasks.Extensions 4.5.4, Vortice.Direct2D1 / Direct3D11 / DXGI 1.9.143 (`OneMore.csproj:2493-2540`) |
| OneMoreCli | 4.8 | Microsoft.Bcl.AsyncInterfaces 9.0.9 |
| OneMoreCalendar | 4.8 | Newtonsoft.Json 13.0.4 |
| OneMoreTests | 4.8 | MSTest.TestAdapter / TestFramework 3.8.3 |
| OneMoreTray, OneMoreProtocolHandler, OneMoreSetupActions | 4.8 | none (framework refs only) |
| OneMoreSetup (WiX) | `WixToolset.Sdk/7.0.0` | UI.wixext `7.0.*`, Util.wixext `7.0.*` |
| OneMoreBundle (WiX) | `WixToolset.Sdk/7.0.0` | Bal.wixext `7.0.*` |

Non-NuGet references: Office interop assemblies 15.0 (`OneMore.csproj:111-131`), `stdole.dll` via `$(DevEnvDir)`, `Windows.winmd` at a hard-coded SDK path (`:175`). `System.Collections.Immutable` and `System.Reflection.Metadata` 9.0.0.0 are referenced with `SpecificVersion=False` and are not in the PackageReference list (`OneMore.csproj:139,151`; `OneMoreSetupActions.csproj:74,80`).

Vendored or bundled: `OneMoreSetup/Resources/seguisym.ttf`, `docs/pagefind/*` (generated JS), the in-house `OneMore/Colorizer/` engine. `git ls-files` shows no tracked `.dll/.exe/.msi/.nupkg` files. `packers/onemore.6.8.4.nupkg` is on disk but git-ignored.

Absent: `nuget.config`, `packages.lock.json`, `.github/dependabot.yml`.

## Confirmed issues

### 1. Floating WiX extension versions in release builds
- Severity: medium. Confidence: high. Effort: S.
- Location: `OneMoreSetup/OneMoreSetup.wixproj` (`<PackageReference Include="WixToolset.UI.wixext" Version="7.0.*" />`, same for Util.wixext); `OneMoreBundle/OneMoreBundle.wixproj` (`WixToolset.Bal.wixext Version="7.0.*"`). The SDK itself is pinned: `Sdk="WixToolset.Sdk/7.0.0"`.
- Why it matters: each restore picks the newest 7.0.x. Two builds of the same commit can produce different installers. If a 7.0.n extension ships a regression or is compromised, it goes into the signed MSI/Burn bundle automatically. The SDK/extension pair can also drift apart and break the release build on the day a patch is published. With no `nuget.config` source mapping or lock file, nothing detects the change.
- Fix: pin exact versions matching the SDK (`Version="7.0.0"`), set `RestorePackagesWithLockFile`, commit the lock files, and add a `nuget.config` with `packageSourceMapping` to nuget.org only.

### 2. No dependency or advisory monitoring, no lock files
- Severity: medium. Confidence: high. Effort: S.
- Location: repo-wide. `.github/workflows/` holds build, cleanup, delete-jobs, pages, pr-issue-*, winget-releaser only. No Dependabot config, no vulnerability scan step in `build.yml`.
- Why it matters: the add-in parses untrusted-ish input (pasted HTML via HtmlAgilityPack, markdown via Markdig, SVG and diagram responses via Svg, JSON from the updater/telemetry/dictionary APIs) and embeds SQLite. A new CVE in any of these is only noticed when the author happens to bump the version. Transitive packages (System.Memory, System.Numerics.Vectors, which `App.config` rebinds) are not visible in the csproj at all.
- Fix: add `.github/dependabot.yml` for `nuget` and `github-actions`; add an OSV-Scanner or `nuget` audit step to `build.yml`.

### 3. Redundant `System.Net.Http` 4.3.4 package on net48
- Severity: low. Confidence: high. Effort: S.
- Location: `OneMore.csproj:2517-2519` (the PackageReference is listed with the others; line numbers are for the `System.Net.Http` block).
- Why it matters: .NET Framework 4.8 ships `System.Net.Http` in the GAC. The 4.3.x package is the legacy NuGet-delivered copy; it drags in the old 4.3 `System.*` graph, and scanners flag that family (the 4.3.0 releases had CVE-2018-8292 and related issues; 4.3.4 is the patched build, verify). It also creates two possible identities for the same assembly inside dllhost.
- Fix: remove the PackageReference, keep the framework reference, rebuild, and smoke-test Updater, Telemetry and Kroki calls.

### 4. Binding redirects in `OneMore/App.config` have no effect, and several `System.*` generations coexist
- Severity: low. Confidence: medium. Effort: M.
- Location: `OneMore/App.config` (comment: "This section has no effect"; the add-in is COM-activated into dllhost.exe, which reads its own config). Redirects listed there: Bcl.AsyncInterfaces, Newtonsoft.Json, System.Buffers, System.Numerics.Vectors, System.Memory, System.Runtime.CompilerServices.Unsafe, System.Threading.Tasks.Extensions.
- Why it matters: package generations 4.3, 4.5, 4.6, 6.1 and 9.0 sit side by side. If a transitive reference asks for a different assembly version than the one in the install folder, it fails only at the first use of the affected type, only in dllhost, and only on machines where another add-in has not already loaded a different copy of that assembly. Confidence is medium because I did not trace binding at runtime.
- Fix: verify with a clean-VM install and fusion/assembly-load logging; add an `AppDomain.AssemblyResolve` handler if one does not already exist, or drop packages that are not needed.

### 5. Machine-specific and build-order-dependent reference paths
- Severity: low. Confidence: high. Effort: S.
- Location: `OneMore.csproj:134` (`$(DevEnvDir)PublicAssemblies\stdole.dll`), `:159` (`$(windir)\Microsoft.NET\Framework64\v4.0.30319\System.Runtime.WindowsRuntime.dll`), `:175` (`C:\Program Files (x86)\Windows Kits\10\UnionMetadata\10.0.26100.0\Windows.winmd`); `OneMoreTests.csproj:52` (`..\OneMore\bin\Debug\Microsoft.Office.Interop.OneNote.dll`).
- Why it matters: the build depends on one specific installed SDK. A CI runner image update (`windows-2025-vs2026`) that drops 10.0.26100.0 breaks release builds. `update-sdk.ps1` exists to rewrite the csproj for this reason. The test project only builds after a Debug build of OneMore.
- Fix: fail fast in `build.ps1` with a clear message when the path is missing, or move to a pinned reference-assembly package.

### 6. WebView2 bootstrapper downloaded and executed with no signature check
- Severity: medium. Confidence: high on the code, medium on exploitability. Effort: S.
- Location: `OneMoreSetupActions/Actions/EdgeWebViewAction.cs:31` (`DownloadUrl = "https://go.microsoft.com/fwlink/p/?LinkId=2124703"`), `:165-175` download, `:86` `Process.Start(bootstrap)`.
- Why it matters: the installer custom action downloads an executable to a random `%TEMP%` name and runs it without verifying its Authenticode signature or publisher. The URL is a moving fwlink, so a hash is not possible; a signature check is. Install normally runs elevated, so a TLS-intercepting proxy or a compromised redirect gets code execution as admin. The random temp name and `FileMode.CreateNew` reduce the local race but do not cover the download.
- Fix: before `Process.Start`, verify the file is signed by "Microsoft Corporation" (WinVerifyTrust, or `X509Certificate.CreateFromSignedFile` plus chain validation) and delete it on failure.

## Worth investigating

### A. System.Data.SQLite.Core 1.0.119: bundled SQLite age and extension loading
- Severity: medium. Confidence: low.
- Location: `OneMore.csproj:2520`; `Services/TextIndex/IndexProvider.cs:110,332` (`connection.LoadExtension(candidate, EntryPoint)` to load FTS5 from `SQLite.Interop.dll`).
- Concern: System.Data.SQLite releases rarely, so the bundled engine trails upstream SQLite, which has had security fixes since (for example CVE-2025-6965 in 3.50.2, from memory). The code also enables extension loading on a connection to a database under user-writable `%LOCALAPPDATA%`.
- Check: run `select sqlite_version()` and compare with sqlite.org/cves; confirm extension loading is switched off again after the FTS5 load and that `candidate` paths cannot resolve to a user-writable location.

### B. InputSimulator 1.0.4 is unmaintained
- Severity: low. Confidence: high that it is abandoned, low on risk.
- Location: `OneMore.csproj:2499`; used in `Helpers/ClipboardProvider.cs:79,171` and `Commands/Edit/ConvertLineOnEnterCommand.cs:340` only.
- Concern: last release around 2013; a thin `SendInput` wrapper. ARM64 behaviour is unverified. Replacing it with a small P/Invoke in `Helpers/` removes a dependency.

### C. Vortice 1.9.143 used by a single feature
- Severity: low. Confidence: medium.
- Location: `OneMore.csproj:2532-2540`; the only user is `Commands/Snippets/Emoji/ColorGlyphRenderer.cs`.
- Concern: three native-interop packages in every install for one renderer; the 1.x line is old compared with current Vortice releases. Check ARM64 behaviour and whether WPF/GDI+ could do the same job.

### D. Chinese 0.5.0 (pinyin)
- Severity: low. Confidence: medium.
- Location: `OneMore.csproj:2493`; used in `Commands/Edit/PronunciateCommand.cs`.
- Concern: pre-1.0, single-maintainer package carrying dictionary data for one command. Verify license and maintenance status.

### E. Parsers of external data: HtmlAgilityPack 1.12.4, Markdig.Signed 1.1.1, Svg 3.4.7, Newtonsoft.Json 13.0.4
- Severity: low. Confidence: low.
- Newtonsoft 13.0.4 is past the CVE-2024-21907 fix (13.0.1). Svg has had XML-entity and DoS reports in earlier versions. Check each against OSV, and confirm XML/SVG from network responses (`Commands/Images/Diagrams/KrokiDiagramProvider.cs`) is parsed with DTD processing prohibited.

### F. Bundled `seguisym.ttf`: licensing
- Severity: low to medium. Confidence: low.
- Location: `OneMoreSetup/Resources/seguisym.ttf` (tracked).
- Concern: Segoe UI Symbol is a Microsoft font, and redistribution is generally restricted by the Windows font license. Find out why it is bundled (and whether it is installed) and prefer the system copy. Verify before acting.

### G. Third-party notices and package licenses
- Severity: low. Confidence: low.
- Only `OneMoreSetup/Resources/license.rtf` (OneMore's own MPL text) was seen. Confirm whether third-party notices ship with the installer, and check the licenses of InputSimulator, Chinese, Svg (MS-PL) and Vortice against the notice obligations.

### H. CI supply chain
- Severity: low. Confidence: high on facts.
- `build.yml` pins every action by SHA (good) and signs via SignPath. `seanmiddleditch/gha-setup-vsdevenv` is a small third-party action pinned to a commit labelled `# master`. I did not review `winget-releaser.yml` or `pages.yml` for SHA pinning.

### I. Housekeeping
- `OneMoreService/` (only `obj/`), `OneMore/y.csproj.user`, and `SettingsDialog.Designer.cs` at the repo root look stray. They are not dependencies but they add noise to scans.

## Top 5 highest-impact items

1. Verify the WebView2 bootstrapper's Authenticode signature before the installer runs it elevated (#6).
2. Add Dependabot and a vulnerability scan to CI, and commit lock files (#2).
3. Pin the `7.0.*` WiX extensions to exact versions (#1).
4. Check the bundled SQLite version against advisories and confirm extension loading is disabled after the FTS5 load (A).
5. Validate assembly binding in dllhost on a clean install, and remove the redundant `System.Net.Http` package (#4, #3).
