// <copyright file="ValidationSeverity.cs" company="Christopher Wenzlick">
// Copyright (c) Christopher Wenzlick. All rights reserved.
// </copyright>

namespace HL7Parser.Application.Validation;

/// <summary>
/// Indicates the severity of a <see cref="ValidationIssue"/>.
/// </summary>
public enum ValidationSeverity
{
    /// <summary>
    /// The message violates a required conformance rule.
    /// </summary>
    Error,

    /// <summary>
    /// The message violates a recommended, non-required conformance rule.
    /// </summary>
    Warning,

    /// <summary>
    /// The message contains a notable but non-conformance-affecting observation.
    /// </summary>
    Info,
}
