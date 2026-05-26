// <copyright file="Component.cs" company="Christopher Wenzlick">
// Copyright (c) Christopher Wenzlick. All rights reserved.
// </copyright>

using System.Collections.Generic;
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
    private const char SubcomponentDelimiter = '&';

    /// <summary>
    /// Gets the ordered list of <see cref="Subcomponent"/> instances
    /// that make up the component. The order reflects the original
    /// message structure. Empty subcomponents are preserved.
    /// </summary>
    public IReadOnlyList<Subcomponent> Subcomponents { get; }

    private Component(IReadOnlyList<Subcomponent> subcomponents)
    {
        Subcomponents = subcomponents;
    }

    /// <summary>
    /// Creates a new <see cref="Component"/> from the specified value.
    /// </summary>
    /// <param name="rawValue">
    /// The raw string value to parse into a <see cref="Component"/>.
    /// May contain the subcomponent delimiter <c>&amp;</c> to separate
    /// multiple subcomponents. Cannot be <see langword="null"/>.
    /// </param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the component,
    /// or a failed <see cref="Result{T}"/> if the value is invalid.
    /// </returns>
    public static Result<Component> Create(string? rawValue)
    {
        if (rawValue is null)
        {
            return Result<Component>.Failure($"{nameof(rawValue)} cannot be null.");
        }

        var subcomponentValues = rawValue.Split(SubcomponentDelimiter);
        var subcomponents = new List<Subcomponent>();
        for (var i = 0; i < subcomponentValues.Length; i++)
        {
            Result<Subcomponent> subcomponentResult = Subcomponent.Create(subcomponentValues[i]);
            if (!subcomponentResult.IsSuccess)
            {
                // Index is zero-based to match the Subcomponents collection access
                var error = $"Failed to create Subcomponent at index {i}: {subcomponentResult.Error}";
                return Result<Component>.Failure(error);
            }

            subcomponents.Add(subcomponentResult.Value);
        }

        return Result<Component>.Success(new Component(subcomponents.AsReadOnly()));
    }
}
