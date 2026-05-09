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
- **Validate** message structure, required segments, and field integrity
- **Transform** messages through a configurable field-mapping pipeline
- **Distinguish** between structural errors and conformance warnings
- **Handle** real-world HL7 edge cases including malformed delimiters, empty fields, and Z-segments
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
using HL7Parser.Application;

var raw = "MSH|^~\\&|SENDING_APP|SENDING_FAC|RECEIVING_APP|RECEIVING_FAC|20240101120000||ADT^A01|MSG00001|P|2.5.1\r" +
          "PID|||12345^^^MRN||SMITH^JOHN^A||19800101|M";

var parser = new Hl7MessageParser();
var result = parser.Parse(raw);

if (result.IsSuccess)
{
    var message = result.Value;
    Console.WriteLine(message.GetField("MSH", 9));   // ADT^A01
    Console.WriteLine(message.GetField("PID", 5));   // SMITH^JOHN^A
}
```

### Validating a Message

```csharp
var validator = new Hl7MessageValidator();
var validation = validator.Validate(message);

foreach (var error in validation.Errors)
{
    Console.WriteLine($"[{error.Severity}] {error.Code}: {error.Description}");
}
```

### Accessing Segments, Fields, and Components

```csharp
// Access by segment name and field index
var patientName = message.GetField("PID", 5);

// Access a specific component
var lastName = message.GetComponent("PID", 5, 1);
var firstName = message.GetComponent("PID", 5, 2);

// Iterate segments
foreach (var segment in message.Segments)
{
    Console.WriteLine(segment.Type);
}
```

> **Note:** The API shown above reflects the intended design direction. Specific method signatures may evolve as the library matures. See the [changelog](CHANGELOG.md) for version history.

---

## Supported Message Types

| Type | Trigger Events | Description |
|---|---|---|
| ADT | A01, A02, A03, A08 | Patient administration |
| ORU | R01 | Observation results |
| ORM | O01 | Order messages |
| MDM | T02 | Medical document management |

Additional message types are handled gracefully — unknown segment types are preserved without error.

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
HL7Parser.Application     # Use cases, parsing pipeline, validation rules
HL7Parser.Infrastructure  # I/O, stream handling, MLLP framing
```

### Domain Model

The domain model reflects the natural structure of an HL7 v2 message:

```
Message
└── Segment[]
    └── Field[]
        └── Component[]
            └── Subcomponent[]
```

Key design decisions are documented in [/docs/adr](docs/adr/).

---

## Validation

HL7Parser distinguishes between two categories of validation findings:

| Category | Description | Example |
|---|---|---|
| **Structural Error** | The message cannot be reliably parsed | Missing MSH segment, malformed delimiters |
| **Conformance Warning** | The message parses but deviates from the spec | Missing optional field, unexpected Z-segment |

This distinction matters in production healthcare environments, where perfectly conformant messages are the exception rather than the rule.

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
- [ ] Core domain model (Message, Segment, Field, Component)
- [ ] MSH parsing and delimiter detection
- [ ] Full segment tokenization
- [ ] Structural validation
- [ ] Common message type support (ADT, ORU, ORM, MDM)
- [ ] Transformation pipeline
- [ ] NuGet package publication
- [ ] MLLP framing support
- [ ] HL7 v2 to FHIR R4 segment mapping

---

## License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for details.

---

## Acknowledgements

HL7® is a registered trademark of Health Level Seven International. Use of the trademark does not constitute endorsement by HL7.

Specification reference: [Caristix HL7 Definition](https://hl7-definition.caristix.com/v2/) and [hl7.eu](https://www.hl7.eu/HL7v2x/v251/std251/hl7.html).
