// <copyright file="IMessageParser.cs" company="Christopher Wenzlick">
// Copyright (c) Christopher Wenzlick. All rights reserved.
// </copyright>

using HL7Parser.Domain;
using HL7Parser.Domain.Common;

namespace HL7Parser.Application.UseCases;

/// <summary>
/// Parses a raw HL7 v2 message string into a <see cref="Message"/>.
/// </summary>
public interface IMessageParser
{
    /// <summary>
    /// Parses a raw HL7 v2 message string into a <see cref="Message"/>.
    /// </summary>
    /// <param name="rawMessage">The raw HL7 message string.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the parsed message or an error message.
    /// </returns>
    Result<Message> Execute(string rawMessage);
}
