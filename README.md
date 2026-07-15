# HL7Parser

A modern, lightweight, and thoroughly tested .NET library for parsing, validating, and transforming HL7 v2 messages. Built with Clean Architecture and Domain-Driven Design principles.

[![Build](https://github.com/ChrisWenzlick/HL7Parser/actions/workflows/build.yml/badge.svg)](https://github.com/ChrisWenzlick/HL7Parser/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/HL7Parser.svg)](https://www.nuget.org/packages/HL7Parser)
[![NuGet Downloads](https://img.shields.io/nuget/dt/HL7Parser.svg)](https://www.nuget.org/packages/HL7Parser)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

---

## Overview

HL7 v2 is the dominant messaging standard in healthcare data exchange, used by the vast majority of hospitals, laboratories, and clinical systems worldwide. Despite this, the .NET ecosystem lacks a modern, idiomatic library that covers parsing, structural validation, and transformation in a single, well-tested package.

HL7Parser fills that gap. It is designed to handle the realities of production healthcare environments — where messages deviate from the specification, custom Z-segments are common, and graceful degradation matters as much as strict conformance.

### Key Features

- **Parse** HL7 v2 messages into a strongly-typed, navigable domain model
- **Validate** MSH conformance and message-type-conditional required-segment presence, distinguishing structural errors from conformance warnings
- **Transform** messages through a minimal, composable field-remapping pipeline
- **Handle** real-world HL7 edge cases including malformed delimiters, empty fields, Z-segments, and HL7 v2 escape sequences
- **Target** multiple runtimes — from legacy .NET Framework to the latest .NET release

---

## Supported Runtimes

| Runtime | Version |
|---|---|
| .NET Standard | 2.0 |
| .NET | 8.0, 9.0, 10.0 |
| .NET Framework | 4.6.1+ (via .NET Standard 2.0) |

---

## Installation

Install via the NuGet Package Manager:

```
dotnet add package HL7Parser
```

Or via the Visual Studio NuGet Package Manager UI by searching for `HL7Parser`.

---

## Quick Start

### Parsing a Message

```csharp
using HL7Parser.Application.UseCases;

var raw = "MSH|^~\\&|SENDING_APP|SENDING_FAC|RECEIVING_APP|RECEIVING_FAC|20240101120000||ADT^A01|MSG00001|P|2.5.1\r" +
          "EVN|A01|20240101120000\r" +
          "PID|1||12345^^^MRN||SMITH^JOHN^A||19800101|M\r" +
          "OBX|1|ST|TEST^Result||ORIGINAL";

var parseResult = new MessageParser().Execute(raw);

if (parseResult.IsSuccess)
{
    var message = parseResult.Value;

    Console.WriteLine(message.Msh.GetField(9).Value.ToHl7String());                        // ADT^A01
    Console.WriteLine(message.GetSegments("PID")[0].GetField(5).Value.ToHl7String());       // SMITH^JOHN^A
}
```

### Validating a Message

```csharp
using HL7Parser.Application.UseCases;

var validationResult = new MessageValidator().Execute(message);

foreach (var issue in validationResult.Issues)
{
    Console.WriteLine($"[{issue.Severity}] {issue.Code} at {issue.Location}: {issue.Description}");
}

Console.WriteLine(validationResult.IsValid ? "Valid" : "Invalid");
```

### Accessing Segments, Fields, and Components

```csharp
// All segments matching an identifier (a repeatable segment can return more than one)
var pidSegments = message.GetSegments("PID");

// A specific field by one-based HL7 index — GetField returns a Result<Field>
var patientNameField = pidSegments[0].GetField(5);

if (patientNameField.IsSuccess)
{
    // Field indexing: field[repetition][component][subcomponent]
    string lastName = patientNameField.Value[0][0][0].RawValue!;   // SMITH
    string firstName = patientNameField.Value[0][1][0].RawValue!;  // JOHN

    // Or the whole field as reconstructed HL7 text
    string wholeField = patientNameField.Value.ToHl7String();      // SMITH^JOHN^A
}

// Iterate segments
foreach (var segment in message.Segments)
{
    Console.WriteLine(segment.SegmentType.Identifier);
}
```

### Transforming a Message

```csharp
using HL7Parser.Application.Transformation;
using HL7Parser.Application.UseCases;

var transformer = new MessageTransformer([new FieldCopyRule("PID", 5, "OBX", 5)]);
var transformed = transformer.Execute(message);

Console.WriteLine(transformed.GetSegments("OBX")[0].GetField(5).Value.ToHl7String()); // SMITH^JOHN^A
```

See [Transformation](#transformation) below — this pipeline is currently a minimal foundation, not a general-purpose remapping engine.

---

## Supported Message Types

HL7Parser validates message-type-conditional **required-segment presence** — that a message of a given type contains the segments it needs, not full field-level conformance within those segments. The current table, from `MessageTypeSegmentRequirementsRule`:

| Type | Required Segments |
|---|---|
| ACK | MSA |
| ADT | EVN, PID |
| BAR | EVN, PID, DG1 |
| DFT | PID, FT1 |
| MDM | EVN, PID, TXA |
| MFN | MFI, MFE |
| OMG | PID, ORC, OBR |
| OML | PID, ORC, OBR |
| ORM | PID, ORC *(legacy — see note below)* |
| ORU | PID, OBR, OBX |
| RAS | PID, ORC, RXA, RXR |
| RDE | PID, ORC, RXE, RXR |
| SIU | SCH, RGS |
| VXU | PID, ORC, RXA |

`ORM^O01` is retained for legacy compatibility even though later HL7 v2 versions split it into the more specific `OMG`/`OML` order messages above.

Message types not in this table are handled gracefully — unknown segment types are preserved without error, and no required-segment check is applied to them.

---

## HL7 v2 Primer

HL7 v2 messages are flat ASCII strings with a defined delimiter hierarchy:

```
MSH|^~\&|App|Fac|||20240101||ADT^A01|001|P|2.5.1
PID|||12345||SMITH^JOHN
```

| Delimiter | Character | Separates |
|---|---|---|
| Field separator | `\|` | Fields within a segment |
| Component separator | `^` | Components within a field |
| Subcomponent separator | `&` | Subcomponents within a component |
| Repetition separator | `~` | Repeated field values |
| Escape character | `\` | Escaped special characters |

The MSH segment is self-describing — MSH-1 defines the field separator and MSH-2 defines the remaining encoding characters. All other segments are parsed using the delimiters derived from MSH.

For a full reference, see the [HL7 v2 specification on Caristix](https://hl7-definition.caristix.com/v2/).

---

## Architecture

HL7Parser is structured using Clean Architecture with Domain-Driven Design principles. Dependencies flow strictly inward — outer layers depend on inner layers, never the reverse.

```
HL7Parser.Domain          # Core entities, value objects, domain logic
HL7Parser.Application     # Use cases, parsing pipeline, validation and transformation rules
HL7Parser.Infrastructure  # Reserved for I/O and transport (e.g. MLLP framing); no shipped functionality yet
```

### Domain Model

The domain model reflects the natural structure of an HL7 v2 message:

```
Message
└── Segment[]
    └── Field[]
        └── Repetition[]
            └── Component[]
                └── Subcomponent[]
```

Key design decisions are documented in [/docs/adr](docs/adr/).

---

## Validation

HL7Parser distinguishes between two categories of validation findings:

| Category | Description | Example |
|---|---|---|
| **Error** | The message fails HL7 conformance | Missing required MSH field, missing required segment for its message type, malformed MSH-7 timestamp |
| **Warning** | The message parses and conforms, but is missing optional or commonly-expected data | Missing optional MSH field (e.g. sending application) |

`ValidationResult.IsValid` reflects only `Error`-severity findings; `Warning`-severity findings are surfaced in `Issues` without affecting validity. This distinction matters in production healthcare environments, where perfectly conformant messages are the exception rather than the rule.

---

## Transformation

HL7Parser includes a composable message-transformation pipeline: an ordered list of `ITransformRule` instances, applied in sequence by `MessageTransformer`, each producing a new (not mutated) `Message`.

**As of v1.0, this is a minimal foundation, not a general-purpose HL7 v2 remapping engine.** The only rule shipped today is `FieldCopyRule`, which copies one field's value — preserving its full repetition/component/subcomponent structure — from a source segment/field to a target segment/field. It operates on the first matching segment for both source and target, and no-ops (returns the message unchanged) if the source segment, source field, target segment, or target field index doesn't exist. It does not create segments, and there is no conditional or multi-field rule support yet.

---

## Real-World Considerations

HL7 v2 is sometimes called the "non-standard standard." The specification provides approximately 80% of the interface definition — the remaining 20% is negotiated between sender and receiver on a per-implementation basis. HL7Parser is designed with this in mind:

- Unknown segment types are preserved, not rejected
- Missing optional fields do not produce errors
- Z-segments (custom segments) are parsed as generic segments
- Validation findings are categorized by severity so callers can decide how to respond

---

## Contributing

Contributions are welcome. Please read [CONTRIBUTING.md](CONTRIBUTING.md) before submitting a pull request.

This project follows the [Contributor Covenant Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this standard.

### Development Setup

1. Clone the repository
   ```
   git clone https://github.com/ChrisWenzlick/HL7Parser.git
   ```
2. Open `HL7Parser.sln` in Visual Studio 2026 or later
3. Build the solution — all target frameworks should compile cleanly
4. Run the test suite via Test Explorer or `dotnet test`

### Running Tests

```
dotnet test
```

To run tests against a specific target framework:

```
dotnet test -f net8.0
```

---

## Roadmap

- [x] Project structure and solution setup
- [x] Core domain model (Message, Segment, Field, Repetition, Component, Subcomponent)
- [x] MSH parsing and delimiter detection
- [x] Full segment tokenization
- [x] Structural and conformance validation (required/optional MSH fields, message-type-conditional required-segment presence, MSH-7 date/time format)
- [x] Message type support — required-segment presence for 14 message types (see [Supported Message Types](#supported-message-types))
- [x] Transformation pipeline foundation (single-field copy between existing segments)
- [ ] NuGet package publication
- [ ] MLLP framing support *(planned post-1.0)*
- [ ] HL7 v2 to FHIR R4 segment mapping *(planned post-1.0)*

---

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.

---

## Acknowledgements

HL7® is a registered trademark of Health Level Seven International. Use of the trademark does not constitute endorsement by HL7.

Specification reference: [Caristix HL7 Definition](https://hl7-definition.caristix.com/v2/) and [hl7.eu](https://www.hl7.eu/HL7v2x/v251/std251/hl7.html).
