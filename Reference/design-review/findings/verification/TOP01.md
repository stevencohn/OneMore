Title: Tests do not run as a CI gate on push/PR
Verdict: confirmed

# TOP01: Run the existing tests as a CI gate on every push and PR

Sources: synthesis.md Top 10 #1, testing.md #1 (HIGH). ARCH §0's claim that "CI does run the tests" is contradicted.

## Attempts to disprove

1. **Does any workflow run tests on push or PR?** No.
   - `.github/workflows/build.yml:3-4`: the only trigger is `workflow_dispatch`. There is no `push` and no `pull_request`.
   - The other workflows with automatic triggers do not build or test code:
     - `pages.yml` runs on `push` to `main`, but only for `docs/**` paths. It deploys Pages.
     - `pr-issue-link.yml` and `pr-issue-comment-on-merge.yml` run on `pull_request`, but they are github-script jobs on `ubuntu-latest` that edit PR bodies and comment on issues.
     - `cleanup.yml`, `delete-jobs.yml` and `winget-releaser.yml` are housekeeping and release jobs.
   - None of them calls `vstest` or `build.ps1 -Test`.

2. **Does the manual workflow run the tests by default?** No.
   - `build.yml:23-27`: the `run_tests` input has `default: false`.
   - `build.yml:264-267`: the "Run tests" step has `if: matrix.arch == 'x64' && inputs.run_tests`.
   - Even when the box is checked, the step runs late: after assembly signing, MSI build, MSI signing and FinishKit. It also runs before `release`, which depends only on `build` succeeding.
   - Beta builds use only `x64`, so the step is reachable for beta. But it is opt-in every time.

3. **Is the fix feasible on a hosted runner?** Plausibly yes.
   - `build.ps1 -CompileOnly` → `BuildSolution` already restores `OneMoreTests` (`build.ps1:598-606`). The CI build compiles the test project.
   - `RunTests` (`build.ps1:310-345`) uses the VS `vstest.console.exe` with `/Platform:x64` and writes a `.trx`. It returns a non-zero exit (`build.ps1:896`), so it can act as a gate.
   - The tests use `OneMoreTests/MockApplication.cs` rather than a real OneNote install, which supports the claim that no OneNote is needed.
   - Not verified: a clean-clone run on `windows-2025-vs2026`. There could be environment dependencies such as AppData or the DB path (see TEST #8).

4. **Test count and timing (1,154 tests, about 19 s).** These come from the testing report's local run against the last build, not HEAD. They were not re-run here. They do not affect the verdict.

## Conclusion

Confirmed. No automatic workflow builds or runs `OneMoreTests`. The only test step is an opt-in checkbox on a manual release workflow, it defaults to off, and it is placed after signing. The existing build script already supports `-Test` with a failing exit code, so adding a `push`/`pull_request` workflow (`build.ps1 -Fast`, then `build.ps1 -Test`) is low effort, as the finding claims (S).
