// <copyright file="IConformanceRule.cs" company="Christopher Wenzlick">
// Copyright (c) Christopher Wenzlick. All rights reserved.
// </copyright>

using System.Collections.Generic;
using HL7Parser.Domain;

namespace HL7Parser.Application.Validation;

/// <summary>
/// Represents a single HL7 v2 conformance rule that can be applied to a parsed message.
/// </summary>
public interface IConformanceRule
{
    /// <summary>
    /// Returns <see langword="true"/> if this rule is applicable to <paramref name="message"/>.
    /// When <see langword="false"/>, <see cref="Evaluate"/> is not called.
    /// </summary>
    /// <param name="message">The parsed message to check.</param>
    /// <returns><see langword="true"/> if this rule applies; otherwise <see langword="false"/>.</returns>
    bool Applies(Message message);

    /// <summary>
    /// Evaluates the rule against <paramref name="message"/> and returns any
    /// <see cref="ValidationIssue"/>s found. Only called when <see cref="Applies"/> returns
    /// <see langword="true"/>.
    /// </summary>
    /// <param name="message">The parsed message to evaluate.</param>
    /// <returns>A read-only list of issues; empty if the message passes this rule.</returns>
    IReadOnlyList<ValidationIssue> Evaluate(Message message);
}
