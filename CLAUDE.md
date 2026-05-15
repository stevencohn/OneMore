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
(this is .NET Framework + MSBuild + a legacy installer). Key flags:

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
| `OneMoreSetup` | **Legacy .vdproj** | Builds the MSI installer |
| `OneMoreSetupActions` | Console .exe (.csproj) | MSI custom actions runner — see `OneMoreSetupActions/CLAUDE.md` |
| `OneMoreCalendar` | .csproj | Calendar feature |
| `OneMoreTray` | .csproj | System-tray companion |
| `OneMoreProtocolHandler` | .csproj | `onemore://` URL handler |

## Bitness is load-bearing

Every project builds for **x86 / x64 / ARM64**. The installer enforces that
OneMore's bitness matches **OneNote's** bitness — not Windows's. Code referring
to `Environment.Is64BitProcess`, `RuntimeInformation.OSArchitecture`,
`WOW6432Node`, or PE-header inspection is part of this machinery and is not
incidental. See `OneMoreSetupActions/CLAUDE.md` for the enforcement rules.

## Conventions and norms

- **Issues / PRs:** use the `gh` CLI. Default repo is `stevencohn/OneMore`
  (e.g. `gh issue view 2017 --repo stevencohn/OneMore --comments`).
- **Commits are GPG-signed.** See `.github/pull_request_template.md`.
- The .vdproj is **restored from git** on each build. Don't hand-edit it
  casually; if you must, expect `build.ps1` to overwrite it unless `-Local`.
- The installer is **VS Deployment Project (.vdproj), not WiX**. Don't propose
  a WiX migration as scope-creep on unrelated work.

## Where to read more (don't duplicate here)

### General local documentation
- `README.md` — user-facing overview
- `OneMore/Commands/Search/CLAUDE.md` — Search subsystem (virtual ListView,
  atom model, regex replace engine)
- `OneMoreSetupActions/CLAUDE.md` — MSI custom actions, bitness rules, .vdproj
  quirks
- `docs/` — wiki rebuild + telemetry dashboard notes

### Architecture and design docs
- See pages under https://onemoreaddin.com/developers
- Specifically:
   - [TechNote - Interop](https://onemoreaddin.com/developers/TechNote%20-%20Interop.htm)
   - [TechNote - COM Surrogate](https://onemoreaddin.com/developers/TechNote%20-%20COM%20Surrogate.htm)
   - [TechNote - Command Framework](https://onemoreaddin.com/developers/Design%20-%20Command%20Framework.htm)
   - [TechNote - Styles](https://onemoreaddin.com/developers/TechNote%20-%20Styles.htm)