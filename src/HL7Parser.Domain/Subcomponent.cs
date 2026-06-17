// <copyright file="Subcomponent.cs" company="Christopher Wenzlick">
// Copyright (c) Christopher Wenzlick. All rights reserved.
// </copyright>

using HL7Parser.Domain.Common;

namespace HL7Parser.Domain;

/// <summary>
/// Represents the most granular unit of data in an HL7 v2 message.
/// A subcomponent contains a raw string value and guarantees that value
/// does not contain HL7 delimiter characters.
/// </summary>
public sealed record Subcomponent
{
    private static readonly char[] Hl7Delimiters = { '|', '^', '&', '~', '\\' };

    /// <summary>
    /// Gets the raw HL7 text of the <see cref="Subcomponent"/> with no
    /// parsing or formatting applied.
    /// </summary>
    public string RawValue { get; }

    private Subcomponent(string rawValue)
    {
        RawValue = rawValue;
    }

    /// <summary>
    /// Creates a new <see cref="Subcomponent"/> with the specified value.
    /// </summary>
    /// <param name="rawValue">
    /// The raw string value of the <see cref="Subcomponent"/> with no parsing
    /// or formatting applied.
    /// </param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the subcomponent,
    /// or a failed <see cref="Result{T}"/> if the value is invalid.
    /// </returns>
    public static Result<Subcomponent> Create(string? rawValue)
    {
        if (rawValue is null)
        {
            return Result<Subcomponent>.Failure($"{nameof(rawValue)} cannot be null.");
        }

        var delimiterIndex = rawValue.IndexOfAny(Hl7Delimiters);
        if (delimiterIndex != -1)
        {
            var offendingCharacter = rawValue[delimiterIndex];
            var error = $"Subcomponent value cannot contain '{offendingCharacter}' as it is a reserved character.";
            return Result<Subcomponent>.Failure(error);
        }

        return Result<Subcomponent>.Success(new Subcomponent(rawValue));
    }

    /// <summary>
    /// Gets the raw HL7 text of the <see cref="Subcomponent"/> with no
    /// parsing or formatting applied.
    /// </summary>
    /// <returns>
    /// The HL7 string value of the <see cref="Subcomponent"/>.
    /// </returns>
    public string ToHl7String() => RawValue;
}
