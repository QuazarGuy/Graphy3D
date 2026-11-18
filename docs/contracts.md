# Pilot contracts

These are the initial requirements for maintainer-approved tasks, not a claim
that the corresponding features already exist. Changes require explicit
maintainer approval. The rendering spike is not the expression engine or
general-purpose sampler.

## Mathematical coordinates and geometry

- Use double-precision Cartesian coordinates: X and Y span the domain; Z is
  the dependent value. Angles are in radians. Z is the renderer's up axis.
- Core types must not reference WPF or another platform-specific UI library.
  The existing `Graphy3D.Core.Point3D` stores coordinates without validating
  them. Sampling and geometry producers are responsible for finite values.
- A grid is row-major: row increases Y and column increases X. Include both
  endpoints on each axis. A single resolution N means N samples per axis.
- For a fully valid N-by-N grid, produce N*N vertices and
  2*(N-1)*(N-1) triangles. Triangle indices must be in range.
- Positive-Z-facing winding for a cell is `(a,b,c)` and `(b,d,c)`, where
  b is the next X sample, c is the next Y sample, and d is diagonal.
- Preserve an explicit validity mask while sampling. Do not substitute zero
  for invalid values. Omit triangles touching invalid samples. Downstream
  geometry contains only finite coordinates; compact and remap invalid
  vertices before rendering.
- Normals are unit vectors where geometry has a well-defined normal.
  Degenerate faces are omitted; empty geometry is a valid, explicit result.
- Never infer that a finite grid detects every discontinuity. The first
  release does not promise complete asymptote detection.

## Expression language

Input is the expression only, for example `sin(x) * cos(y)`, not `z = ...`.
Whitespace is ignored. Names are case-sensitive.

```text
expression = sum
sum        = product { ("+" | "-") product }
product    = unary { ("*" | "/") unary }
unary      = ("+" | "-") unary | power
power      = primary [ "^" unary ]
primary    = number | "x" | "y" | "pi" | "e"
           | "(" expression ")"
           | function "(" expression ")"
function   = "sin" | "cos" | "tan" | "sqrt" | "abs" | "exp" | "log"
number     = (digits [ "." [ digits ] ] | "." digits)
             [ ("e" | "E") [ "+" | "-" ] digits ]
digits     = digit { digit }
digit      = "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9"
```

- Exponentiation is right-associative and binds more tightly than unary sign:
  `-2^2 = -4`, `2^3^2 = 512`, `2^-2 = 0.25`.
- Multiplication must be explicit: reject `2x` and `2(x+y)`.
- `log` is the natural logarithm. Parse numbers using invariant culture.
- There are no assignments, strings, property access, imports, file access,
  reflection, executable scripts, or user-defined functions.
- Use an allowlisted parser and evaluator, not dynamic C# compilation,
  a general-purpose scripting engine, or shell execution.
- Initial finite-result tolerance: absolute error <= 1e-9 for approved
  fixtures whose expected magnitude is <= 1e6. Larger magnitudes require
  a fixture-specific tolerance.

### Errors and numeric invalidity

Compilation returns either a compiled expression or a structured error with
code, message, zero-based UTF-16 start offset, and length. Expected error codes:
`EmptyExpression`, `InvalidToken`, `UnexpectedToken`, `UnknownIdentifier`,
`MissingParenthesis`, and `ComplexityLimit`.

An evaluation result distinguishes a finite value from `DivisionByZero`,
`DomainError`, and `NonFiniteResult`. Division by exactly zero and zero raised
to a negative power are invalid. Define `0^0 = 1`, following .NET `Math.Pow`.
Negative bases with nonintegral exponents are domain errors.

Do not silently clamp values into the plotting domain or convert invalid
results to zero. A valid expression with no valid samples produces an explicit
empty graph and user-visible explanation, not a parser error.

Cancellation is distinct from mathematical invalidity and must propagate.
Unexpected internal exceptions must be reported, not converted into ordinary
invalid samples.

## Limits

These are initial enforceable limits, subject to explicit reviewed revision:

| Resource | Limit |
|---|---|
| Expression length | 4,096 UTF-16 code units |
| Tokens | 1,024 |
| Recursive parser/evaluator depth | 64, counting unary and power nesting too |
| Surfaces | 4 |
| Samples per axis | Integer from 3 through 201; default 101 |
| Domain bounds | Finite; each in [-1e6, 1e6]; minimum strictly less than maximum |
| Configuration input | 1 MiB maximum, JSON depth 16 |
| Contributor context | 256 KiB UTF-8 total for initial pilot text tasks |
| Contributor patch | 256 KiB UTF-8 maximum; text files only |
| Attempt duration | 20 minutes maximum, coordinator-controlled lease |
| Retry policy | At most 3 automatic attempts per task; then maintainer attention |

Check limits before expensive allocation or execution. Sampling must accept a
cancellation token and check it at least once per row. These application limits
are not a replacement for validator OS-level isolation or job timeouts.

## Graph configuration version 1

Use UTF-8 JSON with the following shape:

```json
{
  "version": 1,
  "surfaces": [
    {
      "expression": "sin(x) * cos(y)",
      "xMin": -5,
      "xMax": 5,
      "yMin": -5,
      "yMax": 5,
      "samplesPerAxis": 101,
      "color": "#2563EB",
      "visible": true
    }
  ],
  "showAxes": true,
  "showGrid": true
}
```

Zero through four surfaces are allowed. All fields are required; reject unknown
or duplicate properties, invalid types, unsupported versions, malformed colors,
and expressions/domains exceeding the above limits. Colors use `#RRGGBB`.
Do not accept executable type metadata, external references, or file paths.
Camera persistence is deferred; loading resets the camera.

Load and validate into temporary state before replacing the current graph.
Malformed files must not partially modify an open configuration. Saving must
not truncate a previously valid file if serialization or replacement fails.

## Volunteer task and attempt envelope

A task must contain:

- Stable task ID and contract version.
- Approved objective, non-goals, and acceptance criteria.
- Full base commit SHA, including all accepted prerequisites.
- Prerequisite task IDs and accepted artifact hashes.
- Exact allowed repository-relative paths, not unrestricted glob patterns.
- Required model capabilities, bounded context files, and resource budgets.
- Maintainer-approved validation commands and review requirements.

An attempt has a separate ID, contributor identity, lease expiry, and task ID.
Submission contains a UTF-8 unified text patch, summary, model name/digest,
base SHA, and an honest declaration that local execution was not performed.
The coordinator computes artifact hashes itself; it must not trust submitted
hashes or test logs.

Reject paths outside the task, path traversal, absolute paths, binary patches,
symlinks, submodules, and case-insensitive path collisions for Windows.
For ordinary worker tasks, also prohibit changes to dependency/project files,
CI, validation policy, release machinery, and SDK configuration.
Such changes require a separate maintainer-led task.

Attempts may execute at least once. Duplicate and expired results remain in
history, but only one accepted artifact per task revision may be integrated.
Every required dependency must be accepted before dispatch; never silently
rebase a pending patch or reuse stale context.

Workers have no shell/project-execution tools or arbitrary host filesystem
tools. Repository content and model responses are data, not instructions
granting permission. The coordinator, validator, and release authority remain
separate. No automatic merge or release is authorized.
