// <copyright file="FieldCopyRule.cs" company="Christopher Wenzlick">
// Copyright (c) Christopher Wenzlick. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using HL7Parser.Domain;
using HL7Parser.Domain.Common;
using HL7Parser.Domain.Segments;

namespace HL7Parser.Application.Transformation;

/// <summary>
/// Copies the value of one field to another field, identified by segment type and
/// one-based HL7 field index. Operates on the first matching segment for both source
/// and target. Returns the message unchanged if the source segment, source field, target
/// segment, or target field index does not exist on the given message.
/// </summary>
public sealed class FieldCopyRule : ITransformRule
{
    private readonly string _sourceSegmentIdentifier;
    private readonly int _sourceFieldIndex;
    private readonly string _targetSegmentIdentifier;
    private readonly int _targetFieldIndex;

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldCopyRule"/> class.
    /// </summary>
    /// <param name="sourceSegmentIdentifier">
    /// The three-character HL7 identifier of the source segment (e.g. <c>"PID"</c>).
    /// Cannot be <see langword="null"/>.
    /// </param>
    /// <param name="sourceFieldIndex">The one-based HL7 index of the source field.</param>
    /// <param name="targetSegmentIdentifier">
    /// The three-character HL7 identifier of the target segment (e.g. <c>"OBX"</c>).
    /// Cannot be <see langword="null"/>.
    /// </param>
    /// <param name="targetFieldIndex">The one-based HL7 index of the target field.</param>
    public FieldCopyRule(
        string sourceSegmentIdentifier,
        int sourceFieldIndex,
        string targetSegmentIdentifier,
        int targetFieldIndex)
    {
        if (sourceSegmentIdentifier is null)
        {
            throw new ArgumentNullException(nameof(sourceSegmentIdentifier));
        }

        if (targetSegmentIdentifier is null)
        {
            throw new ArgumentNullException(nameof(targetSegmentIdentifier));
        }

        _sourceSegmentIdentifier = sourceSegmentIdentifier;
        _sourceFieldIndex = sourceFieldIndex;
        _targetSegmentIdentifier = targetSegmentIdentifier;
        _targetFieldIndex = targetFieldIndex;
    }

    /// <inheritdoc/>
    public Message Apply(Message message)
    {
        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        var sourceSegments = message.GetSegments(_sourceSegmentIdentifier);
        if (sourceSegments.Count == 0)
        {
            return message;
        }

        Result<Field> sourceFieldResult = sourceSegments[0].GetField(_sourceFieldIndex);
        if (!sourceFieldResult.IsSuccess)
        {
            return message;
        }

        var targetSegments = message.GetSegments(_targetSegmentIdentifier);
        if (targetSegments.Count == 0)
        {
            return message;
        }

        ISegment originalTarget = targetSegments[0];
        IReadOnlyList<Field>? existingFields = GetFields(originalTarget);

        if (existingFields is null || _targetFieldIndex < 1 || _targetFieldIndex > existingFields.Count)
        {
            return message;
        }

        var newFields = existingFields.ToList();
        newFields[_targetFieldIndex - 1] = sourceFieldResult.Value;

        ISegment updatedTarget = WithFields(originalTarget, newFields.AsReadOnly());

        var newSegments = message.Segments
            .Select(s => ReferenceEquals(s, originalTarget) ? updatedTarget : s)
            .ToList()
            .AsReadOnly();

        return message with { Segments = newSegments };
    }

    private static IReadOnlyList<Field>? GetFields(ISegment segment) =>
        segment switch
        {
            Segment s => s.Fields,
            MshSegment msh => msh.Fields,
            _ => null,
        };

    private static ISegment WithFields(ISegment segment, IReadOnlyList<Field> fields) =>
        segment switch
        {
            Segment s => s with { Fields = fields },
            MshSegment msh => msh with { Fields = fields },
            _ => segment,
        };
}
