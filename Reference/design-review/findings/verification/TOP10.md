Title: Elevated installer actions: SYSTEM DB backup on user paths, unsigned WebView2 bootstrapper
Verdict: confirmed

Finding: synthesis.md Top 10 #10 (sources: SEC C3, SEC W4, DEP #6). Two claims, both checked against the code. Both hold. Some points need narrowing on severity and on the fix, as noted below.

## Part 1: SYSTEM-context DB backup (SEC C3)

### Evidence the finding holds

- `OneMoreSetup/Product.wxs:20`: `Scope="perMachine"`, so deferred `Impersonate="no"` actions run as LocalSystem.
- `OneMoreSetup/Product.wxs:174-179`: `CA_BackupDatabase` is `Execute="deferred"` and `Impersonate="no"`, with `ExeCommand="--backup-db "[BACKUPDIR]|[ONEMOREDB]" ..."`. The comment at `:165-168` says it "runs as SYSTEM".
- `OneMoreSetup/BackupDlg.wxs:24-41`: `BACKUPDIR` (default `[LocalAppDataFolder]OneMore\`) and `ONEMOREDB` (found by `DirectorySearch` under `[%LOCALAPPDATA]\OneMore`) are public, `Secure="yes"` properties. The comment at `:11` says they can be set on the msiexec command line. Both default to the invoking user's profile, which that user can write to.
- `OneMoreSetupActions/Actions/BackupDatabaseAction.cs:63-83`: `File.Exists(source)`, then `Directory.CreateDirectory(folder)`, then `File.Copy(source, target, false)`, then the `-wal`/`-shm` sidecars. Nothing checks for reparse points, validates paths, or restricts paths to the profile root. A repo-wide search finds no `ReparsePoint` handling.
- `MakeUniquePath` (`:107-120`) uses `OneMore-backup-yyyyMMdd-HHmmss.db`. The name is fixed and resolves to the second, so it is predictable.
- `Program.cs:120-124` routes `--backup-db` straight to this action.

### Attempts to disprove

- **Is it reachable?** Yes, but only narrowly. The condition is `NOT Installed AND PRE8VERSIONFOUND AND BACKUPDB="1" AND ONEMOREDB` (`Product.wxs:217`), so it runs only on an upgrade from a version before 8.0, when a DB exists. The attacker must be the user whose profile the client-side AppSearch resolves. In a per-machine MSI that a standard user launches, the client UI runs as that standard user, and elevation (over-the-shoulder admin credentials) happens only for the server-side execute sequence. That standard user is the one who controls `%LOCALAPPDATA%\OneMore`. If the user is already an admin, there is nothing to gain.
- **Does the fixed file name stop exploitation?** No. Replacing `%LOCALAPPDATA%\OneMore` with a junction to `\RPC Control` and adding an object-manager symlink for the predictable `OneMore-backup-<stamp>.db` name redirects the write to any path. This is the standard Forshaw technique. The content written is the attacker's own `OneMore.db`. The result is an arbitrary file write by SYSTEM with controlled content. The reverse also works: a link in place of `OneMore.db` makes SYSTEM copy a protected file into a folder the user can read.
- **Is it handled elsewhere?** No. Nothing in the WiX authoring or the action guards against this.
- **Is SYSTEM needed?** No. The source and default destination are both in the user's profile. The only other step is `ShutdownOneNoteAction.Install()` (`:70`), which closes the user's own OneNote. The user can do that without elevation.

### Caveats on the fix

The synthesis says to use an "immediate, impersonated" action. Either an immediate action, or a deferred action with `Impersonate="yes"`, would run the copy as the user and remove the primitive. Keep the backup before `CA_Install`. With impersonation, `BACKUPDIR` set from the command line can only reach places the user can already write to, which is acceptable. SEC C3 rated this Medium with medium confidence. The code-level primitive is confirmed. No live exploit was attempted.

## Part 2: WebView2 bootstrapper without signature check (SEC W4, DEP #6)

### Evidence the finding holds

- `OneMoreSetupActions/Actions/EdgeWebViewAction.cs:31`: `DownloadUrl = "https://go.microsoft.com/fwlink/p/?LinkId=2124703"`.
- `:69-71`: the file is saved to `Path.GetTempPath()` with a random name ending in `.exe`. `:165-175` downloads it with `HttpClient` into a `FileMode.CreateNew` stream.
- `:86`: `Process.Start(bootstrap)` runs right after the download. Nothing checks Authenticode, the publisher, or a hash. A repo-wide search finds no `WinVerifyTrust`, `CreateFromSignedFile` or `Authenticode`.
- Reached from `--install` (`Program.cs:264`), which `CA_Install` runs (`Product.wxs:181-186`, deferred, `Impersonate="no"`). So the bootstrapper runs as SYSTEM.

### Attempts to disprove

- The download uses HTTPS to a Microsoft host, and .NET Framework validates certificates by default. No `ServerCertificateValidationCallback` override was found in this action. An attacker therefore needs a trusted man-in-the-middle, such as a corporate TLS-inspection proxy with an enterprise root. Otherwise they need to compromise the redirect chain.
- The random name, `CreateNew`, and SYSTEM's temp folder make local planting unlikely.
- It is skipped when the WebView2 runtime is already registered (`:62-67`), which covers most Windows 11 machines.

These points limit exploitability. They do not remove the gap. SEC W4 rates it Low and DEP #6 rates it Medium. Low–Medium is fair.

### Caveat on the fix

A signature check alone does not prove the file is the bootstrapper. Any Microsoft-signed executable would pass. To be safe, also check the signer chain to the Microsoft root, and delete the file if the check fails.

## Conclusion

Both halves are accurate about the code. The backup's SYSTEM file copy on user-controlled paths is a real elevation primitive in the over-the-shoulder UAC case, and SYSTEM is not needed for it. The WebView2 executable runs as SYSTEM with no signature check. The severities in the sources (Medium and Low–Medium) and effort S are reasonable. Both have narrow preconditions: an upgrade from before 8.0 plus a standard user with OTS elevation for the backup, and a trusted TLS interceptor for WebView2.
