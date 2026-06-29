// <copyright file="EncodingCharacters.cs" company="Christopher Wenzlick">
// Copyright (c) Christopher Wenzlick. All rights reserved.
// </copyright>

using System;
using System.Linq;

namespace HL7Parser.Domain.Common;

/// <summary>
/// The five HL7 encoding characters used to store and parse
/// data in HL7 v2 format, which are pulled from MSH-1 and
/// MSH-2. In order, these are the field separator,
/// component separator, repetition separator, escape
/// character, and subcomponent separator.
/// </summary>
public sealed record EncodingCharacters
{
    /// <summary>
    /// Gets the encoding character that separates each field in
    /// an HL7 v2 segment.
    /// </summary>
    public char FieldSeparator { get; init; }

    /// <summary>
    /// Gets the encoding character that separates each component
    /// in an HL7 v2 segment.
    /// </summary>
    public char ComponentSeparator { get; init; }

    /// <summary>
    /// Gets the encoding character that separates each repetition
    /// in an HL7 v2 segment.
    /// </summary>
    public char RepetitionSeparator { get; init; }

    /// <summary>
    /// Gets the encoding character that escapes special characters
    /// in an HL7 v2 segment.
    /// </summary>
    public char EscapeCharacter { get; init; }

    /// <summary>
    /// Gets the encoding character that separates each
    /// subcomponent in an HL7 v2 segment.
    /// </summary>
    public char SubcomponentSeparator { get; init; }

    private EncodingCharacters()
    {
    }

    /// <summary>
    /// Creates a new <see cref="EncodingCharacters"/> instance
    /// from the MSH-1 and MSH-2 strings of an HL7 v2 message.
    /// </summary>
    /// <param name="msh1">
    /// A string containing the field separator encoding character. This
    /// comes from MSH-1, the first character position after the "MSH" segment
    /// identifier in an HL7 v2 message.
    /// </param>
    /// <param name="msh2">
    /// A string containing the component separator, repetition separator,
    /// escape character, and subcomponent separator, in that order. These
    /// come from MSH-2, the four characters immediately following the field
    /// separator in an HL7 v2 message.
    /// </param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the encoding characters,
    /// or a failed <see cref="Result{T}"/> if the passed values are invalid.
    /// </returns>
    public static Result<EncodingCharacters> Create(string msh1, string msh2)
    {
        // Check for nulls to satisfy analyzer rule CA1062
        if (msh1 is null)
        {
            throw new ArgumentNullException(nameof(msh1));
        }

        if (msh2 is null)
        {
            throw new ArgumentNullException(nameof(msh2));
        }

        if (msh1.Length != 1)
        {
            return Result<EncodingCharacters>.Failure($"{nameof(msh1)} must be exactly 1 character.");
        }

        if (msh2.Length != 4)
        {
            return Result<EncodingCharacters>.Failure($"{nameof(msh2)} must be exactly 4 characters.");
        }

        var allCharacters = (msh1 + msh2).ToCharArray();
        if (allCharacters.Length != allCharacters.Distinct().Count())
        {
            return Result<EncodingCharacters>.Failure("All delimiter characters must be distinct.");
        }

        return Result<EncodingCharacters>.Success(new EncodingCharacters
        {
            FieldSeparator = msh1[0],
            ComponentSeparator = msh2[0],
            RepetitionSeparator = msh2[1],
            EscapeCharacter = msh2[2],
            SubcomponentSeparator = msh2[3],
        });
    }

    /// <summary>
    /// Creates a new <see cref="EncodingCharacters"/> instance
    /// from a single string containing both the MSH-1 and MSH-2
    /// strings of an HL7 v2 message.
    /// </summary>
    /// <param name="characterString">
    /// A string containing the field separator, component separator,
    /// repetition separator, escape character, and subcomponent
    /// separator, in that order. These are the first five characters
    /// after the "MSH" segment identifier, and make up MSH-1 and MSH-2.
    /// </param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the encoding characters,
    /// or a failed <see cref="Result{T}"/> if the passed value is invalid.
    /// </returns>
    public static Result<EncodingCharacters> Create(string characterString)
    {
        // Check for null to satisfy analyzer rule CA1062
        if (characterString is null)
        {
            throw new ArgumentNullException(nameof(characterString));
        }

        if (characterString.Length != 5)
        {
            return Result<EncodingCharacters>.Failure($"{nameof(characterString)} must be exactly 5 characters.");
        }

        return Create(characterString.Substring(0, 1), characterString.Substring(1));
    }

    /// <summary>
    /// Gets the raw HL7 text of the <see cref="EncodingCharacters"/>
    /// with no parsing or formatting applied.
    /// </summary>
    /// <returns>
    /// The HL7 string value of the <see cref="EncodingCharacters"/>.
    /// </returns>
    public string ToHl7String() =>
        string.Concat(FieldSeparator, ComponentSeparator, RepetitionSeparator, EscapeCharacter, SubcomponentSeparator);
}
