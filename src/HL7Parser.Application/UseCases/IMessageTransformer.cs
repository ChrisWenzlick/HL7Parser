// <copyright file="IMessageTransformer.cs" company="Christopher Wenzlick">
// Copyright (c) Christopher Wenzlick. All rights reserved.
// </copyright>

using HL7Parser.Domain;

namespace HL7Parser.Application.UseCases;

/// <summary>
/// Applies an ordered sequence of transformation rules to a parsed <see cref="Message"/>.
/// </summary>
public interface IMessageTransformer
{
    /// <summary>
    /// Applies the configured transformation rules to <paramref name="message"/> in order,
    /// threading each rule's output into the next.
    /// </summary>
    /// <param name="message">The message to transform. Cannot be <see langword="null"/>.</param>
    /// <returns>The message produced after all rules have been applied.</returns>
    Message Execute(Message message);
}
