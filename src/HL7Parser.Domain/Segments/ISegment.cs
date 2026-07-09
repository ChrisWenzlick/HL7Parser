// <copyright file="ISegment.cs" company="Christopher Wenzlick">
// Copyright (c) Christopher Wenzlick. All rights reserved.
// </copyright>

using HL7Parser.Domain.Common;

namespace HL7Parser.Domain.Segments;

/// <summary>
/// Represents a single HL7 v2 segment.
/// </summary>
public interface ISegment
{
    /// <summary>
    /// Gets the segment type identifier (e.g. "MSH", "PID").
    /// </summary>
    public SegmentType SegmentType { get; }

    /// <summary>
    /// Returns the field at the specified one-based HL7 index.
    /// </summary>
    /// <param name="hl7Index">The one-based HL7 field index.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the field or an error message.
    /// </returns>
    public Result<Field> GetField(int hl7Index);

    /// <summary>
    /// Returns the HL7 string representation of this segment.
    /// </summary>
    /// <returns>The raw HL7 segment string.</returns>
    public string ToHl7String();
}
