// Copyright (c) Christopher Wenzlick. All rights reserved.

using System.Linq;
using HL7Parser.Application.Validation;
using HL7Parser.Application.Validation.Rules;
using HL7Parser.Domain;
using HL7Parser.Domain.Common;

namespace HL7Parser.Tests.Unit.Application.Validation.Rules;

public class RequiredMshFieldsRuleTests
{
    private const string FullyPopulatedMsh =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ADT^A01|MSG00001|P|2.5";

    private readonly RequiredMshFieldsRule _rule = new RequiredMshFieldsRule();

    [Fact]
    public void Applies_ReturnsTrue_ForAnyMessage()
    {
        var message = CreateMessage(FullyPopulatedMsh);

        Assert.True(_rule.Applies(message));
    }

    [Fact]
    public void Evaluate_ReturnsIssueForMsh7_WhenMsh7IsBlank()
    {
        var message = CreateMessage("MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|||ADT^A01|MSG00001|P|2.5");

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("MSH-7", issue.Location);
        Assert.Equal("REQUIRED_FIELD_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsIssueForMsh9_WhenMsh9IsBlank()
    {
        var message = CreateMessage("MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000|||MSG00001|P|2.5");

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("MSH-9", issue.Location);
        Assert.Equal("REQUIRED_FIELD_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsIssueForMsh10_WhenMsh10IsBlank()
    {
        var message = CreateMessage("MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ADT^A01||P|2.5");

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("MSH-10", issue.Location);
        Assert.Equal("REQUIRED_FIELD_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsIssueForMsh11_WhenMsh11IsBlank()
    {
        var message = CreateMessage("MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ADT^A01|MSG00001||2.5");

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("MSH-11", issue.Location);
        Assert.Equal("REQUIRED_FIELD_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsIssueForMsh12_WhenMsh12IsBlank()
    {
        var message = CreateMessage("MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ADT^A01|MSG00001|P|");

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("MSH-12", issue.Location);
        Assert.Equal("REQUIRED_FIELD_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsIssueForMsh7_WhenMsh7IsWhitespaceOnly()
    {
        var message = CreateMessage("MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|   ||ADT^A01|MSG00001|P|2.5");

        var issues = _rule.Evaluate(message);

        Assert.Contains(issues, i => i.Location == "MSH-7");
    }

    [Fact]
    public void Evaluate_ReturnsIssueForMsh7_WhenMsh7IsEntirelyAbsentFromSegment()
    {
        var message = CreateMessage("MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC");

        var issues = _rule.Evaluate(message);

        Assert.Contains(issues, i => i.Location == "MSH-7");
    }

    [Fact]
    public void Evaluate_ReturnsNoIssues_WhenAllRequiredFieldsArePopulated()
    {
        var message = CreateMessage(FullyPopulatedMsh);

        var issues = _rule.Evaluate(message);

        Assert.Empty(issues);
    }

    [Fact]
    public void Evaluate_ReturnsIssueForEachMissingField_WhenMultipleRequiredFieldsAreBlank()
    {
        var message = CreateMessage("MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC||||MSG00001|P|2.5");

        var issues = _rule.Evaluate(message);

        var locations = issues.Select(i => i.Location).ToList();
        Assert.Contains("MSH-7", locations);
        Assert.Contains("MSH-9", locations);
        Assert.Equal(2, issues.Count);
    }

    private static Message CreateMessage(string rawMsh)
    {
        Result<Message> result = Message.Create(rawMsh);
        Assert.True(result.IsSuccess);
        return result.Value;
    }
}
