# ADR-0002: Test Project Framework Targeting and xUnit v3

**Date:** 2026-05-09
**Status:** Accepted

---

## Context

Test projects cannot target `netstandard2.0` as it is not an executable runtime. xUnit v3 requires executable output type.

## Decision

Test projecta target `net8.0`, `net9.0`, `net10.0` only. `<OutputType>Exe</OutputType>` set explicitly in each test `.csproj` via `Directory.Build.props`.

## Reasoning

- Adding `net48` as a target introduced xUnit version constraints and CI complexity that outweighed the guardrail value at this stage
- `EnableApiCompatibilityCheck` and the pre-commit hook provide sufficient `netstandard2.0` coverage without requiring .NET Framework test targets

## Consequences

- Behavioral regressions specific to .NET Framework runtime will not be caught automatically; this is an accepted tradeoff revisable if a concrete compatibility issue emerges

## References

