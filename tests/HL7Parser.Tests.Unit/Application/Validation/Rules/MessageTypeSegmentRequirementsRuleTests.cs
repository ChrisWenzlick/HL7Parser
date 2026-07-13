// Copyright (c) Christopher Wenzlick. All rights reserved.

using HL7Parser.Application.Validation;
using HL7Parser.Application.Validation.Rules;
using HL7Parser.Domain;
using HL7Parser.Domain.Common;

namespace HL7Parser.Tests.Unit.Application.Validation.Rules;

public class MessageTypeSegmentRequirementsRuleTests
{
    private const string AdtMshNoPid =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ADT^A01|MSG00001|P|2.5";

    private const string AdtMshWithPid =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ADT^A01|MSG00001|P|2.5\r" +
        "PID|1||123456^^^MRN||DOE^JOHN";

    private const string OruMshNoPid =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ORU^R01|MSG00001|P|2.5";

    private const string MissingMsh9 =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|||MSG00001|P|2.5";

    private readonly MessageTypeSegmentRequirementsRule _rule = new MessageTypeSegmentRequirementsRule();

    [Fact]
    public void Applies_ReturnsTrue_WhenMessageTypeIsInRequiredSegmentsTable()
    {
        var message = CreateMessage(AdtMshNoPid);

        Assert.True(_rule.Applies(message));
    }

    [Fact]
    public void Applies_ReturnsFalse_WhenMessageTypeIsNotInRequiredSegmentsTable()
    {
        var message = CreateMessage(OruMshNoPid);

        Assert.False(_rule.Applies(message));
    }

    [Fact]
    public void Applies_ReturnsFalse_WhenMsh9IsBlank()
    {
        var message = CreateMessage(MissingMsh9);

        Assert.False(_rule.Applies(message));
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenAdtMessageHasNoPidSegment()
    {
        var message = CreateMessage(AdtMshNoPid);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("PID", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsNoIssues_WhenAdtMessageHasPidSegment()
    {
        var message = CreateMessage(AdtMshWithPid);

        var issues = _rule.Evaluate(message);

        Assert.Empty(issues);
    }

    [Fact]
    public void Evaluate_ReturnsNoIssues_WhenMessageTypeIsNotInRequiredSegmentsTable()
    {
        var message = CreateMessage(OruMshNoPid);

        var issues = _rule.Evaluate(message);

        Assert.Empty(issues);
    }

    [Fact]
    public void Evaluate_ReturnsNoIssues_WhenMsh9IsBlank()
    {
        var message = CreateMessage(MissingMsh9);

        var issues = _rule.Evaluate(message);

        Assert.Empty(issues);
    }

    private static Message CreateMessage(string raw)
    {
        Result<Message> result = Message.Create(raw);
        Assert.True(result.IsSuccess);
        return result.Value;
    }
}
