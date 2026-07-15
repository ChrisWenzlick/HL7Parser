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
/// segments are absent, per a confirmed reference list of HL7 v2 message types.
/// <c>ORM</c> is retained separately as legacy support; it is not part of that reference
/// list because later HL7 v2 versions split it into <c>OMG</c>/<c>OML</c>.
/// </summary>
public sealed class MessageTypeSegmentRequirementsRule : IConformanceRule
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> RequiredSegments =
        new Dictionary<string, IReadOnlyList<string>>
        {
            ["ACK"] = ["MSA"],
            ["ADT"] = ["EVN", "PID"],
            ["BAR"] = ["EVN", "PID", "DG1"],
            ["DFT"] = ["PID", "FT1"],
            ["MDM"] = ["EVN", "PID", "TXA"],
            ["MFN"] = ["MFI", "MFE"],
            ["OMG"] = ["PID", "ORC", "OBR"],
            ["OML"] = ["PID", "ORC", "OBR"],
            ["ORM"] = ["PID", "ORC"],
            ["ORU"] = ["PID", "OBR", "OBX"],
            ["RAS"] = ["PID", "ORC", "RXA", "RXR"],
            ["RDE"] = ["PID", "ORC", "RXE", "RXR"],
            ["SIU"] = ["SCH", "RGS"],
            ["VXU"] = ["PID", "ORC", "RXA"],
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
