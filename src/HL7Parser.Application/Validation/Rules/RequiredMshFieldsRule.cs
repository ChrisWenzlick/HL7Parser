// <copyright file="RequiredMshFieldsRule.cs" company="Christopher Wenzlick">
// Copyright (c) Christopher Wenzlick. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using HL7Parser.Domain;
using HL7Parser.Domain.Common;

namespace HL7Parser.Application.Validation.Rules;

/// <summary>
/// Checks that all required MSH fields (MSH-7, MSH-9, MSH-10, MSH-11, MSH-12) are
/// present and non-blank per the HL7 v2 base standard.
/// </summary>
public sealed class RequiredMshFieldsRule : IConformanceRule
{
    private static readonly IReadOnlyList<(int FieldIndex, string Description)> RequiredFields =
    [
        (7, "MSH-7 (Date/Time of Message) is required."),
        (9, "MSH-9 (Message Type) is required."),
        (10, "MSH-10 (Message Control ID) is required."),
        (11, "MSH-11 (Processing ID) is required."),
        (12, "MSH-12 (Version ID) is required."),
    ];

    /// <inheritdoc/>
    public bool Applies(Message message) => true;

    /// <inheritdoc/>
    public IReadOnlyList<ValidationIssue> Evaluate(Message message)
    {
        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        var issues = new List<ValidationIssue>();

        foreach (var (fieldIndex, description) in RequiredFields)
        {
            if (IsFieldMissing(message, fieldIndex))
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Error,
                    $"MSH-{fieldIndex}",
                    "REQUIRED_FIELD_MISSING",
                    description));
            }
        }

        return issues.AsReadOnly();
    }

    private static bool IsFieldMissing(Message message, int hl7Index)
    {
        Result<Field> fieldResult = message.Msh.GetField(hl7Index);
        if (!fieldResult.IsSuccess)
        {
            return true;
        }

        var rawValue = fieldResult.Value[0][0][0].RawValue;
        return string.IsNullOrWhiteSpace(rawValue);
    }
}
