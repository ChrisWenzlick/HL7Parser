// <copyright file="Segment.cs" company="Christopher Wenzlick">
// Copyright (c) Christopher Wenzlick. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using HL7Parser.Domain.Common;

namespace HL7Parser.Domain.Segments;

/// <summary>
/// Represents a single HL7 v2 segment.
/// </summary>
public sealed record Segment
{
    /// <summary>
    /// Gets the segment type identifier (e.g. "MSH", "PID").
    /// </summary>
    public SegmentType SegmentType { get; init; } = default!;

    /// <summary>
    /// Gets the fields contained in this segment.
    /// </summary>
    public IReadOnlyList<Field> Fields { get; init; } = [];

    /// <summary>
    /// Gets the encoding characters used to parse this segment.
    /// </summary>
    public EncodingCharacters EncodingCharacters { get; init; } = default!;

    private Segment()
    {
    }

    /// <summary>
    /// Creates a new <see cref="Segment"/> from a raw HL7 segment string.
    /// </summary>
    /// <param name="rawSegment">The raw HL7 segment string.</param>
    /// <param name="encodingCharacters">The encoding characters to use for parsing.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the parsed segment or an error message.
    /// </returns>
    public static Result<Segment> Create(string rawSegment, EncodingCharacters encodingCharacters)
    {
        if (rawSegment is null)
        {
            throw new ArgumentNullException(nameof(rawSegment));
        }

        if (encodingCharacters is null)
        {
            throw new ArgumentNullException(nameof(encodingCharacters));
        }

        if (rawSegment.Length == 0)
        {
            return Result<Segment>.Failure($"{nameof(rawSegment)} must not be empty.");
        }

        var segmentParts = rawSegment.Split(encodingCharacters.FieldSeparator);
        Result<SegmentType> segmentTypeResult = SegmentType.Create(segmentParts[0]);

        if (!segmentTypeResult.IsSuccess)
        {
            return Result<Segment>.Failure($"Failed to create {nameof(SegmentType)}: {segmentTypeResult.Error}");
        }

        var rawFields = segmentParts.Skip(1).ToList();
        var finalizedFields = new List<Field>();

        for (var i = 0; i < rawFields.Count; i++)
        {
            Result<Field> fieldResult = Field.Create(rawFields[i], encodingCharacters);
            if (!fieldResult.IsSuccess)
            {
                return Result<Segment>.Failure($"Failed to create {nameof(Field)} at index {i}: {fieldResult.Error}");
            }

            finalizedFields.Add(fieldResult.Value);
        }

        return Result<Segment>.Success(new Segment
        {
            SegmentType = segmentTypeResult.Value,
            EncodingCharacters = encodingCharacters,
            Fields = finalizedFields.AsReadOnly(),
        });
    }

    /// <summary>
    /// Returns the field at the specified one-based HL7 index.
    /// </summary>
    /// <param name="hl7Index">The one-based HL7 field index.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the field or an error message.
    /// </returns>
    public Result<Field> GetField(int hl7Index)
    {
        if (hl7Index < 1 || hl7Index > Fields.Count)
        {
            return Result<Field>.Failure($"Segment does not contain a field with an HL7 index of {hl7Index}.");
        }

        Field retrievedField = Fields[hl7Index - 1];
        return Result<Field>.Success(retrievedField);
    }

    /// <summary>
    /// Returns the HL7 string representation of this segment.
    /// </summary>
    /// <returns>The raw HL7 segment string.</returns>
    public string ToHl7String()
    {
        IEnumerable<Field> fieldsToJoin = SegmentType.Identifier == "MSH"
            ? Fields.Skip(1)
            : Fields;

        var fields = string.Join(
            EncodingCharacters.FieldSeparator.ToString(),
            fieldsToJoin.Select(field => field.ToHl7String()));

        return SegmentType.Identifier + EncodingCharacters.FieldSeparator + fields;
    }
}
