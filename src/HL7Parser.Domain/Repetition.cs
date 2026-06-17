// <copyright file="Repetition.cs" company="Christopher Wenzlick">
// Copyright (c) Christopher Wenzlick. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using HL7Parser.Domain.Common;

namespace HL7Parser.Domain;

/// <summary>
/// Represents a repetition within an HL7 v2 message field.
/// A repetition contains one or more <see cref="Component"/> instances
/// separated by the component delimiter <c>^</c>. Repetitions are
/// the third level of granularity within a field, above components.
/// </summary>
public sealed record Repetition
{
    private const char ComponentDelimiter = '^';

    /// <summary>
    /// Gets the ordered list of <see cref="Component"/> instances
    /// that make up the repetition. The order reflects the original
    /// message structure. Empty components are preserved.
    /// </summary>
    public IReadOnlyList<Component> Components { get; }

    private Repetition(IReadOnlyList<Component> components)
    {
        Components = components;
    }

    /// <summary>
    /// Creates a new <see cref="Repetition"/> from the specified value.
    /// </summary>
    /// <param name="rawValue">
    /// The raw string value to parse into a <see cref="Repetition"/>.
    /// May contain the component delimiter <c>^</c> to separate
    /// multiple components. Cannot be <see langword="null"/>.
    /// </param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the repetition,
    /// or a failed <see cref="Result{T}"/> if the value is invalid.
    /// </returns>
    public static Result<Repetition> Create(string? rawValue)
    {
        if (rawValue is null)
        {
            return Result<Repetition>.Failure($"{nameof(rawValue)} cannot be null.");
        }

        var componentValues = rawValue.Split(ComponentDelimiter);
        var components = new List<Component>();
        for (var i = 0; i < componentValues.Length; i++)
        {
            Result<Component> componentResult = Component.Create(componentValues[i]);
            if (!componentResult.IsSuccess)
            {
                // Index is zero-based to match the Components collection access
                var error = $"Failed to create Component at index {i}: {componentResult.Error}";
                return Result<Repetition>.Failure(error);
            }

            components.Add(componentResult.Value);
        }

        return Result<Repetition>.Success(new Repetition(components.AsReadOnly()));
    }

    /// <summary>
    /// Gets the raw HL7 text of the <see cref="Repetition"/> with no
    /// parsing or formatting applied.
    /// </summary>
    /// <returns>
    /// The HL7 string value of the <see cref="Repetition"/>.
    /// </returns>
    public string ToHl7String() =>
    string.Join(ComponentDelimiter.ToString(), Components.Select(s => s.ToHl7String()));
}
