// Copyright (c) Christopher Wenzlick. All rights reserved.

using HL7Parser.Application.UseCases;
using HL7Parser.Application.Validation;
using HL7Parser.Domain;
using HL7Parser.Domain.Common;

namespace HL7Parser.Tests.Integration.Application;

public class MessageParsingAndValidationTests
{
    private readonly IMessageParser _messageParser = new MessageParser();
    private readonly IMessageValidator _messageValidator = new MessageValidator();

    [Fact]
    public void ParseThenValidate_ReturnsValidResult_WhenRawMessageHasAllRequiredMshFields()
    {
        const string rawMessage = "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ADT^A01|MSG00001|P|2.5";

        Result<Message> parseResult = _messageParser.Execute(rawMessage);
        Assert.True(parseResult.IsSuccess);

        ValidationResult validationResult = _messageValidator.Execute(parseResult.Value);

        Assert.True(validationResult.IsValid);
        Assert.Empty(validationResult.Issues);
    }

    [Fact]
    public void ParseThenValidate_ReturnsRequiredFieldMissingIssue_WhenRawMessageIsMissingMsh10()
    {
        const string rawMessage = "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ADT^A01||P|2.5";

        Result<Message> parseResult = _messageParser.Execute(rawMessage);
        Assert.True(parseResult.IsSuccess);

        ValidationResult validationResult = _messageValidator.Execute(parseResult.Value);

        Assert.False(validationResult.IsValid);
        ValidationIssue issue = Assert.Single(validationResult.Issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("MSH-10", issue.Location);
        Assert.Equal("REQUIRED_FIELD_MISSING", issue.Code);
    }
}
