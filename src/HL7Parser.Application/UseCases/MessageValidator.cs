// <copyright file="MessageValidator.cs" company="Christopher Wenzlick">
// Copyright (c) Christopher Wenzlick. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using HL7Parser.Application.Validation;
using HL7Parser.Domain;
using HL7Parser.Domain.Common;

namespace HL7Parser.Application.UseCases;

/// <summary>
/// Validates a parsed <see cref="Message"/> against the HL7 v2 base standard's
/// required MSH field rules.
/// </summary>
public sealed class MessageValidator : IMessageValidator
{
    private static readonly IReadOnlyList<(int FieldIndex, string Description)> RequiredMshFields =
    [
        (7, "MSH-7 (Date/Time of Message) is required."),
        (9, "MSH-9 (Message Type) is required."),
        (10, "MSH-10 (Message Control ID) is required."),
        (11, "MSH-11 (Processing ID) is required."),
        (12, "MSH-12 (Version ID) is required."),
    ];

    /// <summary>
    /// Validates a parsed <see cref="Message"/> against the HL7 v2 base standard's
    /// required MSH field rules.
    /// </summary>
    /// <param name="message">The parsed message to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> describing any conformance issues found.</returns>
    public ValidationResult Execute(Message message)
    {
        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        var issues = new List<ValidationIssue>();

        foreach (var (fieldIndex, description) in RequiredMshFields)
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

        return ValidationResult.Create(issues);
    }

    private static bool IsFieldMissing(Message message, int hl7Index)
    {
        Result<Field> fieldResult = message.Msh.GetField(hl7Index);
        if (!fieldResult.IsSuccess)
        {
            return true;
        }

        var rawValue = fieldResult.Value.Repetitions[0].Components[0].Subcomponents[0].RawValue;
        return string.IsNullOrWhiteSpace(rawValue);
    }
}
