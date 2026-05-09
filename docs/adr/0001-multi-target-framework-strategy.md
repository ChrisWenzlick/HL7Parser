# ADR-0001: Multi-Target Framework Strategy

**Date:** 2026-05-09
**Status:** Accepted

---

## Context

We're building a library for the .NET ecosystem to facilitate HL7 v2 message parsing and validation. A significant portion of healthcare organizations still run .NET Framework, so we want to support that.

## Decision

Target `netstandard2.0`, `net8.0`, `net9.0`, `net10.0`

## Reasoning

- `netstandard2.0` provides .NET Framework 4.6.1+ compatibility
- Modern targets ensure access to current language features and performance improvements
- `LangVersion=latest` decouples language version from target framework

## Consequences

- Requires guardrails to prevent accidental use of unsupported APIs
- `csharp_style_prefer_primary_constructors` disabled in `.editorconfig` to avoid C# 12 syntax in shared code

## References

