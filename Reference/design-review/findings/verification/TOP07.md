Title: Harden the IPC boundary (CLI allowlist, per-user pipe, server check, framing)
Verdict: confirmed

Synthesis Top 10 #7. Sources: SEC C4, C1, W3; ARCH A4, A5, W1; ROB R7; TEST #4.

All four claims are true in the current code. Two need caveats on impact (see 3 and 4), and one of the suggested fixes is incomplete on its own (see 3). None of them is a false positive.

## 1. CLI allowlist: confirmed (low severity today)

- `OneMore/Commands/CommandService.cs:228-240`: the only gate is `ProtocolUri.IsValidAction(commandName)`, which is the regex `^[A-Za-z_][A-Za-z0-9_]*$` (`ProtocolUri.cs:25-26`). After that comes `Type.GetType($"River.OneMoreAddIn.Commands.{commandName}", false)`.
- Nothing checks `ICliCommand` or `Command` before `cliFactory.Run(...)` (`:316`, `:401`).
- `CommandFactory.Run` overloads (`CommandFactory.cs:105-148`) do `(Command)Activator.CreateInstance(commandType)`. So a non-`Command` type in that namespace with a parameterless constructor gets built, its constructor runs, and only then does the cast throw. Any `Command` runs with `runningFromCli`.
- By contrast, the `onemore://` path does require `[CommandService]` (`CommandFactory.cs:398-402`).
- Ways I tried to disprove it, all of which failed:
  - I found no other `ICliCommand` check lower down.
  - `ProtocolUri` doesn't filter on type.
  - There are no tests for `CommandService`. Only `OneMoreTests/Commands/ProtocolUriTests.cs` exists.
- What limits it: the caller must already reach the pipe. The ACL is current user only (`CreateSecuredPipe`, `:160-178`). A `onemore:` link can't reach this path, because `OneMoreProtocolHandler` forwards `args[0]` as is, and only the `onemore:` scheme is registered, so the data can't start with `onemorecli://`. So it is same-user only, and Low is the right rating. It is a one-line fix and worth doing.

## 2. Machine-global, single-instance pipe; no server check: confirmed (multi-session hosts only)

- The pipe name is the constant CLSID, read from HKCR `River.OneMoreAddIn\CLSID` (`OneMoreSetup/Registry.wxs:113-115`). It is read identically in all three components:
  - `CommandService.cs:47-51`
  - `OneMoreCli/AddinBridge.cs:242-253`
  - `OneMoreProtocolHandler/Program.cs:61-67`
- The `\\.\pipe\` namespace is shared across sessions. `maxNumberOfServerInstances` is 1 (`CommandService.cs:175`).
- Neither client checks the server's identity:
  - `AddinBridge.cs:48-61` connects, writes and trusts any `OK`/`OUTPUT:` reply.
  - `Program.cs:75-87` connects and writes.
  - Neither calls `GetNamedPipeServerProcessId` or checks the owner SID.
- Outage, two legitimate users on one RDS/Citrix host:
  - User A's instance has an ACL that admits only A.
  - When user B's add-in tries to create the pipe, it gets access denied or pipe-busy. Access denied surfaces as `UnauthorizedAccessException`, which the `ERROR_PIPE_BUSY` filter at `:84` doesn't catch, so `ServiceLoop` backs off exponentially (`ServiceLoop.cs:100-108`).
  - B's clients can't connect to A's instance either. IPC is dead for B with no visible error.
- Squatting needs a hostile user to create the pipe first with a permissive ACL. Nothing in the code prevents it.
- On single-session desktops neither problem happens, so the synthesis note in section 4 still applies.

## 3. `WaitForPipeDrain` with no timeout: confirmed, but the fix as worded isn't enough

- `CommandService.cs:740`: synchronous `pipe.WaitForPipeDrain()` inside `WriteCliResponse`, while holding `pipeWriteLock`.
- `ListenOnce` awaits `InvokeCliCommand` inline (`:123`) on the single-instance listener. While it is blocked, no `onemore://` request (Refresh links, etc.) can connect.
- What triggers it: a client that is alive but not reading. A dead client breaks the pipe, and `WaitForPipeDrain` then throws, which is handled. One realistic trigger is a console in QuickEdit/selection mode. That blocks the CLI's `CliConsole.WriteInfo` on `PROGRESS:` lines, so `AddinBridge` stops calling `ReadAsync`. Another is a suspended client process.
- Caveats:
  - `WriteAsync` (`:738`) would also block once the 4 KB out buffer fills. Dropping `WaitForPipeDrain` alone doesn't fix the hang. The write also needs a timeout, or a cancellable or overlapped write with a deadline.
  - The listener is already serialized for the whole length of every CLI command, so a long export blocks Refresh links by design. That is the single-instance problem from item 2, not a drain bug.

## 4. One 4 KB read, no framing: confirmed, but the truncation isn't always silent

- `CommandService.cs:97-114`: a single `ReadAsync(buffer, 0, 4096)`, then `GetString(...)`. There is no delimiter or length prefix.
- The CLI percent-encodes values (`AddinBridge.cs:275`, `Uri.EscapeDataString`). A CJK character becomes 9 bytes (`%E4%B8%AD`), so about 450 CJK characters across notebook, section, page and outpath hit the limit. That is uncommon but realistic.
- The cut can split a UTF-8 sequence or a `%XX` escape, and trailing `&key=value` pairs are lost.
- Caveat: the bytes past 4096 are still in the pipe. `WatchForClientCancellation` (`:286`, `:691-698`) reads them right away and cancels `cts`, so batch page commands stop early with `CANCELLED`. That makes SEC W3's "silently widens to the whole notebook" scenario unlikely. Single-shot commands that ignore the token still run with the truncated parameters. So this is a real correctness bug, but the security impact is lower than stated.

## Summary

| Claim | Holds? | Note |
|---|---|---|
| Any `Commands.*` type runnable via `onemorecli://` | Yes | Same-user only. A one-line `ICliCommand` check fixes it. |
| Machine-global single-instance pipe; no server verification | Yes | Affects multi-session hosts only. |
| `WaitForPipeDrain` can hang the listener | Yes | Needs a live client that isn't reading. Removing the drain alone isn't enough, because the write also needs a deadline. |
| 4 KB single read truncates requests | Yes | Leftover bytes trigger the cancel watcher, so it is only partly silent. |

The suggested order in the synthesis is sound: allowlist first, then read and write deadlines with framing, then a per-user pipe name with a SID check. Adding tests needs an injectable pipe name, as the synthesis says.
