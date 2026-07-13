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
        const string rawMessage =
            "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ADT^A01|MSG00001|P|2.5\r" +
            "PID|1||123456^^^MRN||DOE^JOHN";

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
        Assert.Contains(validationResult.Issues, i =>
            i.Severity == ValidationSeverity.Error &&
            i.Location == "MSH-10" &&
            i.Code == "REQUIRED_FIELD_MISSING");
    }

    [Fact]
    public void ParseThenValidate_ReturnsRequiredSegmentMissingIssue_WhenAdtMessageHasNoPidSegment()
    {
        const string rawMessage = "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ADT^A01|MSG00001|P|2.5";

        Result<Message> parseResult = _messageParser.Execute(rawMessage);
        Assert.True(parseResult.IsSuccess);

        ValidationResult validationResult = _messageValidator.Execute(parseResult.Value);

        Assert.False(validationResult.IsValid);
        Assert.Contains(validationResult.Issues, i =>
            i.Severity == ValidationSeverity.Error &&
            i.Location == "PID" &&
            i.Code == "REQUIRED_SEGMENT_MISSING");
    }

    [Fact]
    public void ParseThenValidate_ReturnsRequiredSegmentMissingIssue_WhenOruMessageHasNoObxSegment()
    {
        const string rawMessage =
            "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ORU^R01|MSG00001|P|2.5\r" +
            "PID|1||123456^^^MRN||DOE^JOHN";

        Result<Message> parseResult = _messageParser.Execute(rawMessage);
        Assert.True(parseResult.IsSuccess);

        ValidationResult validationResult = _messageValidator.Execute(parseResult.Value);

        Assert.False(validationResult.IsValid);
        Assert.Contains(validationResult.Issues, i =>
            i.Severity == ValidationSeverity.Error &&
            i.Location == "OBX" &&
            i.Code == "REQUIRED_SEGMENT_MISSING");
    }
}
