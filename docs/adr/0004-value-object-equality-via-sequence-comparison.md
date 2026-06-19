# ADR-0004: Value Object Equality via Sequence Comparison

**Date:** 2026-06-19
**Status:** Accepted

---

## Context

`Component`, `Repetition`, and `Field` are declared as C# `record` types,
which generate value-based equality automatically. However, this
generated equality is only correct for scalar members. Each of these
three types has a collection-typed member as its primary content:

- `Component.Subcomponents` — `IReadOnlyList<Subcomponent>`
- `Repetition.Components` — `IReadOnlyList<Component>`
- `Field.Repetitions` — `IReadOnlyList<Repetition>`

`IReadOnlyList<T>` does not implement value-based (sequence) equality by
default. Two separately constructed lists containing identical elements
in the same order are not equal to one another unless equality is
implemented explicitly. As a result, the compiler-generated `Equals` for
these record types compares the underlying list references, not their
contents — meaning two structurally identical `Component` instances
(for example, both parsed from the input `"Smith&Smith"`) are incorrectly
reported as unequal.

This was discovered via a failing test
(`Create_PreservesDuplicateComponents_WhenValueContainsDuplicates`) that
asserted two duplicate, separately-created `Component` instances were
equal. `Subcomponent` was unaffected by this issue because its only
member is a `string`, which has correct value equality natively.

## Decision

`Component`, `Repetition`, and `Field` each override `Equals` and
`GetHashCode` explicitly, implementing sequence-based equality:

```csharp
public bool Equals(Component? other) =>
    other is not null && Subcomponents.SequenceEqual(other.Subcomponents);

public override int GetHashCode()
{
    unchecked
    {
        var hash = 17;
        foreach (var subcomponent in Subcomponents)
        {
            hash = (hash * 31) + (subcomponent?.GetHashCode() ?? 0);
        }

        return hash;
    }
}
```

`System.HashCode` (the modern BCL hash-combination type) was considered
but rejected, as it is not available in `netstandard2.0` and would
violate the project's multi-targeting guardrails. The manual hash
combination pattern (seed `17`, multiplier `31`) is used instead, which
is compatible with all four target frameworks.

This logic is duplicated independently in each of the three types rather
than extracted into a shared base type or shared inheritance hierarchy.
A static, stateless helper method was considered as a middle ground but
deferred — see Consequences below.

## Reasoning

**Correctness over convenience.** Default record equality silently
produces incorrect results for any type with a collection-typed member.
This is a sharp edge in the language that is easy to miss, and value
objects are specifically expected to support correct equality — it is
core to what makes them value objects rather than entities.

**Why not a shared base type.** Every domain type in this hierarchy
(`Subcomponent`, `Component`, `Repetition`, `Field`) is deliberately
`sealed`, with construction restricted to a private constructor and a
public static `Create` factory method. This design exists specifically
to protect invariants — no domain type can be placed into an invalid
state because there is no construction path that bypasses validation.
Introducing a shared abstract base type for equality logic would require
removing `sealed` from each derived type, weakening that guarantee for a
benefit (shared equality code) that is unrelated to invariant protection.

**Why not a shared static helper (for now).** A static, generic helper
method (for example, `CollectionEquality.SequenceEqual<T>(...)`) would
share the comparison logic without introducing inheritance, and remains
a strong candidate for a future refactor. It was not implemented
immediately because the priority at the time of this decision was
unblocking `Repetition` and `Field`, which were both mid-implementation
when the equality gap was discovered. Duplicating a small, well-understood
pattern three times was judged less risky in the moment than introducing
a new shared abstraction under time pressure.

**Alternative considered — accept reference equality.** Rejected outright.
Value objects with reference-only equality violate the core definition of
a value object established earlier in the project (see project
discussion on Entities vs. Value Objects) and would produce confusing,
incorrect behavior for any consumer comparing parsed HL7 data.

## Consequences

**Positive:**
- `Component`, `Repetition`, and `Field` now have correct, predictable
  value equality consistent with their identity as DDD value objects.
- The fix is compatible with all four target frameworks, including
  `netstandard2.0`.
- Equality and hashing are tested directly via duplicate-preservation
  tests at each level.

**Negative:**
- The same ~12 lines of equality/hashing logic are duplicated identically
  across three files. Any future change to the hashing algorithm (for
  example, adopting `System.HashCode` once `netstandard2.0` support is
  eventually dropped) requires updating three locations instead of one.
- This duplication is flagged in-line in each affected file with a
  standard comment marking it as a deliberate, deferred refactor
  candidate.

**Revisit when:** extracting a shared static helper
(`CollectionEquality` or similar) becomes low-risk to implement — for
example, once `Segment` and `Message` are implemented and the same
pattern would otherwise be duplicated two more times, increasing the
maintenance cost of continued duplication beyond what is reasonable.

## References

- `Component.cs`, `Repetition.cs`, `Field.cs` — duplicated equality
  implementations, each marked with a deferred-refactor comment
- Test cases: `Create_PreservesDuplicateSubcomponents_WhenValueContainsDuplicates`,
  `Create_PreservesDuplicateComponents_WhenValueContainsDuplicates`, and
  equivalent tests in `RepetitionTests.cs` and `FieldTests.cs`
