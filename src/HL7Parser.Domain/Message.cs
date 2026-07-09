// <copyright file="Message.cs" company="Christopher Wenzlick">
// Copyright (c) Christopher Wenzlick. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using HL7Parser.Domain.Common;
using HL7Parser.Domain.Segments;

namespace HL7Parser.Domain;

/// <summary>
/// Represents a complete HL7 v2 message.
/// </summary>
public sealed record Message
{
    /// <summary>
    /// Gets all segments in this message, including the MSH segment.
    /// </summary>
    public IReadOnlyList<ISegment> Segments { get; init; } = [];

    /// <summary>
    /// Gets the MSH segment of this message.
    /// </summary>
    public MshSegment Msh => (MshSegment)Segments[0];

    private string LineTerminator { get; init; } = "\r";
    private bool HasTrailingTerminator { get; init; }

    private Message()
    {
    }

    /// <summary>
    /// Creates a new <see cref="Message"/> from a raw HL7 message string.
    /// </summary>
    /// <param name="rawMessage">The raw HL7 message string.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the parsed message or an error message.
    /// </returns>
    public static Result<Message> Create(string rawMessage)
    {
        if (rawMessage is null)
        {
            throw new ArgumentNullException(nameof(rawMessage));
        }

        if (rawMessage.Length == 0)
        {
            return Result<Message>.Failure($"{nameof(rawMessage)} must not be empty.");
        }

        var lineTerminator = DetectLineTerminator(rawMessage);
        var hasTrailingTerminator = rawMessage.EndsWith(lineTerminator);

        var trimmedMessage = hasTrailingTerminator
            ? rawMessage.TrimEnd(lineTerminator.ToCharArray())
            : rawMessage;
        var rawSegments = trimmedMessage.Split([lineTerminator], StringSplitOptions.None);

        if (rawSegments.Any(segment => segment.Length == 0))
        {
            return Result<Message>.Failure($"{nameof(rawMessage)} must not contain empty lines.");
        }

        // Remove trailing terminator if present.
        if (rawSegments[rawSegments.Length - 1].Length == 0)
        {
            rawSegments = rawSegments.Take(rawSegments.Length - 1).ToArray();
        }

        if (!rawSegments[0].StartsWith("MSH"))
        {
            return Result<Message>.Failure("First segment must be MSH.");
        }

        Result<MshSegment> mshResult = MshSegment.Create(rawSegments[0]);
        if (!mshResult.IsSuccess)
        {
            return Result<Message>.Failure($"Failed to create MSH segment: {mshResult.Error}");
        }

        var segments = new List<ISegment> { mshResult.Value };

        for (var i = 1; i < rawSegments.Length; i++)
        {
            Result<Segment> segmentResult = Segment.Create(rawSegments[i], mshResult.Value.EncodingCharacters);
            if (!segmentResult.IsSuccess)
            {
                return Result<Message>.Failure($"Failed to create segment at index {i}: {segmentResult.Error}");
            }

            segments.Add(segmentResult.Value);
        }

        return Result<Message>.Success(new Message
        {
            Segments = segments.AsReadOnly(),
            LineTerminator = lineTerminator,
            HasTrailingTerminator = hasTrailingTerminator,
        });
    }

    /// <summary>
    /// Returns the HL7 string representation of this message.
    /// </summary>
    /// <returns>The raw HL7 message string.</returns>
    public string ToHl7String()
    {
        var segments = string.Join(
            LineTerminator,
            Segments.Select(s => s.ToHl7String()));

        return HasTrailingTerminator
            ? segments + LineTerminator
            : segments;
    }

    private static string DetectLineTerminator(string rawMessage)
    {
        if (rawMessage.Contains("\r\n"))
        {
            return "\r\n";
        }

        if (rawMessage.Contains("\r"))
        {
            return "\r";
        }

        return "\n";
    }
}
