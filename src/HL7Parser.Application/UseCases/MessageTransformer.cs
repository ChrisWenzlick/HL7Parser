// <copyright file="MessageTransformer.cs" company="Christopher Wenzlick">
// Copyright (c) Christopher Wenzlick. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using HL7Parser.Application.Transformation;
using HL7Parser.Domain;

namespace HL7Parser.Application.UseCases;

/// <summary>
/// Applies an ordered sequence of <see cref="ITransformRule"/> instances to a
/// <see cref="Message"/>, threading each rule's output into the next.
/// </summary>
public sealed class MessageTransformer : IMessageTransformer
{
    private readonly IReadOnlyList<ITransformRule> _rules;

    /// <summary>
    /// Initializes a new instance of the <see cref="MessageTransformer"/> class with the
    /// specified transformation rules.
    /// </summary>
    /// <param name="rules">The ordered rules to apply. Cannot be <see langword="null"/>.</param>
    public MessageTransformer(IReadOnlyList<ITransformRule> rules)
    {
        if (rules is null)
        {
            throw new ArgumentNullException(nameof(rules));
        }

        _rules = rules;
    }

    /// <inheritdoc/>
    public Message Execute(Message message)
    {
        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        var current = message;

        foreach (ITransformRule rule in _rules)
        {
            current = rule.Apply(current);
        }

        return current;
    }
}
