// <copyright file="MessageValidator.cs" company="Christopher Wenzlick">
// Copyright (c) Christopher Wenzlick. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using HL7Parser.Application.Validation;
using HL7Parser.Application.Validation.Rules;
using HL7Parser.Domain;

namespace HL7Parser.Application.UseCases;

/// <summary>
/// Validates a parsed <see cref="Message"/> by running a configurable set of
/// <see cref="IConformanceRule"/> instances and aggregating their issues.
/// </summary>
public sealed class MessageValidator : IMessageValidator
{
    private static readonly IReadOnlyList<IConformanceRule> DefaultRules =
        [new RequiredMshFieldsRule(), new OptionalMshFieldsRule(), new MessageTypeSegmentRequirementsRule()];

    private readonly IReadOnlyList<IConformanceRule> _rules;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageValidator"/> class with the default
    /// conformance rules: <see cref="RequiredMshFieldsRule"/>.
    /// </summary>
    public MessageValidator()
        : this(DefaultRules)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageValidator"/> class with the specified
    /// conformance rules.
    /// </summary>
    /// <param name="rules">The rules to evaluate. Cannot be <see langword="null"/>.</param>
    public MessageValidator(IReadOnlyList<IConformanceRule> rules)
    {
        if (rules is null)
        {
            throw new ArgumentNullException(nameof(rules));
        }

        _rules = rules;
    }

    /// <summary>
    /// Validates a parsed <see cref="Message"/> against the configured conformance rules.
    /// </summary>
    /// <param name="message">The parsed message to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> describing any conformance issues found.</returns>
    public ValidationResult Execute(Message message)
    {
        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        var issues = new List<ValidationIssue>();

        foreach (IConformanceRule rule in _rules)
        {
            if (rule.Applies(message))
            {
                issues.AddRange(rule.Evaluate(message));
            }
        }

        return ValidationResult.Create(issues);
    }
}
