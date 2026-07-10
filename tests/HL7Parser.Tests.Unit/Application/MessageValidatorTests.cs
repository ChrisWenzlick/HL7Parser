// Copyright (c) Christopher Wenzlick. All rights reserved.

using System.Linq;
using HL7Parser.Application.UseCases;
using HL7Parser.Application.Validation;
using HL7Parser.Domain;
using HL7Parser.Domain.Common;

namespace HL7Parser.Tests.Unit.Application;

public class MessageValidatorTests
{
    private const string FullyPopulatedMsh =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ADT^A01|MSG00001|P|2.5";

    private readonly IMessageValidator _messageValidator = new MessageValidator();

    [Fact]
    public void Execute_ReturnsIssueForMsh7_WhenMsh7IsBlank()
    {
        var message = CreateMessage("MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|||ADT^A01|MSG00001|P|2.5");

        ValidationResult result = _messageValidator.Execute(message);

        Assert.False(result.IsValid);
        ValidationIssue issue = Assert.Single(result.Issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("MSH-7", issue.Location);
        Assert.Equal("REQUIRED_FIELD_MISSING", issue.Code);
    }

    [Fact]
    public void Execute_ReturnsIssueForMsh9_WhenMsh9IsBlank()
    {
        var message = CreateMessage("MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000|||MSG00001|P|2.5");

        ValidationResult result = _messageValidator.Execute(message);

        Assert.False(result.IsValid);
        ValidationIssue issue = Assert.Single(result.Issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("MSH-9", issue.Location);
        Assert.Equal("REQUIRED_FIELD_MISSING", issue.Code);
    }

    [Fact]
    public void Execute_ReturnsIssueForMsh10_WhenMsh10IsBlank()
    {
        var message = CreateMessage("MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ADT^A01||P|2.5");

        ValidationResult result = _messageValidator.Execute(message);

        Assert.False(result.IsValid);
        ValidationIssue issue = Assert.Single(result.Issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("MSH-10", issue.Location);
        Assert.Equal("REQUIRED_FIELD_MISSING", issue.Code);
    }

    [Fact]
    public void Execute_ReturnsIssueForMsh11_WhenMsh11IsBlank()
    {
        var message = CreateMessage("MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ADT^A01|MSG00001||2.5");

        ValidationResult result = _messageValidator.Execute(message);

        Assert.False(result.IsValid);
        ValidationIssue issue = Assert.Single(result.Issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("MSH-11", issue.Location);
        Assert.Equal("REQUIRED_FIELD_MISSING", issue.Code);
    }

    [Fact]
    public void Execute_ReturnsIssueForMsh12_WhenMsh12IsBlank()
    {
        var message = CreateMessage("MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ADT^A01|MSG00001|P|");

        ValidationResult result = _messageValidator.Execute(message);

        Assert.False(result.IsValid);
        ValidationIssue issue = Assert.Single(result.Issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("MSH-12", issue.Location);
        Assert.Equal("REQUIRED_FIELD_MISSING", issue.Code);
    }

    [Fact]
    public void Execute_ReturnsIssueForMsh7_WhenMsh7IsWhitespaceOnly()
    {
        var message = CreateMessage("MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|   ||ADT^A01|MSG00001|P|2.5");

        ValidationResult result = _messageValidator.Execute(message);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Location == "MSH-7");
    }

    [Fact]
    public void Execute_ReturnsIssueForMsh7_WhenMsh7IsEntirelyAbsentFromSegment()
    {
        var message = CreateMessage("MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC");

        ValidationResult result = _messageValidator.Execute(message);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, i => i.Location == "MSH-7");
    }

    [Fact]
    public void Execute_ReturnsValidResultWithNoIssues_WhenAllRequiredFieldsArePopulated()
    {
        var message = CreateMessage(FullyPopulatedMsh);

        ValidationResult result = _messageValidator.Execute(message);

        Assert.True(result.IsValid);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void Execute_ReturnsIssueForEachMissingField_WhenMultipleRequiredFieldsAreBlank()
    {
        var message = CreateMessage("MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC||||MSG00001|P|2.5");

        ValidationResult result = _messageValidator.Execute(message);

        Assert.False(result.IsValid);
        var locations = result.Issues.Select(i => i.Location).ToList();
        Assert.Contains("MSH-7", locations);
        Assert.Contains("MSH-9", locations);
        Assert.Equal(2, result.Issues.Count);
    }

    [Fact]
    public void Execute_ThrowsArgumentNullException_WhenMessageIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _messageValidator.Execute(null!));
    }

    private static Message CreateMessage(string rawMsh)
    {
        Result<Message> result = Message.Create(rawMsh);
        Assert.True(result.IsSuccess);
        return result.Value;
    }
}
