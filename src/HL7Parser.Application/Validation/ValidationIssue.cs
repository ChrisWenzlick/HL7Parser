// <copyright file="ValidationIssue.cs" company="Christopher Wenzlick">
// Copyright (c) Christopher Wenzlick. All rights reserved.
// </copyright>

using System;

namespace HL7Parser.Application.Validation;

/// <summary>
/// Represents a single conformance issue found while validating a <see cref="HL7Parser.Domain.Message"/>.
/// </summary>
/// <param name="severity">The severity of the issue.</param>
/// <param name="location">The location within the message the issue applies to, e.g. <c>"MSH-7"</c>.</param>
/// <param name="code">A short, stable machine-readable code identifying the rule that produced the issue.</param>
/// <param name="description">A human-readable description of the issue.</param>
public sealed record ValidationIssue(ValidationSeverity severity, string location, string code, string description)
{
    /// <summary>
    /// Gets the severity of the issue.
    /// </summary>
    public ValidationSeverity Severity { get; init; } = severity;

    /// <summary>
    /// Gets the location within the message the issue applies to, e.g. <c>"MSH-7"</c>.
    /// </summary>
    public string Location { get; init; } = location ?? throw new ArgumentNullException(nameof(location));

    /// <summary>
    /// Gets a short, stable machine-readable code identifying the rule that produced the issue.
    /// </summary>
    public string Code { get; init; } = code ?? throw new ArgumentNullException(nameof(code));

    /// <summary>
    /// Gets a human-readable description of the issue.
    /// </summary>
    public string Description { get; init; } = description ?? throw new ArgumentNullException(nameof(description));
}
