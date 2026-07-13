// <copyright file="MessageTypeSegmentRequirementsRule.cs" company="Christopher Wenzlick">
// Copyright (c) Christopher Wenzlick. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using HL7Parser.Domain;
using HL7Parser.Domain.Common;

namespace HL7Parser.Application.Validation.Rules;

/// <summary>
/// Emits <see cref="ValidationSeverity.Error"/> issues when a message type's required
/// segments are absent. Currently seeded with: <c>ADT → [PID]</c>.
/// </summary>
public sealed class MessageTypeSegmentRequirementsRule : IConformanceRule
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> RequiredSegments =
        new Dictionary<string, IReadOnlyList<string>>
        {
            ["ADT"] = ["PID"],
            ["ORU"] = ["PID", "OBX"],
            ["ORM"] = ["PID", "ORC"],
            ["MDM"] = ["PID", "TXA"],
        };

    /// <inheritdoc/>
    public bool Applies(Message message)
    {
        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        var typeCode = ExtractMessageTypeCode(message);
        return typeCode is not null && RequiredSegments.ContainsKey(typeCode);
    }

    /// <inheritdoc/>
    public IReadOnlyList<ValidationIssue> Evaluate(Message message)
    {
        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        var typeCode = ExtractMessageTypeCode(message);
        if (typeCode is null || !RequiredSegments.TryGetValue(typeCode, out var requiredIdentifiers))
        {
            return [];
        }

        var issues = new List<ValidationIssue>();

        foreach (var identifier in requiredIdentifiers)
        {
            if (message.GetSegments(identifier).Count == 0)
            {
                issues.Add(new ValidationIssue(
                    ValidationSeverity.Error,
                    identifier,
                    "REQUIRED_SEGMENT_MISSING",
                    $"{identifier} segment is required for {typeCode} messages."));
            }
        }

        return issues.AsReadOnly();
    }

    private static string? ExtractMessageTypeCode(Message message)
    {
        Result<Field> fieldResult = message.Msh.GetField(9);
        if (!fieldResult.IsSuccess)
        {
            return null;
        }

        var rawValue = fieldResult.Value[0][0][0].RawValue;
        return string.IsNullOrWhiteSpace(rawValue) ? null : rawValue;
    }
}
