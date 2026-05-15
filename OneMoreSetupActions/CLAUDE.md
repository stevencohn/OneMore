# OneMoreSetupActions — MSI custom actions for OneMore

This project produces a **console .exe** that the MSI installer invokes as a
custom action runner. It is *not* a class-library / `[RunInstaller]` /
`Installer`-derived custom-action DLL — that's the first surprise.

## How it's wired into the installer

The installer is `OneMoreSetup/OneMoreSetup.vdproj`, a **legacy Visual Studio
Deployment Project** (binary format, restored from git on each `build.ps1`
unless `-Local`). The .vdproj invokes this .exe via the command line:

```
OneMoreSetupActions --install --x86
OneMoreSetupActions --install --x64
OneMoreSetupActions --install --ARM64
OneMoreSetupActions --uninstall --<arch>
```

`Program.cs` parses those flags and runs the actions in a fixed sequence.

## Build-output quirk

`OneMoreSetupActions.exe` is written into `..\OneMore\bin\Debug\` (or
`..\OneMore\bin\Release\`) so the .vdproj can pick it up alongside the main
add-in binaries. That cross-project output path is intentional — don't
"normalize" it.

## Action contract

Every action under `Actions/` derives from `CustomAction` (`CustomAction.cs`)
and overrides `Install()` / `Uninstall()`, returning Windows Installer exit
codes:

| Constant | Value | Meaning |
|----------|-------|---------|
| `SUCCESS` | 0 | Continue |
| `FAILURE` | 1603 | Fatal error; MSI aborts |
| `USEREXIT` | 1602 | User cancelled |

Returning `FAILURE` from any action aborts the installer immediately.

## Action ordering (matters!)

`CheckBitnessAction` runs **first**. If it returns `FAILURE`, nothing else
runs. Its captured `OneNoteArchitecture` is then passed to downstream actions
(notably `RegistryAction` and `RegistryWowAction`) — they need to know which
OneNote bitness they're registering against.

## Action roster

| File | Purpose |
|------|---------|
| `CheckBitnessAction.cs` | Validates installer / OS / OneNote bitness compatibility — gates everything else. |
| `CheckOneNoteAction.cs` | Confirms OneNote is configured to host add-ins. |
| `ShutdownOneNoteAction.cs` | Force-closes OneNote before file replacement. |
| `RegistryAction.cs` | Writes OneMore's registry settings. |
| `RegistryWowAction.cs` | Clones the CLSID branch under `WOW6432Node` so 32-bit and 64-bit OneNote can both load the add-in. |
| `ActiveSetupAction.cs` | Wires per-user repair on next logon (`msiexec` repair via Active Setup). |
| `ProtocolHandlerAction.cs` | Registers the `onemore://` URL handler. |
| `TrustedProtocolAction.cs` | Marks `onemore://` as safe (companion to ProtocolHandler). |
| `EdgeWebViewAction.cs` | Installs Edge WebView2 (mainly for Windows 10 hosts). |

Helpers: `Logger.cs`, `Stepper.cs`, `RegistryHelper.cs`, `Program.cs` (entry).

## Bitness rules (what `CheckBitnessAction` actually enforces)

OneNote's architecture is determined by reading **OneNote.exe's PE header**
with `System.Reflection.PortableExecutable.PEReader` — *not* by guessing from
registry paths or `WOW6432Node` presence. The matching rules:

- **Windows ARM64** → OneMore must match OneNote (ARM64 OneMore for ARM64
  OneNote; x64 OneMore is allowed when OneNote runs under x64 emulation).
- **Windows x64** → OneMore must match OneNote (x64 with x64, x86 with x86).
- **Windows x86** → must be x86 OneMore with x86 OneNote.

On mismatch, the action shows a `MessageBox` telling the user which installer
flavour to download, and returns `FAILURE` (1603).

## Architecture detection — PE header is the right tool

When code needs to pick the correct installer for the running machine, always read
the **PE header** of the relevant binary (`PEReader.PEHeaders.CoffHeader.Machine`)
and apply the **ARM64EC heuristic**: `Machine.Amd64` on an ARM64 OS means ARM64EC,
not plain x64. Do NOT use `RuntimeInformation.ProcessArchitecture` as a shortcut —
the add-in runs in **dllhost.exe** (COM surrogate), not in `ONENOTE.EXE`, so
`ProcessArchitecture` reflects dllhost's bitness, not OneNote's. For OneNote's
architecture, read `ONENOTE.EXE`'s PE header directly.

## References worth knowing

- `System.Reflection.Metadata` / `System.Collections.Immutable` — used by
  `PEReader` to inspect OneNote.exe.
- `System.Management` — WMI, used by `ShutdownOneNoteAction` to find/kill
  the process tree.
- `System.Windows.Forms` — `MessageBox` for user-facing errors.
- Target framework: **.NET Framework 4.8**; builds for x86, x64, ARM64.
