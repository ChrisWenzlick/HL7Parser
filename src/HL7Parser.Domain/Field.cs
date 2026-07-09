// <copyright file="Field.cs" company="Christopher Wenzlick">
// Copyright (c) Christopher Wenzlick. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using HL7Parser.Domain.Common;

namespace HL7Parser.Domain;

/// <summary>
/// Represents a field within an HL7 v2 message.
/// A field contains one or more <see cref="Repetition"/> instances
/// separated by the repetition delimiter <c>~</c>. A field is a
/// specific, individual piece of information within an HL7 v2
/// message segment.
/// </summary>
public sealed record Field
{
    private readonly char _repetitionSeparator;

    /// <summary>
    /// Gets the ordered list of <see cref="Repetition"/> instances
    /// that make up the field. The order reflects the original
    /// message structure. Empty repetitions are preserved.
    /// </summary>
    public IReadOnlyList<Repetition> Repetitions { get; init; } = [];

    /// <summary>
    /// Gets the raw value of this field, if created without
    /// parsing internal structure.
    /// </summary>
    public string? RawValue { get; init; }

    private Field(IReadOnlyList<Repetition> repetitions, char repetitionSeparator)
    {
        Repetitions = repetitions;
        _repetitionSeparator = repetitionSeparator;
    }

    private Field(string rawValue)
    {
        RawValue = rawValue;
    }

    /// <summary>
    /// Creates a new <see cref="Field"/> from the specified value.
    /// </summary>
    /// <param name="rawValue">
    /// The raw string value to parse into a <see cref="Field"/>.
    /// May contain the repetition delimiter <c>~</c> to separate
    /// multiple repetitions. Cannot be <see langword="null"/>.
    /// </param>
    /// <param name="encodingCharacters">
    /// The encoding characters used to parse the data.
    /// </param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the field,
    /// or a failed <see cref="Result{T}"/> if the value is invalid.
    /// </returns>
    public static Result<Field> Create(string rawValue, EncodingCharacters encodingCharacters)
    {
        if (rawValue is null)
        {
            throw new ArgumentNullException(nameof(rawValue));
        }

        if (encodingCharacters is null)
        {
            return Result<Field>.Failure($"{nameof(encodingCharacters)} cannot be null.");
        }

        var repetitionSeparator = encodingCharacters.RepetitionSeparator;

        var repetitionValues = rawValue.Split(repetitionSeparator);
        var repetitions = new List<Repetition>();
        for (var i = 0; i < repetitionValues.Length; i++)
        {
            Result<Repetition> repetitionResult = Repetition.Create(repetitionValues[i], encodingCharacters);
            if (!repetitionResult.IsSuccess)
            {
                // Index is zero-based to match the Repetitions collection access
                var error = $"Failed to create Repetition at index {i}: {repetitionResult.Error}";
                return Result<Field>.Failure(error);
            }

            repetitions.Add(repetitionResult.Value);
        }

        return Result<Field>.Success(new Field(repetitions.AsReadOnly(), repetitionSeparator));
    }

    /// <summary>
    /// Creates a new <see cref="Field"/> from a raw value without parsing
    /// internal structure. This method is intended exclusively for MSH-1
    /// and MSH-2, which contain separator characters that cannot be parsed
    /// through the standard construction path.
    /// </summary>
    /// <param name="rawValue">The raw field value.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the field or an error message.
    /// </returns>
    public static Result<Field> CreateRaw(string rawValue)
    {
        if (rawValue is null)
        {
            throw new ArgumentNullException(nameof(rawValue));
        }

        return Result<Field>.Success(new Field(rawValue));
    }

    /// <summary>
    /// Gets the raw HL7 text of the <see cref="Field"/> with no
    /// parsing or formatting applied.
    /// </summary>
    /// <returns>
    /// The HL7 string value of the <see cref="Field"/>.
    /// </returns>
    public string ToHl7String() =>
        RawValue ?? string.Join(
            _repetitionSeparator.ToString(),
            Repetitions.Select(s => s.ToHl7String()));

    /// <summary>
    /// Determines whether this <see cref="Field"/> is equal to another by
    /// comparing their <see cref="Repetitions"/> collections element-by-element.
    /// Two fields with structurally identical repetitions in the same
    /// order are considered equal, regardless of reference identity.
    /// </summary>
    /// <param name="other">The field to compare against.</param>
    /// <returns>
    /// <see langword="true"/> if both fields contain equal repetitions
    /// in the same order; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(Field? other)
    {
        if (other is null)
        {
            return false;
        }

        return CollectionEquality.SequenceEqual(Repetitions, other.Repetitions);
    }

    /// <summary>
    /// Returns a hash code computed from this field's <see cref="Repetitions"/>,
    /// consistent with the sequence-based equality defined in <see cref="Equals(Field?)"/>.
    /// </summary>
    /// <returns>A hash code for this field.</returns>
    public override int GetHashCode()
        => CollectionEquality.GetHashCode(Repetitions);
}
