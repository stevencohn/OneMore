Title: Pipeline outside ServiceLoop stop mechanism; stages permanently disabled, no stage timeout
Verdict: confirmed

Finding: synthesis.md Top 10 #4 (sources ARCH A1, ROB R8, related ROB R10).

## Claim 1: PipelineService is not stopped by ServiceLoop.StopAll() (A1) - confirmed

- `OneMore/Helpers/ServiceLoop.cs:27-45`: `StopAll()` cancels only the token sources
  registered by `ServiceLoop.Start` (the static `sources` list). Its doc comment says it
  exists because ApplicationExit is not reliably raised in the dllhost surrogate.
- `OneMore/Services/Pipeline/PipelineService.cs:110-120`: `Startup()` creates its own
  private `serviceToken` and cancels it only from `OnApplicationExit` (lines 135-139).
  Nothing registers it with `ServiceLoop`, and there is no public Stop method.
- `OneMore/AddIn.cs:372-394`: the pipeline is created with `new PipelineService(...).Startup()`
  and no reference is kept, so nothing else can ever stop it.
- `AddIn.cs:331`: the startup `ServiceLoop.StopAll()` (meant to clear a previous instance's
  loops in a lingering dllhost) cannot reach a previous instance's pipeline.
- `AddIn.cs:498-517` (`StopServices`, used by `OnBeginShutdown` and by `OnHostExited`):
  calls `ServiceLoop.StopAll()`, then `Application.Exit()`. The pipeline is stopped only if
  that `Application.Exit()` actually raises `ApplicationExit` in this host, which is the
  exact assumption the codebase itself says is unreliable.
- The re-entrancy guard `running` (line 69) is an instance field, so two `PipelineService`
  instances in one process do not exclude each other.
- The stages do drive OneNote via COM on every cycle without checking that OneNote is
  alive: `IdentityStage` (`OneNoteHierarchySource`, every 2 min, `IsReady` always true),
  `HealingStage.cs:80` (`new OneNote()`, every 2 min, `IsReady` always true). So an
  orphaned pipeline would keep activating OneNote, the failure `WatchHost` was written
  to prevent (`AddIn.cs:520-522`).

Caveat: in the host-watcher path, `Application.Exit()` may well raise `ApplicationExit`
and cancel the pipeline, which narrows the window. The startup path (`AddIn.cs:331`) and
the case where the host watcher could not start (`AddIn.cs:526-529`, pid unknown) have
no fallback at all.

## Claim 2: stages are switched off permanently after 5 failures (R8) - confirmed

- `PipelineService.cs:307-319`: after `MaxFailures` (5) consecutive exceptions,
  `state.Off = true`. No code path ever sets `Off` back to false; `RunCycle` skips `Off`
  stages (line 214) and `NextDelay` drops them (line 332). The class remarks (lines 37-38)
  state this is by design: "switched off for the rest of the session".
- Identity and healing run every `IdentityStage.PassInterval` = 2 min, so five failures is
  about 10 minutes of a busy or failing OneNote. The only signal is a log line.
- Contrast: `ServiceLoop` (lines 100-108) uses capped exponential backoff and never
  gives up, so the two mechanisms have inconsistent error policies.

## Claim 3: no per-stage timeout; a hung COM call wedges the pipeline (R8) - confirmed, detail corrected

- `RunStage` (lines 285-324) awaits `stage.IsReady(token)` and `stage.Run(context, token)`
  with only the service token; there is no deadline or linked token.
- `Run` (lines 145-174) awaits `RunCycle` sequentially, and `RunCycle` is called nowhere
  else in production code (only from `OneMoreTests/Pipeline/PipelineServiceTests.cs:97`).
  So a hung stage simply blocks the loop forever; the "skipping cycle" guard path is not
  what users would see. The effect (all later stages and cycles stalled) is the same.
- Note: a linked-token deadline only helps if the stage honors cancellation; a blocked
  synchronous COM call will not observe it, so the fix also needs the host to stop
  awaiting (e.g. `Task.WhenAny` with a delay) and abandon the stuck stage.

## Related R10: static ResolveDatabasePath - confirmed in code, rarer than implied

- `OneMore/Helpers/DatabaseProvider.cs:27`: `path` is a static field initializer, so any
  exception makes `DatabaseProvider` a permanently broken type for the process.
- Line 94: `mutex.WaitOne()` is outside the `try`, so `AbandonedMutexException` escapes
  (and `ReleaseMutex` in `finally` is never reached in that case anyway).
- Line 80: `EnsurePathExists` can throw before any guard.
- However, the mutex is only taken on the one-time migration path (new db missing and
  legacy roaming db present, lines 84-89), so the abandoned-mutex case is rare. The
  `EnsurePathExists` failure is the more plausible trigger. When it happens, every DB-backed
  stage fails each cycle and trips the permanent off from claim 2, as stated.

## Summary

All three parts of the finding hold in the current code. The one overstatement is the
host-watcher shutdown path, where `Application.Exit()` may still cancel the pipeline;
the startup cleanup path and no-watcher case remain unprotected. Effort S is reasonable
for registering the token and adding a cool-down; a real per-stage deadline is slightly
more work because COM calls do not observe cancellation.
