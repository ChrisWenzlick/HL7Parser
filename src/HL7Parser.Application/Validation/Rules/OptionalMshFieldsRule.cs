// <copyright file="OptionalMshFieldsRule.cs" company="Christopher Wenzlick">
// Copyright (c) Christopher Wenzlick. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using HL7Parser.Domain;
using HL7Parser.Domain.Common;

namespace HL7Parser.Application.Validation.Rules;

/// <summary>
/// Emits <see cref="ValidationSeverity.Warning"/> issues for MSH-3, MSH-4, MSH-5, and MSH-6
/// (Sending Application, Sending Facility, Receiving Application, Receiving Facility) when
/// missing or blank. These fields are Optional per the HL7 v2 base standard.
/// </summary>
public sealed class OptionalMshFieldsRule : IConformanceRule
{
    private static readonly IReadOnlyList<(int FieldIndex, string Description)> OptionalFields =
    [
        (3, "MSH-3 (Sending Application) is recommended."),
        (4, "MSH-4 (Sending Facility) is recommended."),
        (5, "MSH-5 (Receiving Application) is recommended."),
        (6, "MSH-6 (Receiving Facility) is recommended."),
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

        foreach (var (fieldIndex, description) in OptionalFields)
        {
            if (IsFieldMissing(message, fieldIndex))
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Warning,
                    $"MSH-{fieldIndex}",
                    "OPTIONAL_FIELD_MISSING",
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
