# ADR-0003: Validation Delegation Strategy

**Date:** 2026-06-19
**Status:** Accepted

---

## Context

HL7Parser's domain model is a recursive hierarchy of value objects, each
responsible for parsing a raw string into structured child objects:

```
Field
└── Repetition[]
    └── Component[]
        └── Subcomponent[]
```

Each level splits its raw input on a structural delimiter character and
delegates creation of its children to the level below:

| Level | Splits on | Delegates to |
|---|---|---|
| `Field` | `~` | `Repetition.Create` |
| `Repetition` | `^` | `Component.Create` |
| `Component` | `&` | `Subcomponent.Create` |
| `Subcomponent` | — (leaf node) | — |

`Subcomponent`, as the leaf node, is the only type that performs explicit
character-level validation, rejecting any of the five HL7 delimiter
characters (`|`, `^`, `~`, `\`, `&`) appearing in raw subcomponent text.

The question this ADR resolves: should `Component`, `Repetition`, and
`Field` each independently re-validate against delimiter characters that
are structurally meaningful at a level above them (for example, should
`Field` explicitly reject `|`, since `|` will become the field separator
once `Segment` is implemented), or should every level above `Subcomponent`
rely entirely on delegation to catch such violations, with no exceptions?

An earlier version of this decision introduced a single exception: `Field`
explicitly validated against `|`, on the reasoning that `|` is meaningful
one level above `Field` (at the future `Segment` level) and deserved
immediate rejection rather than being caught several layers down at
`Subcomponent`. On review, this exception was found to be inconsistent
with the reasoning already applied to `Repetition` and `Component`, both
of which rely entirely on delegation with no equivalent special-cased
check for the delimiter one level above them (`~` for `Repetition`, `^`
for `Component`). The exception existed for incidental historical
reasons — `Field` was implemented before `Repetition` existed, at a time
when `|` had no level below it to delegate to — rather than as a
deliberate, generalizable design choice. This revision removes the
exception and applies a single, unqualified rule across all levels.

## Decision

No level above `Subcomponent` performs explicit delimiter character
validation. This applies uniformly to `Component`, `Repetition`, and
`Field`, with no exceptions.

Each of these types splits its raw value on its own structural delimiter
and delegates creation of each resulting piece to the level below. Any
invalid character — including characters that are structurally meaningful
one or more levels above the current type, such as `|` relative to
`Field` — is caught when validation reaches `Subcomponent`, and the
resulting failure is propagated back up through each enclosing layer with
added context (the index of the child that failed and the original error
message).

## Reasoning

**Single source of truth.** Delimiter validation rules are defined once,
in `Subcomponent.Create`. If the rule ever needs to change — for example,
to support custom encoding characters defined in a message's own MSH
segment rather than hardcoded defaults — there is exactly one place to
update.

**Less code to maintain.** Re-implementing character checks at every
level, or even at a subset of levels, increases the surface area for
inconsistency. The previous `Field`-only exception for `|` was itself an
example of this risk: it was correct in isolation but inconsistent with
the rest of the hierarchy, and was only caught on later review.

**Consistent error propagation.** Because every level relies on the same
underlying validation with no special cases, error messages follow a
single, predictable pattern as they bubble up: `"Failed to create
{ChildType} at index {i}: {childError}"`. A failure at any depth produces
a chain of context that is uniform in structure and depth-proportional to
where in the hierarchy the input was created — not inconsistently flat
for one specific character and nested for all others.

**Alternative considered — explicit validation at every level for the
delimiter one level up.** This would mean `Component` explicitly rejects
`^`, `Repetition` explicitly rejects `~`, and `Field` explicitly rejects
`|` — making every level symmetric with the original `Field`-only
exception, rather than removing that exception. This was rejected because
it reintroduces the duplication this ADR is specifically intended to
avoid, for a benefit (slightly flatter, more immediate error messages at
every level) that does not outweigh the cost of three additional,
near-identical validation blocks.

**Alternative considered — explicit validation only where convenient.**
This describes the rejected prior state of the decision and is called out
explicitly here as a cautionary case: an exception introduced for
incidental reasons (implementation order) rather than principled ones is
a maintenance liability, since it is easy to mistake for a deliberate
design choice on later review — as in fact happened here.

## Consequences

**Positive:**
- Adding or changing delimiter validation rules requires touching only
  `Subcomponent`.
- Error messages are consistent across the entire hierarchy by
  construction, not by convention, with no special-cased exceptions to
  remember or document separately.
- Less code overall, with less risk of one level's validation logic
  drifting out of sync with another's.
- The hierarchy is now symmetric: the same rule applies identically at
  every level, which makes the codebase easier to reason about and easier
  to extend when `Segment` is added.

**Negative:**
- A caller's error message reports the failure relative to where it was
  ultimately caught (`Subcomponent`), with context added by each
  enclosing layer. The full path requires reading the complete error
  chain rather than a single flat error code. This is now true uniformly
  for all characters at all levels, including `|` at the `Field` level,
  where a flatter message was previously available.
- Performance is not a current concern, but worth noting: a failure deep
  in a large field still requires the full delegation chain to execute
  before the failure is detected, since there is no short-circuiting
  pre-validation at outer levels.

**Revisit if:** a future requirement demands validation against
message-specific encoding characters (rather than hardcoded HL7 defaults)
at a level above `Subcomponent`, which would require passing
`EncodingCharacters` down through the hierarchy — a design not yet
implemented as of this decision. Also revisit if error message flatness
(rather than code deduplication) becomes a demonstrated priority for
library consumers, in which case the rejected "validate the delimiter one
level up" alternative should be reconsidered as a deliberate, symmetric
choice rather than reintroduced as an isolated exception.

## References

- `Subcomponent.cs` — source of all delimiter character validation
- `Component.cs`, `Repetition.cs`, `Field.cs` — delegation and error
  propagation implementations, with no level-specific character
  validation
- Superseded reasoning: original `Field`-level `|` check, removed for
  consistency with `Repetition` and `Component`
