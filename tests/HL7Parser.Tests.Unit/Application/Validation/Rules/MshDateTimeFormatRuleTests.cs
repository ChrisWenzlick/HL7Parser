// Copyright (c) Christopher Wenzlick. All rights reserved.

using HL7Parser.Application.Validation;
using HL7Parser.Application.Validation.Rules;
using HL7Parser.Domain;
using HL7Parser.Domain.Common;

namespace HL7Parser.Tests.Unit.Application.Validation.Rules;

public class MshDateTimeFormatRuleTests
{
    private const string MissingMsh7 =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|||ADT^A01|MSG00001|P|2.5";

    private readonly MshDateTimeFormatRule _rule = new MshDateTimeFormatRule();

    [Fact]
    public void Applies_ReturnsFalse_WhenMsh7IsMissing()
    {
        var message = CreateMessage(MissingMsh7);

        Assert.False(_rule.Applies(message));
    }

    [Fact]
    public void Applies_ReturnsFalse_WhenMsh7IsBlank()
    {
        var message = CreateMessageWithMsh7("   ");

        Assert.False(_rule.Applies(message));
    }

    [Fact]
    public void Applies_ReturnsTrue_WhenMsh7IsPresent()
    {
        var message = CreateMessageWithMsh7("20260709120000");

        Assert.True(_rule.Applies(message));
    }

    [Fact]
    public void Evaluate_ReturnsNoIssues_WhenMsh7HasFourDigitPrefix()
    {
        var message = CreateMessageWithMsh7("2026");

        var issues = _rule.Evaluate(message);

        Assert.Empty(issues);
    }

    [Fact]
    public void Evaluate_ReturnsNoIssues_WhenMsh7HasSixDigitPrefix()
    {
        var message = CreateMessageWithMsh7("202607");

        var issues = _rule.Evaluate(message);

        Assert.Empty(issues);
    }

    [Fact]
    public void Evaluate_ReturnsNoIssues_WhenMsh7HasEightDigitPrefix()
    {
        var message = CreateMessageWithMsh7("20260709");

        var issues = _rule.Evaluate(message);

        Assert.Empty(issues);
    }

    [Fact]
    public void Evaluate_ReturnsNoIssues_WhenMsh7HasTwelveDigitPrefix()
    {
        var message = CreateMessageWithMsh7("202607091200");

        var issues = _rule.Evaluate(message);

        Assert.Empty(issues);
    }

    [Fact]
    public void Evaluate_ReturnsNoIssues_WhenMsh7HasFourteenDigitPrefix()
    {
        var message = CreateMessageWithMsh7("20260709120000");

        var issues = _rule.Evaluate(message);

        Assert.Empty(issues);
    }

    [Fact]
    public void Evaluate_ReturnsNoIssues_WhenMsh7HasFractionalSeconds()
    {
        var message = CreateMessageWithMsh7("20260709120000.1234");

        var issues = _rule.Evaluate(message);

        Assert.Empty(issues);
    }

    [Fact]
    public void Evaluate_ReturnsNoIssues_WhenMsh7HasTimezoneOffset()
    {
        var message = CreateMessageWithMsh7("20260709120000+0500");

        var issues = _rule.Evaluate(message);

        Assert.Empty(issues);
    }

    [Fact]
    public void Evaluate_ReturnsNoIssues_WhenMsh7HasFractionalSecondsAndTimezoneOffset()
    {
        var message = CreateMessageWithMsh7("20260709120000.1234+0500");

        var issues = _rule.Evaluate(message);

        Assert.Empty(issues);
    }

    [Fact]
    public void Evaluate_ReturnsInvalidFormatError_WhenMsh7IsNotNumeric()
    {
        var message = CreateMessageWithMsh7("notadate");

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("MSH-7", issue.Location);
        Assert.Equal("INVALID_FIELD_FORMAT", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsInvalidFormatError_WhenMsh7HasFiveDigitPrefix()
    {
        var message = CreateMessageWithMsh7("20260");

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("MSH-7", issue.Location);
        Assert.Equal("INVALID_FIELD_FORMAT", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsInvalidFormatError_WhenMsh7HasSevenDigitPrefix()
    {
        var message = CreateMessageWithMsh7("2026071");

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("MSH-7", issue.Location);
        Assert.Equal("INVALID_FIELD_FORMAT", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsInvalidFormatError_WhenMsh7HasTimezoneWithoutSign()
    {
        var message = CreateMessageWithMsh7("202607091200000500");

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("MSH-7", issue.Location);
        Assert.Equal("INVALID_FIELD_FORMAT", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsInvalidFormatError_WhenMsh7HasTimezoneWithWrongDigitCount()
    {
        var message = CreateMessageWithMsh7("20260709120000+500");

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("MSH-7", issue.Location);
        Assert.Equal("INVALID_FIELD_FORMAT", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsNoIssues_WhenMsh7IsCalendarInvalidButStructurallyWellFormed()
    {
        var message = CreateMessageWithMsh7("20260230");

        var issues = _rule.Evaluate(message);

        Assert.Empty(issues);
    }

    [Fact]
    public void Evaluate_ReturnsNoIssues_WhenMsh7IsMissing()
    {
        var message = CreateMessage(MissingMsh7);

        var issues = _rule.Evaluate(message);

        Assert.Empty(issues);
    }

    [Fact]
    public void Evaluate_ReturnsNoIssues_WhenMsh7IsBlank()
    {
        var message = CreateMessageWithMsh7("   ");

        var issues = _rule.Evaluate(message);

        Assert.Empty(issues);
    }

    private static Message CreateMessageWithMsh7(string msh7Value)
    {
        return CreateMessage($"MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|{msh7Value}||ADT^A01|MSG00001|P|2.5");
    }

    private static Message CreateMessage(string raw)
    {
        Result<Message> result = Message.Create(raw);
        Assert.True(result.IsSuccess);
        return result.Value;
    }
}
