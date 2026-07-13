// Copyright (c) Christopher Wenzlick. All rights reserved.

using System.Linq;
using HL7Parser.Application.UseCases;
using HL7Parser.Application.Validation;
using HL7Parser.Application.Validation.Rules;
using HL7Parser.Domain;
using HL7Parser.Domain.Common;

namespace HL7Parser.Tests.Unit.Application.Validation.Rules;

public class OptionalMshFieldsRuleTests
{
    private const string FullyPopulatedMsh =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ADT^A01|MSG00001|P|2.5";

    private readonly OptionalMshFieldsRule _rule = new OptionalMshFieldsRule();

    [Fact]
    public void Applies_ReturnsTrue_ForAnyMessage()
    {
        var message = CreateMessage(FullyPopulatedMsh);

        Assert.True(_rule.Applies(message));
    }

    [Fact]
    public void Evaluate_ReturnsIssueForMsh3_WhenMsh3IsBlank()
    {
        var message = CreateMessage("MSH|^~\\&||SENDFAC|RECVAPP|RECVFAC|20260709120000||ADT^A01|MSG00001|P|2.5");

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Warning, issue.Severity);
        Assert.Equal("MSH-3", issue.Location);
        Assert.Equal("OPTIONAL_FIELD_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsIssueForMsh4_WhenMsh4IsBlank()
    {
        var message = CreateMessage("MSH|^~\\&|SENDAPP||RECVAPP|RECVFAC|20260709120000||ADT^A01|MSG00001|P|2.5");

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Warning, issue.Severity);
        Assert.Equal("MSH-4", issue.Location);
        Assert.Equal("OPTIONAL_FIELD_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsIssueForMsh5_WhenMsh5IsBlank()
    {
        var message = CreateMessage("MSH|^~\\&|SENDAPP|SENDFAC||RECVFAC|20260709120000||ADT^A01|MSG00001|P|2.5");

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Warning, issue.Severity);
        Assert.Equal("MSH-5", issue.Location);
        Assert.Equal("OPTIONAL_FIELD_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsIssueForMsh6_WhenMsh6IsBlank()
    {
        var message = CreateMessage("MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP||20260709120000||ADT^A01|MSG00001|P|2.5");

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Warning, issue.Severity);
        Assert.Equal("MSH-6", issue.Location);
        Assert.Equal("OPTIONAL_FIELD_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsIssueForMsh3_WhenMsh3IsWhitespaceOnly()
    {
        var message = CreateMessage("MSH|^~\\&|   |SENDFAC|RECVAPP|RECVFAC|20260709120000||ADT^A01|MSG00001|P|2.5");

        var issues = _rule.Evaluate(message);

        Assert.Contains(issues, i => i.Location == "MSH-3");
    }

    [Fact]
    public void Evaluate_ReturnsNoIssues_WhenAllOptionalFieldsArePopulated()
    {
        var message = CreateMessage(FullyPopulatedMsh);

        var issues = _rule.Evaluate(message);

        Assert.Empty(issues);
    }

    [Fact]
    public void Evaluate_ReturnsIssueForEachMissingField_WhenMultipleOptionalFieldsAreBlank()
    {
        var message = CreateMessage("MSH|^~\\&|||RECVAPP||20260709120000||ADT^A01|MSG00001|P|2.5");

        var issues = _rule.Evaluate(message);

        var locations = issues.Select(i => i.Location).ToList();
        Assert.Contains("MSH-3", locations);
        Assert.Contains("MSH-4", locations);
        Assert.Contains("MSH-6", locations);
        Assert.Equal(3, issues.Count);
    }

    [Fact]
    public void Evaluate_ReturnsWarning_AndIsValidRemainsTrue_WhenOnlyOptionalFieldsAreMissing()
    {
        var message = CreateMessage("MSH|^~\\&|||RECVAPP|RECVFAC|20260709120000||ADT^A01|MSG00001|P|2.5");
        var validator = new MessageValidator(new IConformanceRule[] { _rule });

        ValidationResult result = validator.Execute(message);

        Assert.True(result.IsValid);
        Assert.NotEmpty(result.Issues);
        Assert.All(result.Issues, i => Assert.Equal(ValidationSeverity.Warning, i.Severity));
    }

    [Fact]
    public void Evaluate_ProducesBothErrorAndWarning_WhenRequiredAndOptionalFieldsAreMissing()
    {
        var message = CreateMessage("MSH|^~\\&|SENDAPP||RECVAPP|RECVFAC|20260709120000||ADT^A01||P|2.5");
        var validator = new MessageValidator(
            new IConformanceRule[] { new RequiredMshFieldsRule(), _rule });

        ValidationResult result = validator.Execute(message);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Severity == ValidationSeverity.Error && i.Location == "MSH-10");
        Assert.Contains(result.Issues, i => i.Severity == ValidationSeverity.Warning && i.Location == "MSH-4");
    }

    private static Message CreateMessage(string rawMsh)
    {
        Result<Message> result = Message.Create(rawMsh);
        Assert.True(result.IsSuccess);
        return result.Value;
    }
}
