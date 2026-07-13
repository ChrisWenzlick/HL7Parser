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

    private const string OruMshWithObxNoPid =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ORU^R01|MSG00001|P|2.5\r" +
        "OBX|1|ST|TEST^Result||VALUE";

    private const string OruMshWithPidNoObx =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ORU^R01|MSG00001|P|2.5\r" +
        "PID|1||123456^^^MRN||DOE^JOHN";

    private const string OruMshWithPidAndObx =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ORU^R01|MSG00001|P|2.5\r" +
        "PID|1||123456^^^MRN||DOE^JOHN\r" +
        "OBX|1|ST|TEST^Result||VALUE";

    private const string OrmMshWithOrcNoPid =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ORM^O01|MSG00001|P|2.5\r" +
        "ORC|NW|ORDER001";

    private const string OrmMshWithPidNoOrc =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ORM^O01|MSG00001|P|2.5\r" +
        "PID|1||123456^^^MRN||DOE^JOHN";

    private const string OrmMshWithPidAndOrc =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ORM^O01|MSG00001|P|2.5\r" +
        "PID|1||123456^^^MRN||DOE^JOHN\r" +
        "ORC|NW|ORDER001";

    private const string MdmMshWithTxaNoPid =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||MDM^T02|MSG00001|P|2.5\r" +
        "TXA|1|OP";

    private const string MdmMshWithPidNoTxa =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||MDM^T02|MSG00001|P|2.5\r" +
        "PID|1||123456^^^MRN||DOE^JOHN";

    private const string MdmMshWithPidAndTxa =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||MDM^T02|MSG00001|P|2.5\r" +
        "PID|1||123456^^^MRN||DOE^JOHN\r" +
        "TXA|1|OP";

    private const string ZzzMsh =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ZZZ^Z01|MSG00001|P|2.5";

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
        var message = CreateMessage(ZzzMsh);

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
        var message = CreateMessage(ZzzMsh);

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

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenOruMessageHasNoPidSegment()
    {
        var message = CreateMessage(OruMshWithObxNoPid);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("PID", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenOruMessageHasNoObxSegment()
    {
        var message = CreateMessage(OruMshWithPidNoObx);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("OBX", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsNoIssues_WhenOruMessageHasPidAndObxSegments()
    {
        var message = CreateMessage(OruMshWithPidAndObx);

        var issues = _rule.Evaluate(message);

        Assert.Empty(issues);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenOrmMessageHasNoPidSegment()
    {
        var message = CreateMessage(OrmMshWithOrcNoPid);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("PID", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenOrmMessageHasNoOrcSegment()
    {
        var message = CreateMessage(OrmMshWithPidNoOrc);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("ORC", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsNoIssues_WhenOrmMessageHasPidAndOrcSegments()
    {
        var message = CreateMessage(OrmMshWithPidAndOrc);

        var issues = _rule.Evaluate(message);

        Assert.Empty(issues);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenMdmMessageHasNoPidSegment()
    {
        var message = CreateMessage(MdmMshWithTxaNoPid);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("PID", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenMdmMessageHasNoTxaSegment()
    {
        var message = CreateMessage(MdmMshWithPidNoTxa);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("TXA", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsNoIssues_WhenMdmMessageHasPidAndTxaSegments()
    {
        var message = CreateMessage(MdmMshWithPidAndTxa);

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
