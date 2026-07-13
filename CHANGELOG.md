# Changelog

All notable changes to HL7Parser will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Added
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
- `Field`, `Repetition`, and `Component` implement `IReadOnlyList<T>` over their child collections (`Repetitions`, `Components`, `Subcomponents` respectively), enabling indexer chaining (`field[0][0][0].RawValue`), `foreach`, `Count`, and LINQ directly on these types

### Changed
- Expanded `.editorconfig` with modern .NET conventions, nullable enforcement, null check pattern matching, and StyleCop rule overrides
- Added `max_line_length = 120` formatting convention
- Suppressed SA1309 and SA1101 StyleCop rules in favor of `_camelCase` convention

### Fixed
- Record-generated equality did not perform value comparison on collection-typed members (`Subcomponents`, `Components`, `Repetitions`), causing structurally identical instances to compare as unequal
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
