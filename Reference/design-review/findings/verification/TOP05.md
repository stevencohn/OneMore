Title: Harden the updater (Authenticode, no mismatch click-through, sound download, quoted paths)
Verdict: confirmed

# TOP05: verification

Sources: synthesis.md Top 10 #5, which draws on SEC C2, SEC C5, ROB R6 and TEST #5.
Code checked: `OneMore/Commands/Tools/Updater/Updater.cs`, `OneMore/Commands/Tools/UpdateCommand.cs`,
`OneMore/Helpers/HttpClientFactory.cs`, `OneMore/Properties/Resources.resx`, `OneMoreSetup/Product.wxs`,
`.github/workflows/build.yml`, `OneMoreTests/`.

I tried to disprove each claim. Every one holds when checked against the current code.

## 1. The checksum comes from the same origin as the binary: confirmed
- `Updater.VerifyChecksum` (l.363-379) calls `ParseChecksum(release.body, key)`. The expected hash comes from the text of the same GitHub release that supplies `asset.browser_download_url` (l.279). Anyone who can edit the release controls both values.
- Nothing in the repo checks Authenticode. A grep for `WinVerifyTrust|Authenticode|X509Certificate.CreateFromSignedFile` across all `*.cs` files finds nothing.

## 2. A mismatch can be clicked through: confirmed
- `Updater.Update` l.306-311 aborts only if `status != Verified` **and** `confirmProceed` declines.
- `UpdateCommand.ConfirmChecksumStatus` (l.94-101) shows a plain Yes/No box (`MoreMessageBox.ShowQuestion`) for both `Mismatch` and `NotFound`. The mismatch text even says "This could indicate ... tampering. Proceed with installation anyway?" (`Update_ChecksumMismatch`). Yes then installs.
- `ParseChecksum`'s own comment says the section is "hand-maintained" and "typed by hand", so `NotFound` really is a normal state. That trains users to click Yes.

## 3. The download is unsound: confirmed
- l.293: `client.GetAsync(...)` with no `EnsureSuccessStatusCode()`. A 4xx/5xx or rate-limit body gets written as the installer.
- l.295: `File.OpenWrite(installerPath)` does not truncate. The path is `%TEMP%\<asset.name>`, and asset names follow a fixed `Setup{key}.{ext}` pattern, so an earlier download of the same name is likely to still be there. A shorter new file keeps the stale tail.
- In both cases the result is `Mismatch`, which brings up the click-through prompt from point 2.
- `HttpClientFactory.Create()` returns a shared singleton with the default 100 s `Timeout`, and `GetAsync` buffers the whole response by default (no `HttpCompletionOption.ResponseHeadersRead`). ROB R6's slow-link failure claim holds.

## 4. Paths are unquoted: confirmed
- l.330: `writer.WriteLineAsync(installerPath)` writes the raw, unquoted path into `OneMoreInstaller.cmd`.
- l.343: `Arguments = $"/c {path}"`, also unquoted. `Path.GetTempPath()` returns the long profile path, so a username with a space (or `&`, `^`, `%`) breaks the script or changes how cmd parses it.
- The script name `OneMoreInstaller.cmd` is fixed and sits in `%TEMP%`, so it is predictable.
- Only the shutdown line (l.326) is quoted correctly.

## 5. No tests: confirmed (TEST #5)
- `OneMoreTests` has no reference to `Updater` or `ParseChecksum`.

## 6. The install ends elevated: confirmed
- `OneMoreSetup/Product.wxs:20` sets `Scope="perMachine"`, so the MSI or bundle runs with elevation after a UAC prompt.

## Mitigating context (does not change the verdict)
- Release artifacts **are** Authenticode-signed. `.github/workflows/build.yml` signs the assemblies, the MSI and the ARM64 bundle through SignPath: the `release-signing` policy for releases, `test-signing` for betas. That makes the proposed signer-pinning fix practical. The pin has to allow for the SignPath Foundation production certificate, and it must decide how to treat test-signed prereleases (`ONEMORE_ALLOW_PRERELEASE`).
- UAC on a per-machine MSI shows the publisher, which is a weak, human-only check. It does not replace a programmatic signer check.
- `GuardedByClickToRun` returns `!requested`, so updates run only when the user asks from the About box. That limits how often the path is used, not the integrity gap.
- Exploiting this needs either control of the GitHub release (a compromised maintainer or CI token) or a failed or stale download that the user then accepts. TLS 1.2/1.3 limits plain network MITM. The "Medium" rating in SEC C2 is reasonable. The other items (C5, R6) are robustness defects that feed into the click-through.

## Secondary items from R6, also confirmed
- l.79: `cmd.Substring(cmd.IndexOf('{'))` throws `ArgumentOutOfRangeException` if `UninstallString` has no `{`, and it runs in the constructor.
- l.224-225: `new Version(...)` sits outside the try block. l.279: `release.assets.Find` would throw a NullReferenceException if `assets` is null.
