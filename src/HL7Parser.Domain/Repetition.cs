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

    // NOTE: Equality/hashing logic is duplicated across Component, Repetition,
    // and Field. Candidate for extraction to a shared internal helper —
    // deferred deliberately to avoid introducing inheritance into a hierarchy
    // designed around sealed, independently-constructed value objects.

    /// <summary>
    /// Determines whether this <see cref="Repetition"/> is equal to another by
    /// comparing their <see cref="Components"/> collections element-by-element.
    /// Two repetitions with structurally identical components in the same
    /// order are considered equal, regardless of reference identity.
    /// </summary>
    /// <param name="other">The repetition to compare against.</param>
    /// <returns>
    /// <see langword="true"/> if both repetitions contain equal components
    /// in the same order; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(Repetition? other) =>
    other is not null && Components.SequenceEqual(other.Components);

    /// <summary>
    /// Returns a hash code computed from this repetition's <see cref="Components"/>,
    /// consistent with the sequence-based equality defined in <see cref="Equals(Repetition?)"/>.
    /// </summary>
    /// <returns>A hash code for this repetition.</returns>
    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            foreach (Component component in Components)
            {
                hash = (hash * 31) + (component?.GetHashCode() ?? 0);
            }

            return hash;
        }
    }
}
