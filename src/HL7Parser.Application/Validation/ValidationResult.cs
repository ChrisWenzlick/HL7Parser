// <copyright file="ValidationResult.cs" company="Christopher Wenzlick">
// Copyright (c) Christopher Wenzlick. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

namespace HL7Parser.Application.Validation;

/// <summary>
/// Represents the outcome of validating a <see cref="HL7Parser.Domain.Message"/> against
/// a set of conformance rules.
/// </summary>
public sealed record ValidationResult
{
    /// <summary>
    /// Gets a value indicating whether the validated message is free of any
    /// issue with a <see cref="ValidationSeverity.Error"/> severity.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// Gets the issues found while validating the message.
    /// </summary>
    public IReadOnlyList<ValidationIssue> Issues { get; }

    private ValidationResult(IReadOnlyList<ValidationIssue> issues)
    {
        Issues = issues;
        IsValid = !issues.Any(issue => issue.Severity == ValidationSeverity.Error);
    }

    /// <summary>
    /// Creates a new <see cref="ValidationResult"/> from the specified issues.
    /// </summary>
    /// <param name="issues">The issues found while validating the message.</param>
    /// <returns>
    /// A <see cref="ValidationResult"/> whose <see cref="IsValid"/> is derived from
    /// whether <paramref name="issues"/> contains any <see cref="ValidationSeverity.Error"/> issue.
    /// </returns>
    public static ValidationResult Create(IReadOnlyList<ValidationIssue> issues)
    {
        if (issues is null)
        {
            throw new ArgumentNullException(nameof(issues));
        }

        return new ValidationResult(issues);
    }
}
