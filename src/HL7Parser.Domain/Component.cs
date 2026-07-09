// <copyright file="Component.cs" company="Christopher Wenzlick">
// Copyright (c) Christopher Wenzlick. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using HL7Parser.Domain.Common;

namespace HL7Parser.Domain;

/// <summary>
/// Represents a component within an HL7 v2 message field.
/// A component contains one or more <see cref="Subcomponent"/> instances
/// separated by the subcomponent delimiter <c>&amp;</c>. Components are
/// the second level of granularity within a field, above subcomponents.
/// </summary>
public sealed record Component
{
    private readonly char _subcomponentSeparator;

    /// <summary>
    /// Gets the ordered list of <see cref="Subcomponent"/> instances
    /// that make up the component. The order reflects the original
    /// message structure. Empty subcomponents are preserved.
    /// </summary>
    public IReadOnlyList<Subcomponent> Subcomponents { get; init; } = [];

    private Component(IReadOnlyList<Subcomponent> subcomponents, char subcomponentSeparator)
    {
        Subcomponents = subcomponents;
        _subcomponentSeparator = subcomponentSeparator;
    }

    /// <summary>
    /// Creates a new <see cref="Component"/> from the specified value.
    /// </summary>
    /// <param name="rawValue">
    /// The raw string value to parse into a <see cref="Component"/>.
    /// May contain the subcomponent delimiter <c>&amp;</c> to separate
    /// multiple subcomponents. Cannot be <see langword="null"/>.
    /// </param>
    /// <param name="encodingCharacters">
    /// The encoding characters used to parse the data.
    /// </param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the component,
    /// or a failed <see cref="Result{T}"/> if the value is invalid.
    /// </returns>
    public static Result<Component> Create(string rawValue, EncodingCharacters encodingCharacters)
    {
        if (rawValue is null)
        {
            throw new ArgumentNullException(nameof(rawValue));
        }

        if (encodingCharacters is null)
        {
            return Result<Component>.Failure($"{nameof(encodingCharacters)} cannot be null.");
        }

        var subcomponentSeparator = encodingCharacters.SubcomponentSeparator;

        var subcomponentValues = rawValue.Split(subcomponentSeparator);
        var subcomponents = new List<Subcomponent>();
        for (var i = 0; i < subcomponentValues.Length; i++)
        {
            Result<Subcomponent> subcomponentResult = Subcomponent.Create(subcomponentValues[i], encodingCharacters);
            if (!subcomponentResult.IsSuccess)
            {
                // Index is zero-based to match the Subcomponents collection access
                var error = $"Failed to create Subcomponent at index {i}: {subcomponentResult.Error}";
                return Result<Component>.Failure(error);
            }

            subcomponents.Add(subcomponentResult.Value);
        }

        return Result<Component>.Success(new Component(subcomponents.AsReadOnly(), subcomponentSeparator));
    }

    /// <summary>
    /// Gets the raw HL7 text of the <see cref="Component"/> with no
    /// parsing or formatting applied.
    /// </summary>
    /// <returns>
    /// The HL7 string value of the <see cref="Component"/>.
    /// </returns>
    public string ToHl7String() =>
        string.Join(
            _subcomponentSeparator.ToString(),
            Subcomponents.Select(s => s.ToHl7String()));

    // NOTE: Equality/hashing logic is duplicated across Component, Repetition,
    // and Field. Candidate for extraction to a shared internal helper —
    // deferred deliberately to avoid introducing inheritance into a hierarchy
    // designed around sealed, independently-constructed value objects.

    /// <summary>
    /// Determines whether this <see cref="Component"/> is equal to another by
    /// comparing their <see cref="Subcomponents"/> collections element-by-element.
    /// Two components with structurally identical subcomponents in the same
    /// order are considered equal, regardless of reference identity.
    /// </summary>
    /// <param name="other">The component to compare against.</param>
    /// <returns>
    /// <see langword="true"/> if both components contain equal subcomponents
    /// in the same order; otherwise, <see langword="false"/>.
    /// </returns>
    public bool Equals(Component? other) =>
        other is not null && Subcomponents.SequenceEqual(other.Subcomponents);

    /// <summary>
    /// Returns a hash code computed from this component's <see cref="Subcomponents"/>,
    /// consistent with the sequence-based equality defined in <see cref="Equals(Component?)"/>.
    /// </summary>
    /// <returns>A hash code for this component.</returns>
    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            foreach (Subcomponent subcomponent in Subcomponents)
            {
                hash = (hash * 31) + (subcomponent?.GetHashCode() ?? 0);
            }

            return hash;
        }
    }
}
