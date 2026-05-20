# OneMore — solution-wide context for Claude

A OneNote add-in (VSIX) with 160+ commands. Internal namespace
`River.OneMoreAddIn`. Licensed MPL 2.0. Homepage: https://onemoreaddin.com/

## Stack

- **.NET Framework 4.8** (not .NET / .NET Core / .NET 5+)
- **C# LangVersion 9.0** — nullable refs, records, pattern matching are fine
- **Windows-only** — uses Office COM interop, registry, WinForms
- Code analysis rules live in `.editorconfig` (many CA* warnings enabled,
  CA1031 disabled). No StyleCop, no `Directory.Build.props`.

## Building

Use `.\build.ps1 -fast -local` from the repo root in PowerShell — **not** `dotnet build`
(this is .NET Framework + MSBuild + WiX installer). Key flags:

| Flag | Purpose |
|------|---------|
| `-Architecture <x86\|x64\|ARM64\|All\|x>` | Default `x86`. `x` = x86+x64. |
| `-Fast` | Skip the installer kit; just build the .csproj projects. |
| `-Kit` | Reuse existing binaries; build the installer only. |
| `-Prep` | One-time `DisableOutOfProcBuild` for Visual Studio. |
| `-Local` | Skip restoring the .vdproj from git (advanced — usually leave off). |
| `-VLog` | Verbose MSBuild logging. |

CI: `.github/workflows/build.yml` runs `build.ps1` on `windows-latest`.

## Project topology (from `OneMore.sln`)

| Project | Kind | Role |
|---------|------|------|
| `OneMore` | VSIX (.csproj) | Main add-in: ribbon, commands, UI |
| `OneMoreSetup` | WiX (.wixproj) | Builds the installer (MSI + Burn bootstrapper) |
| `OneMoreSetupActions` | Console .exe (.csproj) | Custom actions runner (bitness check, etc.) — see `OneMoreSetupActions/CLAUDE.md` |
| `OneMoreCalendar` | .csproj | Calendar feature |
| `OneMoreTray` | .csproj | System-tray companion |
| `OneMoreProtocolHandler` | .csproj | `onemore://` URL handler |

## Bitness is load-bearing

Every project builds for **x86 / x64 / ARM64**. The installer enforces that
OneMore's bitness matches **OneNote's** bitness — not Windows's. Code referring
to `Environment.Is64BitProcess`, `RuntimeInformation.OSArchitecture`,
`WOW6432Node`, or PE-header inspection is part of this machinery and is not
incidental. See `OneMoreSetupActions/CLAUDE.md` for the enforcement rules.

### ARM64EC heuristic — apply everywhere architecture is detected

Office on ARM64 Windows ships as **ARM64EC**: binaries whose COFF `Machine`
field is `IMAGE_FILE_MACHINE_AMD64` even though they run natively on ARM64.
A plain PE-header read therefore misreports ARM64EC OneNote/OneMore as x64.

**The required pattern** (used in `CheckBitnessAction.cs` and `Updater.cs`):

```csharp
Machine.I386  => /* x86 */,
Machine.Arm64 => /* ARM64 */,
Machine.Amd64 when RuntimeInformation.OSArchitecture == Architecture.Arm64
              => /* ARM64  ← ARM64EC heuristic */,
_             => /* x64 */
```

Apply this whenever selecting an installer, matching a registry path, or
otherwise branching on the architecture of a binary running on this machine.

**Do NOT apply it in `SessionLogger.GetAssemblyArchitecture`** — that method
intentionally returns the literal PE value for diagnostics and telemetry.
ARM64EC reporting as `"x64"` there is useful population data.

**Do NOT use `RuntimeInformation.ProcessArchitecture`** as a shortcut for
OneNote's architecture. The add-in runs in **dllhost.exe** (COM surrogate),
not `ONENOTE.EXE`, so `ProcessArchitecture` reflects dllhost's bitness.
Read `ONENOTE.EXE`'s PE header directly when you need OneNote's architecture.

## Conventions and norms

- **Issues / PRs:** use the `gh` CLI. Default repo is `stevencohn/OneMore`
  (e.g. `gh issue view 2017 --repo stevencohn/OneMore --comments`).
- **Commits are GPG-signed.** See `.github/pull_request_template.md`.

## Where to read more (don't duplicate here)

### General local documentation
- `README.md` — user-facing overview
- `OneMore/Commands/Search/CLAUDE.md` — Search subsystem (virtual ListView,
  atom model, regex replace engine)
- `OneMoreSetup/CLAUDE.md` — WiX installer, Burn bootstrapper, bitness check
- `OneMoreSetupActions/CLAUDE.md` — MSI custom actions, bitness enforcement rules
- `docs/` — wiki rebuild + telemetry dashboard notes

### Architecture and design docs
- See pages under https://onemoreaddin.com/developers
- Specifically:
   - [TechNote - Interop](https://onemoreaddin.com/developers/TechNote%20-%20Interop.htm)
   - [TechNote - COM Surrogate](https://onemoreaddin.com/developers/TechNote%20-%20COM%20Surrogate.htm)
   - [TechNote - Command Framework](https://onemoreaddin.com/developers/Design%20-%20Command%20Framework.htm)
   - [TechNote - Styles](https://onemoreaddin.com/developers/TechNote%20-%20Styles.htm)