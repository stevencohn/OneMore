# CheckBitnessAction — findings and fixes

This file documents the bugs found and fixes applied in `CheckBitnessAction.cs`
against issues #1981, #1963, and #1949.

## Issue map

| Issue | Symptom | Root cause | Fix |
|-------|---------|------------|-----|
| #1981 | ARM64 Windows + ARM64EC OneNote → ARM64 installer rejected | ARM64EC OneNote.exe reports `Machine.Amd64` in its PE header; code classifies it as X64, then rejects an ARM64 installer | Detect ARM64EC heuristically: `Machine.Amd64` on an ARM64 OS → treat as Arm64; accept either ARM64 or x64 installer |
| #1949 | OneNote Windows Store app installed (no Desktop) → misleading "use the ARM installer" error | When `GetOneNotePath()` returns null (path not in registry), `GetOneNoteArchitecture` returned the `Architecture.Arm` sentinel, which fell into the bitness-mismatch message path | `GetOneNoteArchitecture` now returns `Architecture?`; null means not detected; `Install()` shows a dedicated "OneNote Desktop not found" message |
| #1963 | Intune / PSADT deployment → add-in not registered per user | Active Setup `msiexec /fu` is blocked by AppLocker/App Control in corporate environments | **Not in this file** — handled in `ActiveSetupAction.cs`. Workaround: write `LoadBehavior` under `HKLM` (see issue thread for registry snippet) |

## Bugs fixed (by finding number)

### F1 — ARM64EC OneNote misdetected as X64 (fixes #1981)

**Before:** `Machine.Amd64` → `Architecture.X64` (the `_` switch arm).

**After:** `Machine.Amd64` when `OSArchitecture == Arm64` → `Architecture.Arm64` (ARM64EC
heuristic). The matching rule for ARM64 Windows is updated to accept either the ARM64 or x64
OneMore installer when ARM64EC OneNote is detected.

**Why the heuristic works:** Office on Windows ARM64 ships as ARM64EC — an x64-compatible
binary that runs natively on ARM64 hardware. Its COFF machine field is `IMAGE_FILE_MACHINE_AMD64`.
A pure x64 Office install on ARM64 Windows is not a supported Microsoft scenario.

**Also updated:** the ARM64 matching rule now reads:
- `(onarc == Arm64 && (urarc == Arm64 || urarc == X64))` — ARM64EC OneNote accepts both installers
- `(onarc == X64 && urarc == X64)` — retained for any pure-x64-emulated OneNote

### F2 — "Not found" used `Architecture.Arm` sentinel → misleading error message (fixes #1949)

**Before:** when OneNote.exe couldn't be located or its PE header couldn't be read, the code
returned `Architecture.Arm` (32-bit ARM), which failed every matching branch and displayed
*"This is an X64 installer. You must use the OneMore Arm installer."* — completely wrong.

**After:** `GetOneNoteArchitecture()` returns `Architecture?` (nullable). `null` means
"not detected." `Install()` checks for null first and shows a clear message:
*"OneNote Desktop was not detected on this system. OneMore requires OneNote Desktop,
not the Microsoft Store app."*

### F3 — Dead `WOWAA64Node` registry lookups

**Before:** `GetOneNotePath()` tried six `WOWAA64Node` registry paths (three in the App Paths
fallback, three in the Office fallback). The registry node `HKLM\SOFTWARE\WOWAA64Node` does not
exist on any shipping Windows version — confirmed in #1981 by users on ARM64 hardware.

**After:** all six `WOWAA64Node` lookups removed. Real ARM64-on-Windows lookup order: native
`SOFTWARE\` hive (covers both native ARM64 and x64-emulated installs), then `WOW6432Node`
(for x86-emulated installs).

### F4 — Install process architecture logged incorrectly on ARM64

**Before:** `Environment.Is64BitProcess ? X64 : X86` — an ARM64 process reports `true` for
`Is64BitProcess`, so the log showed `X64` when running the ARM64 installer. Misled triage.

**After:** `RuntimeInformation.ProcessArchitecture` — returns `Arm64`, `X64`, or `X86` correctly.

### F5 — `logger.Indented` not reset on early-return paths

**Before:** `GetOneNotePath()` sets `logger.Indented = true`; `GetOneNoteArchitecture()` resets
it at the end — but not on the two early-return paths (path not found, file not on disk).
Subsequent log lines were indented incorrectly.

**After:** `logger.Indented = false` added before both null returns.

## What was NOT changed

- `RegistryAction.cs` — only uses `architecture` to choose `ProgramFiles` vs `ProgramFiles(x86)`.
  `Arm64` already maps to `ProgramFiles`. No change needed.
- `RegistryWowAction.cs` — ignores the `architecture` field entirely; self-detects by reading the
  OneNote CLSID path from the registry. No change needed.
- `Program.cs` — `OneNoteArchitecture` remains a non-nullable `Architecture` property. When
  `Install()` returns `FAILURE` (null onarc), `Program.cs` calls `Environment.Exit` immediately;
  the property is never consumed in that path.
- `ActiveSetupAction.cs` — #1963 is out of scope for this file.
