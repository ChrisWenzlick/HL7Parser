# Changelog

All notable changes to HL7Parser will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Added
- ADR-0007: NuGet Trusted Publishing over long-lived API key — documents the decision to authenticate `publish.yml` to nuget.org via OIDC-issued, short-lived credentials instead of a stored `NUGET_API_KEY` secret

### Changed
- `README.md` rewritten to accurately describe the v1.0 API: corrected Quick Start samples (`MessageParser`/`MessageValidator`/`MessageTransformer`/`FieldCopyRule`, real `GetSegments`/`GetField`/indexer-chaining call shapes), an accurate "Supported Message Types" table sourced from `MessageTypeSegmentRequirementsRule`'s live 14-entry table, a new "Transformation" section describing the pipeline as a minimal foundation (one shipped rule, no segment creation), an updated Roadmap reflecting what's actually shipped, and FHIR R4 mapping / MLLP framing relabeled as planned post-1.0. Every code sample was verified to compile and run against the actual packed `HL7Parser` NuGet package, not just against source
- `publish.yml` migrated from `NUGET_API_KEY`-based long-lived secret authentication to [NuGet Trusted Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing): the job now declares `permissions: id-token: write` (and `contents: write` for release creation), exchanges a GitHub OIDC token for a temporary nuget.org API key via `NuGet/login@v1` immediately before the push step, and identifies the publishing account via a `NUGET_USER` secret (profile username, not an email) rather than storing a push-capable credential. The `NUGET_API_KEY` secret and its early-exit check are removed entirely, not kept as a fallback
- `docs/RELEASING.md` and `CONTRIBUTING.md`'s "Releasing" section updated to describe the Trusted Publishing policy setup and `NUGET_USER` secret instead of the superseded `NUGET_API_KEY` instructions

### Added
- `.github/workflows/publish.yml` — git-tag-driven (`v*`) GitHub Actions workflow that builds and tests (Release, net8.0/net9.0/net10.0) as a safety gate, packs `HL7Parser` with the version derived from the tag, publishes the `.nupkg`/`.snupkg` to nuget.org using a `NUGET_API_KEY` secret, and creates a GitHub Release with notes sourced from the matching `CHANGELOG.md` section; fails clearly (rather than silently skipping) if `NUGET_API_KEY` is unset or the `CHANGELOG.md` section for the tagged version is missing; supports a `workflow_dispatch` dry run (build/test/pack only, no publish) for validating the pipeline without a real tag
- `docs/RELEASING.md` — documents the `NUGET_API_KEY` secret requirement and the release runbook; linked from `CONTRIBUTING.md`
- NuGet packaging configuration: `HL7Parser.Application` is the single packable unit (`PackageId=HL7Parser`), bundling `HL7Parser.Domain` assemblies for all target frameworks into one package; `HL7Parser.Domain` is set `IsPackable=false`; shared metadata (`Authors`, `Description`, `PackageLicenseExpression=MIT`, `PackageProjectUrl`, `RepositoryUrl`, `PackageTags`, `PackageReadmeFile`, `IncludeSymbols`, `SymbolPackageFormat=snupkg`, dev-default `Version=0.0.1-dev`) added to `Directory.Build.props`
- Initial solution structure with Domain, Application, Infrastructure, Tests.Unit, and Tests.Integration projects
- Multi-target support for `netstandard2.0`, `net8.0`, `net9.0`, and `net10.0`
- README, LICENSE, CONTRIBUTING, and CHANGELOG documentation
- Architecture Decision Records in `/docs/adr`
- GitHub Actions CI/CD pipeline skeleton
- EditorConfig and StyleCop analyzer configuration
- `Result<T>` with factory methods `Success` and `Failure`, invariant-protected `Value` and `Error` properties
- `Subcomponent` value object with HL7 delimiter validation via `Create` factory method
- `Component` value object that parses subcomponent-delimited strings into `Subcomponent` collections with error propagation
- `Repetition` value object parsing component-delimited strings with ordered `Component` collections
- `Field` value object parsing repetition-delimited strings with ordered `Repetition` collections
- `ToHl7String()` round-trip serialization across `Subcomponent`, `Component`, `Repetition`, and `Field`
- Value-based equality (`Equals`/`GetHashCode`) for `Component`, `Repetition`, and `Field` using sequence comparison of child collections
- `IMessageParser` interface and `MessageParser` use case — parses a raw HL7 v2 string via `Execute(string)`, returning `Result<Message>`
- `ValidationSeverity` enum (`Error`, `Warning`, `Info`)
- `ValidationIssue` record with `Severity`, `Location`, `Code`, and `Description` properties
- `ValidationResult` record with `IsValid` (true when no `Error`-severity issues are present) and `Issues` collection, constructed via `ValidationResult.Create`
- `IMessageValidator` interface
- `MessageValidator` — validates that MSH-7, MSH-9, MSH-10, MSH-11, and MSH-12 are present and non-blank, emitting `Error`/`"REQUIRED_FIELD_MISSING"` issues for each missing or whitespace-only field
- `Field`, `Repetition`, and `Component` implement `IReadOnlyList<T>` over their child collections (`Repetitions`, `Components`, `Subcomponents` respectively), enabling indexer chaining (`field[0][0][0].RawValue`), `foreach`, `Count`, and LINQ directly on these types
- `IConformanceRule` interface (`HL7Parser.Application.Validation`) with `Applies(Message)` and `Evaluate(Message)` methods, establishing the extensible conformance-rule abstraction
- `RequiredMshFieldsRule` (`HL7Parser.Application.Validation.Rules`) implementing `IConformanceRule` with the required MSH-7/9/10/11/12 rules
- `OptionalMshFieldsRule` (`HL7Parser.Application.Validation.Rules`) implementing `IConformanceRule` with `Warning`-severity issues for MSH-3/4/5/6 (Sending Application, Sending Facility, Receiving Application, Receiving Facility) when missing or blank
- `Message.GetSegments(string segmentIdentifier)` on `HL7Parser.Domain.Message` — returns all segments matching the given identifier as `IReadOnlyList<ISegment>` (empty list when none match)
- `MessageTypeSegmentRequirementsRule` (`HL7Parser.Application.Validation.Rules`) implementing `IConformanceRule` with the first message-type-conditional required-segment rule: ADT messages require a PID segment
- `MshDateTimeFormatRule` (`HL7Parser.Application.Validation.Rules`) implementing `IConformanceRule` — validates MSH-7 structural format against the HL7 v2 `TS` (Time Stamp) pattern (4, 6, 8, 12, or 14 digit numeric prefix, optional fractional seconds, optional timezone offset); emits `Error`/`"INVALID_FIELD_FORMAT"` when MSH-7 is present and non-blank but structurally malformed; does not fire when MSH-7 is missing or blank (that case is already covered by `RequiredMshFieldsRule`)
- `MessageTypeSegmentRequirementsRule` extended with three additional message-type entries: `ORU → [PID, OBX]`, `ORM → [PID, ORC]`, `MDM → [PID, TXA]`
- `ITransformRule` interface (`HL7Parser.Application.Transformation`) — single `Apply(Message) → Message` method establishing the transformation rule abstraction
- `IMessageTransformer` interface and `MessageTransformer` use case (`HL7Parser.Application.UseCases`) — applies an ordered list of `ITransformRule` instances, threading each rule's output into the next
- `FieldCopyRule` (`HL7Parser.Application.Transformation`) implementing `ITransformRule` — copies a field's value (preserving full repetition/component/subcomponent structure) from a source segment/field to a target segment/field; no-ops gracefully when source segment, source field, target segment, or target field index is absent
- `MessageTypeSegmentRequirementsRule` extended with ten additional message-type entries, per a confirmed HL7 v2 reference list: `ACK → [MSA]`, `BAR → [EVN, PID, DG1]`, `DFT → [PID, FT1]`, `MFN → [MFI, MFE]`, `OMG → [PID, ORC, OBR]`, `OML → [PID, ORC, OBR]`, `RAS → [PID, ORC, RXA, RXR]`, `RDE → [PID, ORC, RXE, RXR]`, `SIU → [SCH, RGS]`, `VXU → [PID, ORC, RXA]`

### Changed
- Expanded `.editorconfig` with modern .NET conventions, nullable enforcement, null check pattern matching, and StyleCop rule overrides
- Added `max_line_length = 120` formatting convention
- Suppressed SA1309 and SA1101 StyleCop rules in favor of `_camelCase` convention
- `MessageValidator` refactored to accept an `IReadOnlyList<IConformanceRule>` (injected or defaulting to `{ new RequiredMshFieldsRule() }`); behavior is identical to the pre-refactor version when using the parameterless constructor
- `MessageValidator` default rule set extended to include `OptionalMshFieldsRule`; parameterless constructor now runs both required-field and optional-field checks
- `MessageValidator` default rule set further extended to include `MessageTypeSegmentRequirementsRule`
- `MessageValidator` default rule set further extended to include `MshDateTimeFormatRule`
- Backfilled `CHANGELOG.md` entries for Application-layer work in specs 01–03 (previously unrecorded), verified against git history
- **Behavior change:** corrected `MessageTypeSegmentRequirementsRule`'s required-segments table against a confirmed HL7 v2 reference list — `ADT` now requires `[EVN, PID]` (was `[PID]`), `MDM` now requires `[EVN, PID, TXA]` (was `[PID, TXA]`), `ORU` now requires `[PID, OBR, OBX]` (was `[PID, OBX]`); messages of these types that were previously treated as fully valid without an EVN (ADT/MDM) or OBR (ORU) segment will now fail validation with `Error`/`"REQUIRED_SEGMENT_MISSING"`. `ORM`'s `[PID, ORC]` mapping is unchanged, retained as legacy support for the pre-split generic order message

### Fixed
- Record-generated equality did not perform value comparison on collection-typed members (`Subcomponents`, `Components`, `Repetitions`), causing structurally identical instances to compare as unequal
- `src/Common/IsExternalInit.cs` polyfill was duplicated across `HL7Parser.Domain` and `HL7Parser.Application`; deduplicated to a single canonical file shared via `<Compile Link>` in each `.csproj`, resolving a compile error when using `init`-only properties across the assembly boundary under `netstandard2.0`
- `HL7Parser.Tests.Integration` had no project references, making integration tests impossible to write; added references to `HL7Parser.Application` and `HL7Parser.Domain`
- `Subcomponent.Create` incorrectly rejected the message's configured HL7 v2 escape character (default `\`) as a reserved delimiter, blocking parsing of any message containing standard escape sequences in `FT`-typed fields (e.g. `\.br\`, `\T\`); the escape character is now permitted as valid subcomponent content and stored as-is (pass-through, no interpretation)
---

<!--
  CHANGELOG INSTRUCTIONS
  ======================

  When making changes, add an entry under [Unreleased] in the appropriate
  subsection below. When a release is made, the [Unreleased] section is
  renamed to the release version and date, and a new empty [Unreleased]
  section is added above it.

  SUBSECTIONS (include only those that apply to a given release):

  ### Added
  New features, capabilities, or public API surface.

  ### Changed
  Changes to existing functionality that are non-breaking.

  ### Deprecated
  Features that will be removed in a future release.
  Include migration guidance where possible.

  ### Removed
  Features removed in this release. Should have appeared under
  Deprecated in a prior release.

  ### Fixed
  Bug fixes.

  ### Security
  Security-related fixes. Always include even for patch releases.

  VERSIONING GUIDE:

  Given a version number MAJOR.MINOR.PATCH:

  MAJOR — breaking change to the public API
  MINOR — new backward-compatible functionality
  PATCH — backward-compatible bug fix

  Pre-release versions use the suffix -alpha.N, -beta.N, or -rc.N.
  Example: 1.0.0-alpha.1, 1.0.0-beta.2, 1.0.0-rc.1

  EXAMPLE RELEASE ENTRY:

  ## [1.2.0] - 2026-08-15

  ### Added
  - Support for ORM^O01 message type parsing
  - `Message.GetRepetition(segment, field, index)` for accessing repeated fields

  ### Fixed
  - Incorrect component index when MSH-2 encoding characters contain a caret

  ### Changed
  - `ValidationResult.Errors` now returns `IReadOnlyList<ValidationFinding>`
    instead of `IEnumerable<ValidationFinding>` for consistency

  LINK REFERENCES (add at the bottom of the file for each release):

  [Unreleased]: https://github.com/ChrisWenzlick/HL7Parser/compare/v1.2.0...HEAD
  [1.2.0]: https://github.com/ChrisWenzlick/HL7Parser/compare/v1.1.0...v1.2.0
  [1.1.0]: https://github.com/ChrisWenzlick/HL7Parser/compare/v1.0.0...v1.1.0
  [1.0.0]: https://github.com/ChrisWenzlick/HL7Parser/releases/tag/v1.0.0
-->

---

[Unreleased]: https://github.com/ChrisWenzlick/HL7Parser/commits/main
