# Contributing

Graphy3D is currently establishing its trusted foundation. Volunteer task
dispatch is not enabled yet.

## Before starting a task

Obtain maintainer approval for the objective, base commit, permitted files,
dependencies, and acceptance checks. Keep patches scoped to that agreement.
Report missing requirements or necessary scope changes instead of silently
expanding the task.

## Submission requirements

- Describe the behavior changed and checks performed.
- Distinguish checks actually run from checks proposed or unavailable.
- Include regression tests for changed core behavior where applicable.
- Do not add credentials, executable payloads, or unrelated dependencies.
- Changes to dependencies, CI, security policy, or release machinery require
  explicit maintainer approval.
- Never weaken required checks to make the same submission pass.

An inference-only worker returns a patch proposal without executing repository
code, generated commands, builds, or tests locally. It must not claim those
checks passed. A separate isolated validator must evaluate the patch.

## Review and release

All contributions require maintainer review. Contributor-provided logs and AI
reviews are supporting information, not authoritative validation. Acceptance
does not authorize a contributor to merge, publish, or access release secrets.

Follow the [pilot contracts](docs/contracts.md). The repository includes CI
checks but does not yet implement the independently protected validator or
automated task dispatch. Repository enforcement must be configured and verified
as described in [validation setup](docs/validation.md). A successful local build
or green CI check alone is not a security approval.

## License

Contributions must be compatible with the project's [MIT license](LICENSE).
Do not submit material you do not have permission to contribute.
