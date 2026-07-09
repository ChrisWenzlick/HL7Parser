# Architecture Decision Records

This directory contains Architecture Decision Records (ADRs) for the HL7Parser project.

---

## What Is an ADR?

An Architecture Decision Record documents a significant architectural or design decision made during the development of this project. Each record captures:

- The **context** — what situation or problem prompted the decision
- The **decision** — what was decided
- The **reasoning** — why this option was chosen over alternatives
- The **consequences** — what becomes easier, harder, or different as a result

ADRs are not meeting notes or changelogs. They are permanent records of *why* the project is structured the way it is. They are written at the time a decision is made and are not retroactively updated to reflect a better understanding — instead, a new ADR is written to supersede an old one if a decision changes.

---

## Why ADRs Matter

Codebases accumulate decisions invisibly. Six months from now — or for any new contributor — it is rarely obvious why a particular structure, pattern, or constraint exists. ADRs make that reasoning explicit and permanent, which:

- Prevents relitigating settled decisions
- Helps new contributors understand the project's design philosophy quickly
- Creates a historical record of how the project evolved and why
- Demonstrates engineering maturity to anyone reviewing the repository

---

## ADR Status Values

Each ADR carries a status indicating its current standing:

| Status | Meaning |
|---|---|
| **Proposed** | Under consideration, not yet decided |
| **Accepted** | The decision has been made and is in effect |
| **Deprecated** | The decision was once accepted but is no longer current |
| **Superseded** | Replaced by a newer ADR (which is referenced) |

---

## File Naming Convention

ADRs are named sequentially with a short descriptive title:

```
0001-multi-target-framework-strategy.md
0002-clean-architecture-layer-structure.md
0003-netstandard2-as-compatibility-floor.md
```

The number is permanent and never reused, even if an ADR is superseded.

---

## ADR Template

When creating a new ADR, copy the following template:

```markdown
# ADR-XXXX: [Short Title]

**Date:** YYYY-MM-DD
**Status:** Proposed | Accepted | Deprecated | Superseded by [ADR-XXXX]

---

## Context

What situation, constraint, or problem prompted this decision?
What forces are at play? What is the current state of things?
Write this in the present tense as it was at the time of the decision.

## Decision

What was decided? State it clearly and directly.
Avoid justification here — that belongs in the Reasoning section.

## Reasoning

Why was this option chosen?
What alternatives were considered and why were they not chosen?
What tradeoffs were accepted?

## Consequences

What becomes easier as a result of this decision?
What becomes harder?
What new constraints or obligations does this decision introduce?
Are there any risks?

## References

- Links to relevant documentation, issues, or discussions
```

---

## Index of Records

| ADR | Title | Status |
|---|---|---|
| 0001 | Multi-Target Framework Strategy | Accepted |
| 0002 | Test Project Framework Targeting and xUnit v3 | Accepted |
| 0003 | Validation Delegation Strategy | Accepted |
| 0004 | Value Object Equality via Sequence Comparison | Accepted |
| 0005 | MshSegment as a Distinct Type | Accepted |

---

## Further Reading

- [Documenting Architecture Decisions — Michael Nygard (original ADR proposal)](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions)
- [ADR GitHub organization and tooling](https://adr.github.io)
- [Architectural Decision Records in practice — ThoughtWorks Technology Radar](https://www.thoughtworks.com/radar/techniques/lightweight-architecture-decision-records)
