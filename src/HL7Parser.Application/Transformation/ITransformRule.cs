// <copyright file="ITransformRule.cs" company="Christopher Wenzlick">
// Copyright (c) Christopher Wenzlick. All rights reserved.
// </copyright>

using HL7Parser.Domain;

namespace HL7Parser.Application.Transformation;

/// <summary>
/// Represents a single transformation rule that produces a new <see cref="Message"/>
/// from an input <see cref="Message"/>. Implementations must not mutate the input.
/// </summary>
public interface ITransformRule
{
    /// <summary>
    /// Applies this rule to <paramref name="message"/> and returns the resulting message.
    /// </summary>
    /// <param name="message">The message to transform. Cannot be <see langword="null"/>.</param>
    /// <returns>
    /// A new <see cref="Message"/> reflecting this rule's changes, or the original
    /// <paramref name="message"/> if the rule does not apply.
    /// </returns>
    Message Apply(Message message);
}
