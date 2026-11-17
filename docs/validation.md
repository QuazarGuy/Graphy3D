# Validation boundary and repository setup

## What exists

The `Validate` workflow uses fresh GitHub-hosted Linux and Windows jobs,
read-only repository permissions, SHA-pinned actions, no persisted checkout
credentials, no package/build caches, and no configured secrets or release
steps. Each job has a 15-minute timeout.

Core tests run on both platforms; solution build and rendering tests run on
Windows. Checks execute candidate code, so these jobs are untrusted execution.
Do not switch them to self-hosted runners or add secrets, deployment access,
signing credentials, or privileged follow-on artifact execution.

The workflow uses `pull_request`, not `pull_request_target`. In particular,
do not use privileged triggers to check out and run an untrusted patch.

## What does not exist yet

Local files do not enable GitHub protections. The workflow has not been run
remotely until the foundation is reviewed and published. No external
coordinator, isolated central validator service, frozen independent conformance
suite, release pipeline, or automatic task dispatch exists yet.

These checks do not prove that code is safe. A candidate can modify its own
tests and workflows. A green check is not sufficient authorization to merge.
The initial pilot's authority boundary is mandatory human review enforced by
repository settings, including approval of validation changes.

For stronger nonmodifiable checks, run a separately protected validator or an
organization-required workflow that pins the approved policy revision. Its
controller must not execute candidate code or consume candidate-controlled
success markers as authoritative results. Candidate execution belongs in
disposable workers without controller/release credentials.

## Required maintainer configuration before enrollment

After review and publication:

1. Enable GitHub Actions with the default token permission set to read-only.
   Do not allow fork workflows write tokens or repository secrets.
2. Require approval for all outside contributors' workflow runs.
3. Protect `main` with a ruleset or branch protection that requires pull requests,
   at least one approving review, and code-owner approval. The CODEOWNERS file
   assigns every path to `@QuazarGuy`; GitHub evaluates the base branch's
   ownership rules.
4. Dismiss stale approvals when new commits are pushed, require review
   conversations to be resolved, and require the branch to be current before
   merge. Block force pushes and deletion. Disable bypasses, including
   administrator bypass, where the repository's settings support it.
5. After their first successful runs, require these exact status checks from
   the GitHub Actions integration:
   - `Core / ubuntu-24.04`
   - `Core / windows-2025`
   - `Windows build and rendering`
6. Confirm a fresh contributor cannot approve their own PR, merge without the
   owner, or bypass a missing/failed required check. If another maintainer must
   approve owner-authored PRs, add that trusted person to CODEOWNERS first.
7. Test a harmless change that removes a check: it must still need owner
   approval. Test stale approvals and an intentionally failing test. Do not
   claim protection is enabled merely because CODEOWNERS exists.

If these enforcement features are unavailable for the repository or a gate is
not verified, keep volunteer dispatch disabled and resolve that limitation.

## Frozen policy and scope review

For each submission, inspect the diff against the task's pinned base and exact
allowed paths before executing it. Reject unauthorized edits, binary payloads,
symlinks, submodules, and path collisions.

Treat `.github/`, this document, the contracts, SDK/package/project files,
required baseline tests, and future validation/release tooling as protected.
New tests may be part of an approved task; existing required checks may not
be weakened within an ordinary worker task. Approval must concern the actual
patch and current commit, not a worker-supplied description.

Do not mistake standard repository CI for the future independently protected
validation service. No worker enrollment is authorized by this scaffold.

## Local checks

On Windows, from the repository root:

```powershell
dotnet build Graphy3D.slnx --configuration Release --warnaserror
dotnet test Graphy3D.slnx --configuration Release --no-build --no-restore
```

On Linux, build and test only the core test project. The WPF projects require
Windows. Rendering test results report mesh-build time and count pixels with
the expected surface color; merely opening a window is not the rendering test.

The rendering spike tests enforce a 2-second ceiling for creating a fixed
10,201-vertex, 20,000-triangle mesh. They also check finite coordinates, unit
normals, valid indices, consistent winding, camera limits/reset, and that
surface-colored pixels occupy more than 1% of the test viewport.

This is a feasibility budget, not an FPS, GPU-throughput, UI latency, memory
growth, or four-surface performance guarantee. Those require later measured
tests on representative hardware. Offscreen software-capable rendering in
hosted CI does not establish hardware-accelerated interactive performance.

### Initial local baseline

Measured on November 4, 2026 using .NET SDK 10.0.403, runtime 10.0.14,
Windows build 26300, an AMD EPYC 7763 virtualized host, and Hyper-V/Remote
Display graphics adapters:

- Release solution build: zero warnings and errors.
- Tests: 3 core cases and 4 Windows rendering/input cases passed.
- Fixed mesh creation: 3.5 ms in the recorded in-process run. This is not a
  cold-start measurement.
- Surface-colored pixels: 137,386 of 371,184 viewport pixels, about 37%.
- Camera helper tests cover orbit, pan, extreme zoom, pole avoidance, and reset.
  Routed-event tests additionally verify keyboard, wheel, and reset-button
  wiring. Physical pointer dragging has not been manually verified.

Linux execution and GitHub-hosted jobs remain unverified locally: no working
Linux Docker engine or general-purpose WSL distribution was available.
This virtualized baseline does not certify performance on consumer GPUs.
