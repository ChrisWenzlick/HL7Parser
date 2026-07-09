// <copyright file="MshSegment.cs" company="Christopher Wenzlick">
// Copyright (c) Christopher Wenzlick. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using HL7Parser.Domain.Common;

namespace HL7Parser.Domain.Segments;

/// <summary>
/// Represents the MSH (Message Header) segment of an HL7 v2 message.
/// MSH is structurally unique: MSH-1 is the field separator itself and
/// MSH-2 contains delimiter characters that cannot be parsed as a standard field.
/// </summary>
public sealed record MshSegment : ISegment
{
    /// <summary>
    /// Gets the encoding characters derived from MSH-1 and MSH-2.
    /// </summary>
    public EncodingCharacters EncodingCharacters { get; init; } = default!;

    /// <summary>
    /// Gets the fields contained in this segment, starting at MSH-1.
    /// </summary>
    public IReadOnlyList<Field> Fields { get; init; } = [];

    /// <inheritdoc/>
    public SegmentType SegmentType => throw new NotImplementedException();

    private MshSegment()
    {
    }

    /// <summary>
    /// Creates a new <see cref="MshSegment"/> from a raw HL7 MSH segment string.
    /// </summary>
    /// <param name="rawSegment">The raw HL7 MSH segment string.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the parsed segment or an error message.
    /// </returns>
    public static Result<MshSegment> Create(string rawSegment)
    {
        if (rawSegment is null)
        {
            throw new ArgumentNullException(nameof(rawSegment));
        }

        if (rawSegment.Length == 0)
        {
            return Result<MshSegment>.Failure($"{nameof(rawSegment)} must not be empty.");
        }

        if (rawSegment.Length < 4)
        {
            return Result<MshSegment>.Failure($"{nameof(rawSegment)} must contain MSH-1.");
        }

        var fieldSeparator = rawSegment[3];
        var segmentParts = rawSegment.Split(fieldSeparator);

        if (segmentParts[0] != "MSH")
        {
            return Result<MshSegment>.Failure($"{nameof(rawSegment)} must begin with 'MSH'.");
        }

        var rawFields = segmentParts.Skip(1).ToList();
        var finalizedFields = new List<Field>();

        // MSH-1: synthesize the field separator as a raw field.
        Result<Field> msh1Result = Field.CreateRaw(fieldSeparator.ToString());
        if (!msh1Result.IsSuccess)
        {
            return Result<MshSegment>.Failure($"Failed to create MSH-1: {msh1Result.Error}");
        }

        finalizedFields.Add(msh1Result.Value);

        // MSH-2: store encoding characters as a raw field without parsing.
        if (rawFields.Count == 0)
        {
            return Result<MshSegment>.Failure("MSH segment must contain MSH-2.");
        }

        Result<Field> msh2Result = Field.CreateRaw(rawFields[0]);
        if (!msh2Result.IsSuccess)
        {
            return Result<MshSegment>.Failure($"Failed to create MSH-2: {msh2Result.Error}");
        }

        finalizedFields.Add(msh2Result.Value);

        Result<EncodingCharacters> encodingCharactersResult =
            EncodingCharacters.Create(fieldSeparator.ToString() + msh2Result.Value.ToHl7String());
        if (!encodingCharactersResult.IsSuccess)
        {
            return Result<MshSegment>.Failure($"Invalid encoding characters: {encodingCharactersResult.Error}");
        }

        // Remaining fields parsed normally.
        for (var i = 1; i < rawFields.Count; i++)
        {
            Result<Field> fieldResult = Field.Create(rawFields[i], encodingCharactersResult.Value);
            if (!fieldResult.IsSuccess)
            {
                return Result<MshSegment>.Failure($"Failed to create {nameof(Field)} at index {i + 1}: {fieldResult.Error}");
            }

            finalizedFields.Add(fieldResult.Value);
        }

        return Result<MshSegment>.Success(new MshSegment
        {
            EncodingCharacters = encodingCharactersResult.Value,
            Fields = finalizedFields.AsReadOnly(),
        });
    }

    /// <inheritdoc/>
    public Result<Field> GetField(int hl7Index)
    {
        if (hl7Index < 1 || hl7Index > Fields.Count)
        {
            return Result<Field>.Failure($"Segment does not contain a field with an HL7 index of {hl7Index}.");
        }

        return Result<Field>.Success(Fields[hl7Index - 1]);
    }

    /// <inheritdoc/>
    public string ToHl7String()
    {
        // Skip MSH-1 when joining — the field separator handles it naturally.
        var fields = string.Join(
            EncodingCharacters.FieldSeparator.ToString(),
            Fields.Skip(1).Select(f => f.ToHl7String()));

        return "MSH" + EncodingCharacters.FieldSeparator + fields;
    }
}
