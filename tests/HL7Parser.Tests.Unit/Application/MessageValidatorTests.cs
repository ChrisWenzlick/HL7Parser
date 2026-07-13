// Copyright (c) Christopher Wenzlick. All rights reserved.

using System.Collections.Generic;
using System.Linq;
using HL7Parser.Application.UseCases;
using HL7Parser.Application.Validation;
using HL7Parser.Domain;
using HL7Parser.Domain.Common;

namespace HL7Parser.Tests.Unit.Application;

public class MessageValidatorTests
{
    private const string FullyPopulatedMsh =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ADT^A01|MSG00001|P|2.5\r" +
        "PID|1||123456^^^MRN||DOE^JOHN";

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenRulesIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new MessageValidator(null!));
    }

    [Fact]
    public void Execute_SkipsRule_WhenAppliesReturnsFalse()
    {
        var issue = new ValidationIssue(ValidationSeverity.Error, "LOC", "CODE", "desc");
        var rule = new StubRule(applies: false, issues: [issue]);
        var validator = new MessageValidator([rule]);
        var message = CreateMessage(FullyPopulatedMsh);

        ValidationResult result = validator.Execute(message);

        Assert.True(result.IsValid);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void Execute_IncludesIssues_WhenAppliesReturnsTrue()
    {
        var issue = new ValidationIssue(ValidationSeverity.Error, "LOC", "CODE", "desc");
        var rule = new StubRule(applies: true, issues: [issue]);
        var validator = new MessageValidator([rule]);
        var message = CreateMessage(FullyPopulatedMsh);

        ValidationResult result = validator.Execute(message);

        Assert.False(result.IsValid);
        Assert.Single(result.Issues);
    }

    [Fact]
    public void Execute_AggregatesIssues_AcrossMultipleApplicableRules()
    {
        var issue1 = new ValidationIssue(ValidationSeverity.Error, "LOC1", "CODE1", "desc1");
        var issue2 = new ValidationIssue(ValidationSeverity.Error, "LOC2", "CODE2", "desc2");
        var validator = new MessageValidator(
        [
            new StubRule(applies: true, issues: [issue1]),
            new StubRule(applies: true, issues: [issue2]),
        ]);
        var message = CreateMessage(FullyPopulatedMsh);

        ValidationResult result = validator.Execute(message);

        Assert.Equal(2, result.Issues.Count);
        Assert.Contains(result.Issues, i => i.Location == "LOC1");
        Assert.Contains(result.Issues, i => i.Location == "LOC2");
    }

    [Fact]
    public void Execute_ExcludesIssuesFromNonApplicableRule_WhenMixedApplicability()
    {
        var included = new ValidationIssue(ValidationSeverity.Error, "INCLUDED", "CODE", "desc");
        var excluded = new ValidationIssue(ValidationSeverity.Error, "EXCLUDED", "CODE", "desc");
        var validator = new MessageValidator(
        [
            new StubRule(applies: true,  issues: [included]),
            new StubRule(applies: false, issues: [excluded]),
        ]);
        var message = CreateMessage(FullyPopulatedMsh);

        ValidationResult result = validator.Execute(message);

        Assert.Single(result.Issues);
        Assert.Equal("INCLUDED", result.Issues[0].Location);
    }

    [Fact]
    public void Execute_ThrowsArgumentNullException_WhenMessageIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new MessageValidator().Execute(null!));
    }

    [Fact]
    public void Execute_ReturnsIssueForMsh7_WhenMsh7IsBlank()
    {
        var message = CreateMessage("MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|||ADT^A01|MSG00001|P|2.5");

        ValidationResult result = new MessageValidator().Execute(message);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i =>
            i.Location == "MSH-7" && i.Code == "REQUIRED_FIELD_MISSING");
    }

    [Fact]
    public void Execute_ReturnsValidResult_WhenAllRequiredFieldsArePopulated()
    {
        var message = CreateMessage(FullyPopulatedMsh);

        ValidationResult result = new MessageValidator().Execute(message);

        Assert.True(result.IsValid);
        Assert.Empty(result.Issues);
    }

    private static Message CreateMessage(string rawMsh)
    {
        Result<Message> result = Message.Create(rawMsh);
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    private sealed class StubRule : IConformanceRule
    {
        private readonly bool _applies;
        private readonly IReadOnlyList<ValidationIssue> _issues;

        public StubRule(bool applies, IReadOnlyList<ValidationIssue> issues)
        {
            _applies = applies;
            _issues = issues;
        }

        public bool Applies(Message message) => _applies;

        public IReadOnlyList<ValidationIssue> Evaluate(Message message) => _issues;
    }
}
