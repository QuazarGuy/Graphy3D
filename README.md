# Graphy3D

A Windows 3D graphing calculator and pilot workload for a volunteer AI-compute
application.

## Status

This is the initial trusted foundation, not a working calculator. It contains a
Windows WPF rendering spike, a platform-independent core library, core tests,
and Windows rendering tests. Expression parsing, general-purpose graph
sampling, and project persistence are not implemented yet.

## Prerequisites

- A stable .NET 10 SDK. `global.json` permits stable .NET 10 feature-band updates.
- Windows to build and run the WPF application.
- Windows or Linux to build and test the core library.

## Build and run on Windows

From the repository root:

```powershell
dotnet build Graphy3D.slnx
dotnet run --project src\Graphy3D.App\Graphy3D.App.csproj
```

The application displays a fixed saddle surface with orbit, pan, zoom, and reset
controls. Left-drag to orbit, right-drag to pan, use the mouse wheel to zoom,
or use arrow keys, +/-, and R. No model server, network
service, or administrator access is required to run it after dependencies
are restored.

## Test the core

On Windows:

```powershell
dotnet test tests\Graphy3D.Core.Tests\Graphy3D.Core.Tests.csproj
```

On Linux:

```sh
dotnet test tests/Graphy3D.Core.Tests/Graphy3D.Core.Tests.csproj
```

On Linux, target the core test project rather than the Windows application or
the whole solution. Package restore requires access to the configured NuGet
package sources.

## Project layout

- `src/Graphy3D.App`: Windows-only UI and, eventually, rendering.
- `src/Graphy3D.Core`: platform-independent mathematics and geometry.
- `tests/Graphy3D.Core.Tests`: core tests, runnable on Windows and Linux.
- `tests/Graphy3D.App.Tests`: Windows-only mesh, camera, and rendered-pixel tests.

To run all tests on Windows:

```powershell
dotnet test Graphy3D.slnx --configuration Release
```

See the [pilot contracts](docs/contracts.md) for the approved initial grammar,
geometry, configuration, limits, and task envelope. See
[validation setup](docs/validation.md) for checks and required GitHub settings.

## Contribution boundary

The pilot will use maintainer-approved tasks and patch-only submissions.
Contributors must not execute project code on inference-only worker machines.
Validation must run separately in an isolated environment, with human approval
before merge or release.

CI workflow files and code ownership rules are present, but remote execution
and enforcement require publication and repository configuration. A separate
protected validator is not implemented. Do not enroll volunteer workers until
the required gates are configured and verified.
See [contribution guidance](CONTRIBUTING.md).

## License

[MIT](LICENSE).
