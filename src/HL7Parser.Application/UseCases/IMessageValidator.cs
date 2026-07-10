// <copyright file="IMessageValidator.cs" company="Christopher Wenzlick">
// Copyright (c) Christopher Wenzlick. All rights reserved.
// </copyright>

using HL7Parser.Application.Validation;
using HL7Parser.Domain;

namespace HL7Parser.Application.UseCases;

/// <summary>
/// Validates a parsed <see cref="Message"/> against a set of HL7 v2 conformance rules.
/// </summary>
public interface IMessageValidator
{
    /// <summary>
    /// Validates a parsed <see cref="Message"/> against a set of HL7 v2 conformance rules.
    /// </summary>
    /// <param name="message">The parsed message to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> describing any conformance issues found.</returns>
    ValidationResult Execute(Message message);
}
