# OneMore — Security review findings

Scope: read-only review through the security lens, following
`specs/design-review/onemore-architecture-brief.md`. Every finding below points at
code I read. Severities assume the usual desktop threat model: the attacker is
**another local user**, **content the user opens** (web pages, email, Office docs,
shared notebooks), or **the network/update channel**. I did not rate
same-user-process attacks above Low, because a process running as the user can
already drive OneNote through COM.

Trust boundaries I checked:

| Boundary | Component / privilege |
|---|---|
| Browser / Office link → `onemore://` | `OneMoreProtocolHandler.exe` (user) → named pipe → add-in in dllhost (user) |
| CLI → add-in | `OneMoreCli.exe` (user) → same named pipe (`onemorecli://`) |
| Pipe between users on the same host | fixed, machine-wide pipe name |
| GitHub Releases → updater | add-in downloads the installer, then runs an elevated MSI/Burn bundle |
| MSI UI (user) → deferred custom actions (SYSTEM, `Impersonate="no"`) | `OneMoreSetupActions.exe` |
| Add-in → telemetry / kroki / PlantUML | outbound HTTPS |

---

## Confirmed issues

### C1. Pipe squatting: clients never check who owns the command pipe, and its name is the same for every user — **Medium** (High on RDS/Citrix hosts)
- **Confidence:** medium. The code paths are certain. To confirm, on a multi-session host start a listener as user B on `\\.\pipe\{88AB88AB-CDFB-4C68-9C3A-F10B75A5BC61}` with an Everyone ACL before user A starts OneNote, then click a `onemore://` link or run `OneMoreCli` as A.
- **Location:**
  - `OneMoreSetup/Registry.wxs:113-115`: the pipe name is the constant CLSID, written machine-wide
    ```xml
    Key="River.OneMoreAddIn\CLSID" ...
    <RegistryValue Value="{88AB88AB-CDFB-4C68-9C3A-F10B75A5BC61}" Type="string" />
    ```
  - `OneMoreProtocolHandler/Program.cs:75-85` and `OneMoreCli/AddinBridge.cs:48-61`: the clients connect and write without checking the server
    ```csharp
    using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.Out, PipeOptions.Asynchronous);
    client.Connect(500);
    ... client.Write(buffer, 0, buffer.Length);
    ```
  - `OneMore/Commands/CommandService.cs:160-178`: the server ACL protects only the server's own instance. It cannot stop another user from creating the pipe first.
- **Why it matters:** named pipes share one namespace across all sessions on a machine. A user who creates the pipe first (logged on earlier, or a script at logon) becomes "OneMore" for everyone else:
  - They receive every other user's `onemore://` requests (page IDs, embed source IDs, Outlook contact GUIDs) and CLI requests (notebook/section/page names, `outpath`).
  - They can send back made-up `OUTPUT:`/`OK` replies to the CLI, which `AddinBridge` prints as real results. A CLI `export --backup` would "succeed" without writing anything.
  - The real user's add-in then fails to create its pipe instance. You get access denied, not `ERROR_PIPE_BUSY`, so `ListenOnce` keeps failing and IPC is silently dead for that user.
- **Fix:** make the pipe name per-user, e.g. `$"{clsid}-{WindowsIdentity.GetCurrent().User.Value}"` (plus the session ID if wanted), and compute it the same way in all three components. In both clients, verify the server before writing: call `GetNamedPipeServerProcessId`, open the process token and compare its SID to the current user. Or check `pipe.GetAccessControl().GetOwner(typeof(SecurityIdentifier))` against the current user.
- **Effort:** S–M (three call sites, plus a shared helper).

### C2. The updater's checksum comes from the same origin as the binary, a mismatch can be clicked through, and there is no signature check before the elevated install — **Medium**
- **Confidence:** high.
- **Location:** `OneMore/Commands/Tools/Updater/Updater.cs`
  - `:304-311`: a mismatch or missing checksum can be bypassed if the user clicks Yes
    ```csharp
    var status = VerifyChecksum(installerPath, key);
    if (status != ChecksumStatus.Verified &&
        (confirmProceed is null || !confirmProceed(status)))
    ```
  - `:363-396`: the expected hash is parsed from `release.body`, which is the same GitHub release that supplies `browser_download_url`
  - `OneMore/Commands/Tools/UpdateCommand.cs:94-101`: `ConfirmChecksumStatus` is a plain Yes/No box
- **Why it matters:** the hash only catches transport corruption. Anyone who can edit the release (a compromised maintainer token, or a CI release step) controls both the asset and its hash. Worse, **`NotFound`** is a normal state, because the checksum section is "hand-maintained". Users get trained to click Yes, so even a mismatch becomes a one-click install. The MSI/bundle then runs elevated. Nothing checks Authenticode (`WinVerifyTrust`) or a pinned publisher/thumbprint.
- **Fix:** check the Authenticode signature of the downloaded MSI/EXE and pin the expected signer subject or thumbprint. Refuse to install on `Mismatch` with no override. Make `NotFound` fail closed once releases reliably carry checksums, or better, publish a detached signature (e.g. minisign/cosign) with the public key embedded in the add-in.
  ```csharp
  if (status == ChecksumStatus.Mismatch || !AuthenticodeVerifier.IsSignedBy(installerPath, ExpectedThumbprint))
      return false;
  ```
- **Effort:** M.

### C3. SYSTEM-context database backup copies between user-controlled paths without guarding against links — **Medium**
- **Confidence:** medium. To confirm: as the standard user whose profile the MSI UI resolves (for example over-the-shoulder UAC with admin credentials), replace `%LOCALAPPDATA%\OneMore` with a junction to a protected folder, run the pre-8.0 upgrade, and see whether `OneMore-backup-*.db` lands there owned by SYSTEM. Separately, try a mount-point/object-manager symlink in place of `OneMore.db` to read a protected file into a user-readable folder.
- **Location:**
  - `OneMoreSetup/Product.wxs:174-179`: deferred, `Impersonate="no"`, with paths taken from public properties
    ```xml
    ExeCommand="--backup-db &quot;[BACKUPDIR]|[ONEMOREDB]&quot; --$(var.ArchFlag)"
    Execute="deferred" Return="ignore" Impersonate="no" />
    ```
  - `OneMoreSetup/BackupDlg.wxs:11-14,24-41`: `BACKUPDIR` and `ONEMOREDB` are public and can be set from the command line. `ONEMOREDB` resolves under the user's `%LOCALAPPDATA%`.
  - `OneMoreSetupActions/Actions/BackupDatabaseAction.cs:72-83`
    ```csharp
    Directory.CreateDirectory(folder);
    var target = MakeUniquePath();
    File.Copy(source, target, false);
    ... File.Copy(source + suffix, target + suffix, false);
    ```
- **Why it matters:** this is the classic "privileged file operation on user-writable paths" pattern. SYSTEM follows junctions and symlinks in both the source and the destination. That allows writing attacker-chosen content into protected directories, or copying protected files into a user-readable folder. Exploitation is limited by the fixed `OneMore-backup-<stamp>.db` name and the pre-8.0 upgrade condition, but the primitive is there.
- **Fix:** do the copy in an **immediate, impersonated** custom action (`Impersonate="yes"`). The source and destination are both in the user's profile, so SYSTEM is not needed. If it has to stay deferred, open the source with `FILE_FLAG_OPEN_REPARSE_POINT`, reject reparse points anywhere in either path, and check that both paths are under the expected user profile root.
- **Effort:** S (move to an impersonated CA) / M (hardening in place).

### C4. The CLI path skips the command allowlist and instantiates before type-checking — **Low**
- **Confidence:** high.
- **Location:** `OneMore/Commands/CommandService.cs:228-240,401-403` compared with `OneMore/Commands/CommandFactory.cs:398-402`
  ```csharp
  // CLI path
  var typeName = $"River.OneMoreAddIn.Commands.{commandName}";
  var commandType = Type.GetType(typeName, false);
  ...
  result = await cliFactory.Run(commandType, cts.Token, ..., parameters);
  // CommandFactory.Run
  var command = (Command)Activator.CreateInstance(commandType);
  ```
  The `onemore://` path, by contrast, requires `[CommandService]` (`CommandFactory.cs:398`).
- **Why it matters:** the `onemorecli://` route never checks `ICliCommand`. Any type in the `Commands` namespace with a parameterless constructor gets built, and its constructor runs before the cast fails for non-`Command` types. Any `Command` then runs with `runningFromCli`, including commands never designed for headless use (dialogs, destructive bulk edits). Today the pipe ACL limits callers to the same user, which keeps this Low. But if C1 were "fixed" by loosening the ACL, or the pipe were ever exposed more widely, this becomes the main escalation point.
- **Fix:** before instantiating, require `typeof(Command).IsAssignableFrom(commandType) && typeof(ICliCommand).IsAssignableFrom(commandType)`. Better, look the name up in an explicit dictionary built from the types `OneMoreCli/CommandDiscovery.cs` exposes.
- **Effort:** S.

### C5. The updater's install script and download are fragile (unquoted paths, no truncation, no HTTP status check) — **Low**
- **Confidence:** high.
- **Location:** `OneMore/Commands/Tools/Updater/Updater.cs`
  - `:286,293-296`
    ```csharp
    var installerPath = Path.Combine(Path.GetTempPath(), asset.name);
    using var response = await client.GetAsync(asset.browser_download_url);
    using var stream = await response.Content.ReadAsStreamAsync();
    using var file = File.OpenWrite(installerPath);   // does not truncate
    ```
  - `:326-330,340-344`
    ```csharp
    await writer.WriteLineAsync(installerPath);        // unquoted in .cmd
    Arguments = $"/c {path}",                           // unquoted
    ```
- **Why it matters:**
  - The `.cmd` contains the raw, unquoted installer path. A `%TEMP%` containing spaces breaks it, and one containing `&`, `^` or `%` lets cmd interpret those characters. Some usernames create such paths.
  - `asset.name` is used as a file name in the shell script unchanged.
  - `File.OpenWrite` leaves the tail of an older, larger file in place, and a non-2xx body (an HTML error page) gets written as the "installer". The checksum then fails, which feeds straight into the click-through in C2.
  - The script name `OneMoreInstaller.cmd` is predictable.
- **Fix:** call `response.EnsureSuccessStatusCode()`. Write with `FileMode.Create`. Use a random sub-folder of `%TEMP%`. Quote every path (`"\"{installerPath}\""`). Better still, skip the `.cmd` and start `msiexec /i "<path>"` (or the bundle) directly through `ProcessStartInfo` with an argument list.
- **Effort:** S.

### C6. Telemetry error events send unscrubbed exception text — **Low** (privacy)
- **Confidence:** high for the client side. Check whether the API Gateway/Lambda redacts anything on the server.
- **Location:** `OneMore/Commands/Tools/Telemetry/TelemetryClient.cs:108-111`, `OneMore/Helpers/Extensions/ExceptionExtensions.cs:57-73`
  ```csharp
  await Log("error", eventName, message, exc.FormatDetails());
  ...
  builder.AppendLine($"{h}Message: {exc.Message}");
  ```
- **Why it matters:** .NET exception messages often contain full file paths (`C:\Users\<name>\...`), notebook or section names in path or COM errors, URLs, and sometimes user-entered text such as regex patterns or formula text. Telemetry is opt-in, but users agreed to anonymous usage data, not their profile names and notebook titles. The same text also appears in the verbose log (`TelemetryClient.cs:149`).
- **Fix:** scrub before sending: replace `%USERPROFILE%`, `%TEMP%` and `%LOCALAPPDATA%` prefixes with tokens, strip anything that looks like a path or URL from `Message`, and keep the type plus the stack (which only has method names). Add a unit test with a sample `FileNotFoundException`.
- **Effort:** S.

### C7. The telemetry API key is embedded, so anyone can write to the telemetry store — **Low**
- **Confidence:** high (the brief already notes it as intentional).
- **Location:** `OneMore/Commands/Tools/Telemetry/TelemetryClient.cs:25`
  ```csharp
  private static readonly string ApiKey = "F3J9FKPYsX7gypXLaBQmITRu5DCoIe77x8jgV4m0";
  ```
- **Why it matters:** the key is not a secret, which the code admits. The risk is **data integrity and cost**: anyone can POST forged `error`/`event` records, skewing the usage and error dashboards (and the `fix-telemetry`/`estimate-usage` workflows built on them), or run up API Gateway and storage costs.
- **Fix:** make sure the API Gateway usage plan has tight throttle and quota limits. Validate the schema and size on the server. Treat telemetry as untrusted input in the dashboards (no HTML rendering of `Message`/`Info`). Rotating the key does not help while it ships in the binary.
- **Effort:** S.

### C8. The image viewer is launched through `cmd /c start` with an unquoted path — **Low**
- **Confidence:** high.
- **Location:** `OneMore/Commands/Images/OpenImageWithCommand.cs:80-84`
  ```csharp
  info.FileName = "cmd.exe";
  info.Arguments = $"/c start {path}";
  ```
- **Why it matters:** `path` is under `%TEMP%`. A temp path with spaces or cmd metacharacters (from the user profile name) breaks the launch or lets cmd interpret those characters. Separately, `imageViewer` from settings runs as an executable (`:74-77`). That is same-user configuration, so it is noted, not rated.
- **Fix:** use `new ProcessStartInfo(path) { UseShellExecute = true }` with no cmd.
- **Effort:** S.

---

## Worth investigating

### W1. `onemore://` is marked trusted in all Office apps, so links in email, documents or shared notebooks run `[CommandService]` commands without a prompt — **Medium?**
- **Confidence:** low–medium. To confirm: put `onemore://RefreshFootnotesCommand` or `onemore://OutlookContactCommand/save/<guid>/Both` in an Outlook email and a Word doc, click it, and see that no Office warning appears and the command runs against whatever page is current in OneNote.
- **Location:**
  - `OneMoreSetupActions/Actions/TrustedProtocolAction.cs:32,116`: `...\Security\Trusted Protocols\All Applications\onemore:`
  - The reachable set is every `[CommandService]` type (found by grep): `EmbedCommand`, `EmbedSubpageProxy`, `OutlookContactCommand` (`save` writes back to Outlook contacts, `OutlookContactCommand.cs:60-70`), `ImportOutlookContactsCommand`, `ImportOutlookTasksCommand`, `LinkReferencesCommand`, `RefreshFootnotesCommand`, `InsertTocCommand`, `MermaidCommand`/`PlantUmlCommand` (diagram text is sent to kroki.io/PlantUML), `RemindCommand`, `ReportRemindersCommand`, `SummarizeHighlightsCommand`, `FormControlCommand`, `GotoCommand`.
- **Why it matters:** the policy applies to *All Applications*, not just OneNote. So a crafted link in an email, a Word or Excel document, or a page a collaborator edits in a shared notebook triggers page-modifying commands on the victim's current page in one click, without the Office "this link may be unsafe" prompt. Arguments are URL-decoded strings with no further validation (`ProtocolUri.cs:67`).
- **Suggested fix:** restrict the trusted-protocol key to OneNote (`...\Trusted Protocols\OneNote\onemore:` if your Office version supports per-app keys), not `All Applications`. Review each `[CommandService]` command for one-click side effects. For commands that write outside OneNote (Outlook `save`) or send content off-box (diagrams), ask the user to confirm before acting on a protocol call.
- **Effort:** S (scope the key) / M (per-command review).

### W2. Embed refresh trusts a source page ID stored in page content, which may allow exfiltration through a shared notebook — **Medium?**
- **Confidence:** low. To confirm: see whether a collaborator can write a `Meta name=EmbedMetaName content=<victim private page id>` plus a refresh link into a shared page, and whether the victim's refresh then copies the private page's content into the shared page.
- **Location:** `OneMore/Commands/References/EmbedCommand.cs:127-151` (protocol `refresh` args), `:1296-1335`
  ```csharp
  var currentSourceId = meta.Attribute("content").Value;
  var source = await GetSource(currentSourceId, objectId, linkId, beginTag, endTag);
  ...
  await one.Update(page);
  ```
- **Why it matters:** the page that triggers the refresh decides which page gets read, and the result is written into that same page, which may be shared. This is a confused-deputy pattern. Practical risk depends on whether an attacker can learn a victim's page ID (from copied links, `onemore://` links the victim shared, or the logs).
- **Suggested fix:** on refresh, check that the source page is in the same notebook as the target, or warn when content crosses notebooks.
- **Effort:** S–M.

### W3. The pipe reader takes a single 4096-byte read with no framing — **Low**
- **Location:** `OneMore/Commands/CommandService.cs:97-114`
  ```csharp
  bytesRead = await server.ReadAsync(buffer, 0, MaxBytes, cts.Token);
  var data = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim((char)0);
  ```
- **Why it matters:** a request longer than 4 KB, or one split across writes, gets cut off silently. The CLI's `&`-separated parameters (`CommandService.cs:248-256`) then lose trailing keys, such as a dropped `section`, which can widen a CLI command from one section to the whole notebook (`:339-342`). A cut can also split a multi-byte UTF-8 character. To confirm: run `OneMoreCli` with a very long `--page` or `--outpath`.
- **Suggested fix:** use a length prefix, or read until EOF/newline with a hard limit, and reject oversize requests with `ERR:`.
- **Effort:** S.

### W4. The WebView2 bootstrapper runs as SYSTEM without a signature check — **Low**
- **Location:** `OneMoreSetupActions/Actions/EdgeWebViewAction.cs:69-88`
- **Why it matters:** the download goes over HTTPS from `go.microsoft.com` into a random name under SYSTEM's temp folder with `FileMode.CreateNew`, so planting is unlikely. But nothing checks Authenticode before executing as SYSTEM. If TLS is inspected by a corporate proxy with a trusted root, that proxy can swap the binary. To confirm: check whether any enterprise TLS-inspection environments are in scope.
- **Suggested fix:** call `WinVerifyTrust` and check that the signer is Microsoft before `Process.Start`.
- **Effort:** S.

### W5. Protocol handler command line uses unquoted `%1 %2 %3 %4 %5` — **Low**
- **Location:** `OneMoreSetupActions/Actions/ProtocolHandlerAction.cs:153`
  ```csharp
  var cmd = $"\"{exe}\" %1 %2 %3 %4 %5";
  ```
- **Why it matters:** a URL containing spaces or quotes (possible from non-browser launchers such as Office or the Run dialog) is split into several argv entries. `Program.Main` only forwards `args[0]` (`Program.cs:41`), so this mostly truncates. But a crafted link whose first token is `-Embedding` sends the process down the COM-server path (`Program.cs:34-38`), which then waits 15 s. That is only a nuisance, but the quoting is still wrong. To confirm: test `onemore:" -Embedding` from Word.
- **Suggested fix:** register `"\"{exe}\" \"%1\""`.
- **Effort:** S.

---

## Areas reviewed with no findings
- SQL: every dynamic `CommandText` interpolates constants or schema names only (e.g. `VariableProvider.cs:16,102`, `HashtagProvider.cs:83`). User values are parameterized.
- XML: `EnexReader` sets `DtdProcessing.Ignore` and `XmlResolver = null`. `XElement.Load` defaults are safe on .NET 4.8. No `BinaryFormatter` or `TypeNameHandling` anywhere.
- FTS5 `LoadExtension` turns extensions on only for the duration of the call (`IndexProvider.cs:329-342`).
- `ProtocolUri.IsValidAction` blocks assembly-qualified and nested type names in `Type.GetType` (`ProtocolUri.cs:25-35`).
- Plugin execution escapes quotes in arguments and uses `UseShellExecute=false` (`RunPluginCommand.cs:293-321`). Plugins are same-user configuration by design.
- Logs live under the per-user `%TEMP%` (`Logger.cs:52`).

---

## Top 5 highest-impact items
1. **C2 — Updater integrity:** add an Authenticode/publisher check and remove the click-through on checksum mismatch, because an elevated install is the biggest prize.
2. **C1 — Pipe squatting across sessions:** use a per-user pipe name and have clients verify the server's SID. This matters a lot for RDS/Citrix deployments.
3. **C3 — SYSTEM backup copy on user-controlled paths:** move it to an impersonated custom action.
4. **W1 — `onemore:` trusted for All Office Applications:** limit it to OneNote and add confirmation to protocol-invoked commands that act outside OneNote.
5. **C4 — CLI allowlist:** require `ICliCommand` (and `Command`) before `Activator.CreateInstance`. This is cheap and keeps the IPC surface honest if C1's fix changes the pipe.
