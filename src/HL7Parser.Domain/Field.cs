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
    private const char RepetitionDelimiter = '~';

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

    private Field(IReadOnlyList<Repetition> repetitions)
    {
        Repetitions = repetitions;
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
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the field,
    /// or a failed <see cref="Result{T}"/> if the value is invalid.
    /// </returns>
    public static Result<Field> Create(string? rawValue)
    {
        if (rawValue is null)
        {
            return Result<Field>.Failure($"{nameof(rawValue)} cannot be null.");
        }

        var repetitionValues = rawValue.Split(RepetitionDelimiter);
        var repetitions = new List<Repetition>();
        for (var i = 0; i < repetitionValues.Length; i++)
        {
            Result<Repetition> repetitionResult = Repetition.Create(repetitionValues[i]);
            if (!repetitionResult.IsSuccess)
            {
                // Index is zero-based to match the Repetitions collection access
                var error = $"Failed to create Repetition at index {i}: {repetitionResult.Error}";
                return Result<Field>.Failure(error);
            }

            repetitions.Add(repetitionResult.Value);
        }

        return Result<Field>.Success(new Field(repetitions.AsReadOnly()));
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
            RepetitionDelimiter.ToString(),
            Repetitions.Select(s => s.ToHl7String()));

    // NOTE: Equality/hashing logic is duplicated across Component, Repetition,
    // and Field. Candidate for extraction to a shared internal helper —
    // deferred deliberately to avoid introducing inheritance into a hierarchy
    // designed around sealed, independently-constructed value objects.

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
    public bool Equals(Field? other) =>
    other is not null && Repetitions.SequenceEqual(other.Repetitions);

    /// <summary>
    /// Returns a hash code computed from this field's <see cref="Repetitions"/>,
    /// consistent with the sequence-based equality defined in <see cref="Equals(Field?)"/>.
    /// </summary>
    /// <returns>A hash code for this field.</returns>
    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            foreach (Repetition repetition in Repetitions)
            {
                hash = (hash * 31) + (repetition?.GetHashCode() ?? 0);
            }

            return hash;
        }
    }
}
