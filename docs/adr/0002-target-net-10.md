# ADR 0002 - Target .NET 10, not .NET 8

Status: accepted

## Context

The project was a .NET Framework 4.7.2 application in the legacy (non-SDK) csproj format.
The modernisation brief called for .NET 8.

## Decision

Target `net10.0-windows` for the WinForms project and `net10.0` for the other layers.

## Rationale

.NET 8's support window ends in November 2026, which is two months from the date of this
decision; a project being modernised for maintainability should not start on a runtime
that is about to leave support. .NET 10 is the current LTS. The brief named .NET 8 before
that timing mattered, and nothing in the brief depends on the version number.

Splitting the target frameworks matters for CI: only the UI needs Windows, so Domain,
Data and Services can run their tests on a Linux agent, which is also where the SQL Server
test container runs.

## Consequences

- `System.Data.SqlClient` is not part of the .NET 10 BCL and is no longer maintained;
  `Microsoft.Data.SqlClient` replaces it. That provider defaults to `Encrypt=True`, so
  local connection strings need `TrustServerCertificate=True`.
- WinForms on .NET does not default to DPI-unaware the way .NET Framework 4.7.2 did. The
  forms are fixed-size and pixel-aligned, so `ApplicationHighDpiMode` is pinned to
  `DpiUnaware` to keep the layout identical. The cost is blurry rendering on scaled
  displays; moving to `PerMonitorV2` is deferred and tracked separately.
- `BinaryFormatter` was removed in .NET 9. The form `.resx` files were checked and store
  their images through a type converter, not `BinaryFormatter`, so they still load.
