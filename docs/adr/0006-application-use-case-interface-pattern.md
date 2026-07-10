# ADR-0006: Application Use-Case Interface Pattern

**Date:** 2026-07-10
**Status:** Accepted

---

## Context

`IMessageParser`/`MessageParser` is the first type added to `HL7Parser.Application`. Unlike `HL7Parser.Domain`, this layer had no established construction or typing convention yet: Domain value objects (e.g. `Message`, `Field`) use a private constructor plus a static `Create` factory returning `Result<T>`, because they validate raw HL7 text and can fail. Application-layer use cases are different in kind — they are behaviors invoked by a caller (or a future DI container), not values that are parsed and validated at construction time. This decision fixes the shape all future Application-layer use cases (e.g. the queued `ValidateMessage`/conformance-rules work) should follow.

Two sub-decisions were in scope:

1. Should a use case with zero dependencies (`MessageParser` has none) still get an interface, or is a concrete class sufficient until a second implementation or a dependency exists?
2. Should use-case types be named after the action they perform (verb-first, e.g. `ParseMessage`/`IParseMessage`) or the role they play (noun-first, e.g. `MessageParser`/`IMessageParser`)?

## Decision

1. **Every Application-layer use case gets an interface**, even with no current dependencies and no current second implementation. Pattern: `I{Noun}` interface with a single `Execute(...)` method, plus a `sealed class {Noun}` implementation.
2. **Use-case types are named noun-first, after the role they play** (`MessageParser`, not `ParseMessage`), with the interface following the same noun (`IMessageParser`, not `IParseMessage`).
3. Application-layer use-case classes are constructed via an ordinary public constructor (implicit, when there are no dependencies to inject) — not the Domain layer's private-constructor-plus-static-factory pattern. There is no validation to fail at construction time, so there is nothing for a `Result<T>`-returning factory to protect against.

## Reasoning

**Interface per use case (ISP + DIP):** establishes a stable, mockable seam at the Application boundary from the start, rather than retrofitting one once a second caller or a test double is needed. Interface Segregation keeps each use case's contract to exactly the one method it needs; Dependency Inversion means callers (and, later, any DI registration — explicitly out of scope for this repo per its library-only usage) depend on the abstraction, not the concrete parser. The cost of an interface with one implementation is low (one small file) and consistent with this repo's existing bias toward documenting and standardizing patterns early (see ADR-0001 through ADR-0005).

**Noun-first naming, chosen over verb-first:** a type named after its role (`MessageParser`) reads naturally at both the interface and call-site level — `IMessageParser parser`, `new MessageParser()` — and matches conventional .NET naming for single-responsibility service types (`*Parser`, `*Validator`, `*Repository`). Verb-first naming (`ParseMessage`) reads more naturally only at the call site (`parseMessage.Execute(...)` still needs a method name, so the verb is restated) and is less consistent with the broader .NET ecosystem's naming of injectable services.

**Alternative considered — no interface until a second implementation or dependency exists (YAGNI).** Rejected: this is the first Application-layer type, and the goal of this specific change was explicitly to establish the pattern for the layer, not just to implement one method. Deferring the interface would leave the second use case to decide the convention under time pressure instead of by design.

**Alternative considered — Domain-style private constructor + static `Create()` factory.** Rejected for Application-layer use cases: that pattern exists in Domain specifically to guard against constructing an invalid value (raw HL7 text that fails to parse), returning `Result<T>` instead of throwing. A use-case class itself is never "invalid" — it has no invariants to protect at construction — so a factory adds indirection without a corresponding safety benefit. (Note: a *future* use case with real dependencies to inject, or with per-instance configuration, may reconsider this — see Consequences.)

**Alternative considered — verb-first naming (`ParseMessage`/`IParseMessage`), matching the spec as originally written.** This was the initial implementation and is technically valid; it was superseded in this session at the user's explicit request to better reflect the type's functionality. It is documented here as rejected only in the sense that noun-first is now the standing convention — not because verb-first was defective.

## Consequences

**Positive:**
- Every future Application-layer use case has an unambiguous shape to follow: `I{Noun}` + `sealed class {Noun}`, ordinary public construction, single `Execute` method.
- Consumers and tests can depend on `IMessageParser` rather than `MessageParser` from day one, with no later refactor required to introduce the seam.

**Negative / follow-up required:**
- The queued spec `2026-07-09-03-application-validate-message-conformance.md` (and its dependency, `2026-07-09-02-application-validation-types.md`) currently specifies verb-first naming (`ValidateMessage`/`IValidateMessage`) and a private-constructor-plus-static-`Create()`-factory pattern, both of which conflict with this ADR. Those specs were **not** modified by this execution session — spec authoring is planning-phase-only and out of scope for a repo-side session. The planning phase should reconcile the queued spec with this ADR (e.g. rename to `MessageValidator`/`IMessageValidator` and drop the factory in favor of ordinary construction) before that spec is executed.
- If a future use case genuinely needs constructor-time validation (e.g. a use case configured with an injected, user-supplied rule set that must itself be validated), the "ordinary public constructor" part of this decision should be revisited for that specific type rather than assumed to apply universally.

## References

- `src/HL7Parser.Application/UseCases/IMessageParser.cs`, `MessageParser.cs`
- `tests/HL7Parser.Tests.Unit/Application/MessageParserTests.cs`
- [[planning/specs/2026-07-09-01-application-parse-message]] — originating spec (named `IParseMessage`/`ParseMessage`; superseded by this ADR's naming convention)
- [[planning/specs/2026-07-09-02-application-validation-types]], [[planning/specs/2026-07-09-03-application-validate-message-conformance]] — queued specs that need reconciliation with this ADR before execution
- ADR-0001 through ADR-0005 — established precedent of documenting layer-level conventions early
