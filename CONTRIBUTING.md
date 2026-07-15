# Contributing to HL7Parser

Thank you for your interest in contributing to HL7Parser. This document explains how to get involved, what to expect from the process, and the standards this project holds itself to.

By participating in this project, you agree to abide by the [Code of Conduct](CODE_OF_CONDUCT.md).

---

## Table of Contents

- [Ways to Contribute](#ways-to-contribute)
- [Reporting Issues](#reporting-issues)
- [Suggesting Enhancements](#suggesting-enhancements)
- [Development Setup](#development-setup)
- [Making Changes](#making-changes)
- [Coding Standards](#coding-standards)
- [Testing Requirements](#testing-requirements)
- [Submitting a Pull Request](#submitting-a-pull-request)
- [Commit Message Convention](#commit-message-convention)

---

## Ways to Contribute

Contributions of all kinds are welcome:

- Reporting bugs or unexpected behavior
- Suggesting new features or API improvements
- Improving documentation, examples, or the README
- Fixing bugs or implementing features from the issue tracker
- Writing or improving tests
- Reviewing pull requests

If you are unsure whether your idea is a good fit, open a Discussion before investing time in an implementation.

---

## Reporting Issues

Before opening an issue, please search existing issues to avoid duplicates.

When reporting a bug, include:

- The version of HL7Parser you are using
- The target framework your project uses (.NET version or .NET Framework version)
- A minimal, reproducible example — the smallest HL7 message and code that demonstrates the problem
- The actual behavior you observed
- The behavior you expected

If the issue involves a specific HL7 message, please de-identify it before sharing. Do not include real patient health information in any issue, pull request, or discussion.

---

## Suggesting Enhancements

Open an issue with the label `enhancement` describing:

- The problem you are trying to solve
- Your proposed solution or API design
- Any alternatives you considered
- Whether you are willing to implement it yourself

For significant API changes, expect discussion before implementation begins. Breaking changes require strong justification and will be considered carefully.

---

## Development Setup

### Prerequisites

- Visual Studio 2026 or later (Windows), or VS Code with the C# Dev Kit (macOS / Linux)
- .NET SDK 8.0, 9.0, and 10.0
- Git

### Getting Started

1. Fork the repository on GitHub
2. Clone your fork locally:
   ```
   git clone https://github.com/ChrisWenzlick/HL7Parser.git
   cd HL7Parser
   ```
3. Open `HL7Parser.sln` in Visual Studio or your editor of choice
4. Build the solution to verify your setup:
   ```
   dotnet build
   ```
5. Run the full test suite:
   ```
   dotnet test
   ```

All projects should build cleanly and all tests should pass before you make any changes. If they do not, open an issue.

---

## Making Changes

### Branching

Create a branch from `main` for your work. Use a descriptive name:

```
git checkout -b fix/msh-delimiter-detection
git checkout -b feature/ack-message-generation
git checkout -b docs/improve-quick-start-example
```

### Keep Changes Focused

Each pull request should address one thing. A PR that fixes a bug and also refactors an unrelated class is harder to review and harder to revert if something goes wrong. If you find something worth improving while working on something else, open a separate issue or PR.

### Architecture

HL7Parser uses Clean Architecture. Dependencies flow strictly inward:

```
Domain ← Application ← Infrastructure
```

Do not introduce dependencies that violate this flow. Domain must have no project references. If you find yourself wanting to reference an outer layer from an inner one, the code likely belongs in a different layer. See [/docs/adr](docs/adr/) for documented architectural decisions.

---

## Coding Standards

This project enforces coding standards through Roslyn analyzers, StyleCop, and `.editorconfig`. Running a build will surface any violations.

Key standards to be aware of:

- All public types and members must have XML documentation comments
- Follow the ubiquitous language of the HL7 domain in naming — prefer `Segment`, `Field`, `Component` over generic alternatives
- Domain objects must protect their invariants — never allow construction of an object in an invalid state
- Prefer immutability, especially in the domain layer
- Avoid primitive obsession — wrap meaningful values in value objects where appropriate

If you disagree with a specific rule, open a Discussion rather than suppressing the analyzer inline. Inline suppression requires a comment explaining the justification and will be scrutinized during review.

---

## Testing Requirements

All contributions that change behavior must include tests. This project follows Test-Driven Development — tests should be written before or alongside implementation, not after.

### Standards

- Tests are written with xUnit and FluentAssertions
- Test names follow the pattern `MethodOrBehavior_ExpectedResult_WhenCondition`
- Both happy paths and failure cases must be tested
- Tests must be deterministic — no time dependencies, random values, or external network calls in unit tests
- Do not write tests solely to increase coverage numbers; every test should assert meaningful behavior

### Running Tests Against a Specific Framework

```
dotnet test -f net8.0
dotnet test -f net9.0
dotnet test -f net10.0
```

### Coverage

Coverage reports can be generated locally using Coverlet and ReportGenerator. Instructions will be added to this document once the tooling is configured.

---

## Submitting a Pull Request

1. Ensure all tests pass locally across all target frameworks
2. Ensure the build produces no warnings (warnings are treated as errors in CI)
3. Update the [CHANGELOG.md](CHANGELOG.md) under the `[Unreleased]` section describing your change
4. Update documentation and XML doc comments if your change affects the public API
5. Push your branch and open a pull request against `main`
6. Fill out the pull request template completely
7. Be responsive to review feedback — PRs that go stale will be closed

### What to Expect

All pull requests are reviewed before merging. Feedback is given in the spirit of improving the library and is not a reflection on you as a developer. You may be asked to make changes, add tests, or reconsider an approach. This is normal and expected.

### Merge Strategy

This repository uses **squash and merge** for all pull requests. This keeps
the commit history on `main` clean and linear — one commit per PR, using the
PR title as the commit message. Ensure your PR title follows the
[Conventional Commits](#commit-message-convention) format before merging, as
it becomes the permanent commit message on `main`.

---

## Commit Message Convention

This project follows the [Conventional Commits](https://www.conventionalcommits.org/) specification.

Format:

```
<type>(<scope>): <short description>

[optional body]

[optional footer]
```

Types:

| Type | When to Use |
|---|---|
| `feat` | A new feature or capability |
| `fix` | A bug fix |
| `docs` | Documentation changes only |
| `test` | Adding or correcting tests |
| `refactor` | Code change that neither fixes a bug nor adds a feature |
| `chore` | Build process, tooling, or dependency updates |
| `perf` | Performance improvement |

Examples:

```
feat(parser): add support for ORM^O01 message type

fix(validation): correctly handle empty MSH-2 field

docs(readme): add component access example to quick start

test(domain): add negative cases for malformed delimiter detection
```

Breaking changes must include `BREAKING CHANGE:` in the commit footer with a description of what changed and why.

---

## Releasing

Releases are cut by a maintainer via a git-tag-driven publish pipeline. See [docs/RELEASING.md](docs/RELEASING.md) for the runbook and required repository secrets.

---

## Questions

If you have a question that is not covered here, open a Discussion on GitHub. Do not use issues for general questions.
