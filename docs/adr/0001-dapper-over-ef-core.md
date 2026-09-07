# ADR 0001 - Dapper for data access, not EF Core

Status: accepted

## Context

The application currently talks to SQL Server through a static `DatabaseHelper` that
returns `DataTable`. Phase 1 replaces it with a repository layer. The two realistic
options are Dapper and EF Core.

## Decision

Use Dapper.

## Rationale

The existing queries are already hand-written T-SQL against a single table, and Phase 5
needs server-side paging (`OFFSET/FETCH`), server-side filtering and `ORDER BY NEWID()`
sampling. Dapper keeps those statements as they are and only replaces the `DataTable`
plumbing with typed mapping, so the migration is mechanical and each query stays visible
and reviewable.

EF Core would mean modelling the schema twice - once in migrations, once in the entity
configuration - before a single existing screen worked again, and its LINQ translation
would hide exactly the SQL that Phase 3 and Phase 5 are supposed to tune. Its change
tracker and lazy loading buy nothing here: this is a single-user desktop app doing short,
explicit reads and writes.

## Consequences

- Every query is written by hand and must be parameterised. Building SQL by concatenating
  or interpolating strings is not allowed anywhere in the data layer.
- No automatic migrations. Schema changes go through DbUp scripts (Phase 3).
- Repository integration tests run against a real SQL Server container rather than an
  in-memory provider, which is more faithful and slower.
- If the schema later grows many related tables, the hand-written joins become the cost of
  this decision. Revisit if the entity count passes roughly ten.
