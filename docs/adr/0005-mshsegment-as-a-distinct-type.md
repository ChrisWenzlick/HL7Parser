# ADR-0005: MshSegment as a Distinct Type

## Status
Accepted

## Context
MSH is structurally unique among HL7 v2 segments:
- MSH-1 is the field separator character itself, not a parseable field value
- MSH-2 contains delimiter characters that would be incorrectly parsed by standard field splitting logic
- MSH is the canonical source of `EncodingCharacters` for the entire message
- All other segments depend on MSH having been parsed first

An initial approach attempted to handle MSH as a special case within `Segment` via internal branching on the `"MSH"` identifier. This caused accumulating branching logic and a chicken-and-egg dependency where `EncodingCharacters` was required as a construction parameter before it could be derived from MSH itself.

## Decision
`MshSegment` is implemented as a distinct type separate from `Segment`, implementing the shared `ISegment` interface. `MshSegment.Create` takes only the raw segment string, derives `EncodingCharacters` internally from MSH-1 and MSH-2, and exposes them as a public property for `Message` to use when parsing subsequent segments.

`Message` holds `IReadOnlyList<ISegment>` for uniform segment access and exposes a typed `Msh` convenience property that casts `Segments[0]` to `MshSegment`. This cast is guaranteed safe since `Message.Create` enforces MSH as the first segment.

## Consequences
- `Segment` remains uniform and free of MSH-specific branching
- `EncodingCharacters` flows naturally from `MshSegment` to `Message` to `Segment` without circular dependencies
- `ISegment` provides a shared contract enabling uniform storage and traversal
- `MshSegment` is the single authoritative source of `EncodingCharacters` within a message
- The `Msh` convenience cast on `Message` is safe by construction but would throw if the invariant were ever violated — acceptable given `Message.Create` enforces it