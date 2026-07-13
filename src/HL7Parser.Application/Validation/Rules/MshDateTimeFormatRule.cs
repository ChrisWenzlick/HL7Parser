// <copyright file="MshDateTimeFormatRule.cs" company="Christopher Wenzlick">
// Copyright (c) Christopher Wenzlick. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HL7Parser.Domain;
using HL7Parser.Domain.Common;

namespace HL7Parser.Application.Validation.Rules;

/// <summary>
/// Emits an <see cref="ValidationSeverity.Error"/> issue when MSH-7 is present and non-blank
/// but does not conform to the HL7 v2 <c>TS</c> (Time Stamp) structural format.
/// Does not fire when MSH-7 is missing or blank — that case is already covered by
/// <see cref="RequiredMshFieldsRule"/>.
/// </summary>
public sealed class MshDateTimeFormatRule : IConformanceRule
{
    private static readonly Regex TsPattern = new Regex(
        @"^\d{4}(\d{2}(\d{2}(\d{4}(\d{2})?)?)?)?(\.\d{1,4})?([+-]\d{4})?$",
        RegexOptions.Compiled);

    /// <inheritdoc/>
    public bool Applies(Message message)
    {
        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        return ReadMsh7(message) is not null;
    }

    /// <inheritdoc/>
    public IReadOnlyList<ValidationIssue> Evaluate(Message message)
    {
        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        var rawValue = ReadMsh7(message);
        if (rawValue is null || TsPattern.IsMatch(rawValue))
        {
            return [];
        }

        var issues = new List<ValidationIssue>
        {
            new ValidationIssue(
                ValidationSeverity.Error,
                "MSH-7",
                "INVALID_FIELD_FORMAT",
                "MSH-7 (Date/Time of Message) is not a valid HL7 v2 timestamp."),
        };

        return issues.AsReadOnly();
    }

    private static string? ReadMsh7(Message message)
    {
        Result<Field> fieldResult = message.Msh.GetField(7);
        if (!fieldResult.IsSuccess)
        {
            return null;
        }

        var rawValue = fieldResult.Value[0][0][0].RawValue;
        return string.IsNullOrWhiteSpace(rawValue) ? null : rawValue;
    }
}
