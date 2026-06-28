// <copyright file="SegmentType.cs" company="Christopher Wenzlick">
// Copyright (c) Christopher Wenzlick. All rights reserved.
// </copyright>

using System;
using System.Linq;
using HL7Parser.Domain.Common;

namespace HL7Parser.Domain.Segments;

/// <summary>
/// Represents the three-character alphanumeric segment
/// identifier for an HL7 v2 segment.
/// </summary>
public sealed record SegmentType
{
    /// <summary>
    /// Gets the HL7 v2 segment identifier.
    /// </summary>
    public string Identifier { get; init; } = string.Empty;

    private SegmentType()
    {
    }

    /// <summary>
    /// Creates a new <see cref="SegmentType"/> from the specified value.
    /// </summary>
    /// <param name="value">
    /// The three-character alphanumeric string identifier of the <see cref="SegmentType"/>.
    /// </param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the validated segment type
    /// identifier, or a failed <see cref="Result{T}"/> if the value is invalid.
    /// </returns>
    public static Result<SegmentType> Create(string value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (value.Length != 3)
        {
            return Result<SegmentType>.Failure($"{nameof(value)} must be exactly 3 characters.");
        }

        if (!value.All(c => char.IsUpper(c) || char.IsDigit(c)))
        {
            return Result<SegmentType>.Failure($"{nameof(value)} must contain only uppercase letters and digits.");
        }

        return Result<SegmentType>.Success(new SegmentType { Identifier = value });
    }
}
