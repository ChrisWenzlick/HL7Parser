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

    private const string AdtMshWithEvnNoPid =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ADT^A01|MSG00001|P|2.5\r" +
        "EVN|A01|20260709120000";

    private const string AdtMshWithPidNoEvn =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ADT^A01|MSG00001|P|2.5\r" +
        "PID|1||123456^^^MRN||DOE^JOHN";

    private const string AdtMshWithEvnAndPid =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ADT^A01|MSG00001|P|2.5\r" +
        "EVN|A01|20260709120000\r" +
        "PID|1||123456^^^MRN||DOE^JOHN";

    private const string OruMshWithObrAndObxNoPid =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ORU^R01|MSG00001|P|2.5\r" +
        "OBR|1|||CBC^Complete Blood Count\r" +
        "OBX|1|ST|TEST^Result||VALUE";

    private const string OruMshWithPidAndObrNoObx =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ORU^R01|MSG00001|P|2.5\r" +
        "PID|1||123456^^^MRN||DOE^JOHN\r" +
        "OBR|1|||CBC^Complete Blood Count";

    private const string OruMshWithPidAndObxNoObr =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ORU^R01|MSG00001|P|2.5\r" +
        "PID|1||123456^^^MRN||DOE^JOHN\r" +
        "OBX|1|ST|TEST^Result||VALUE";

    private const string OruMshWithPidObrAndObx =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ORU^R01|MSG00001|P|2.5\r" +
        "PID|1||123456^^^MRN||DOE^JOHN\r" +
        "OBR|1|||CBC^Complete Blood Count\r" +
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

    private const string MdmMshWithEvnAndTxaNoPid =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||MDM^T02|MSG00001|P|2.5\r" +
        "EVN|T02|20260709120000\r" +
        "TXA|1|OP";

    private const string MdmMshWithEvnAndPidNoTxa =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||MDM^T02|MSG00001|P|2.5\r" +
        "EVN|T02|20260709120000\r" +
        "PID|1||123456^^^MRN||DOE^JOHN";

    private const string MdmMshWithPidAndTxaNoEvn =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||MDM^T02|MSG00001|P|2.5\r" +
        "PID|1||123456^^^MRN||DOE^JOHN\r" +
        "TXA|1|OP";

    private const string MdmMshWithEvnPidAndTxa =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||MDM^T02|MSG00001|P|2.5\r" +
        "EVN|T02|20260709120000\r" +
        "PID|1||123456^^^MRN||DOE^JOHN\r" +
        "TXA|1|OP";

    private const string ZzzMsh =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ZZZ^Z01|MSG00001|P|2.5";

    private const string MissingMsh9 =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|||MSG00001|P|2.5";

    // ACK -> [MSA]
    private const string AckMshNoMsa =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ACK^A01|MSG00001|P|2.5";

    private const string AckMshWithMsa =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ACK^A01|MSG00001|P|2.5\r" +
        "MSA|AA|MSG00001";

    // BAR -> [EVN, PID, DG1]
    private const string BarMshWithPidAndDg1NoEvn =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||BAR^P01|MSG00001|P|2.5\r" +
        "PID|1||123456^^^MRN||DOE^JOHN\r" +
        "DG1|1||A00^Cholera^I10";

    private const string BarMshWithEvnAndDg1NoPid =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||BAR^P01|MSG00001|P|2.5\r" +
        "EVN|P01|20260709120000\r" +
        "DG1|1||A00^Cholera^I10";

    private const string BarMshWithEvnAndPidNoDg1 =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||BAR^P01|MSG00001|P|2.5\r" +
        "EVN|P01|20260709120000\r" +
        "PID|1||123456^^^MRN||DOE^JOHN";

    private const string BarMshWithEvnPidAndDg1 =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||BAR^P01|MSG00001|P|2.5\r" +
        "EVN|P01|20260709120000\r" +
        "PID|1||123456^^^MRN||DOE^JOHN\r" +
        "DG1|1||A00^Cholera^I10";

    // DFT -> [PID, FT1]
    private const string DftMshWithFt1NoPid =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||DFT^P03|MSG00001|P|2.5\r" +
        "FT1|1||||20260709120000||CG";

    private const string DftMshWithPidNoFt1 =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||DFT^P03|MSG00001|P|2.5\r" +
        "PID|1||123456^^^MRN||DOE^JOHN";

    private const string DftMshWithPidAndFt1 =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||DFT^P03|MSG00001|P|2.5\r" +
        "PID|1||123456^^^MRN||DOE^JOHN\r" +
        "FT1|1||||20260709120000||CG";

    // MFN -> [MFI, MFE]
    private const string MfnMshWithMfeNoMfi =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||MFN^M01|MSG00001|P|2.5\r" +
        "MFE|MAD|||1";

    private const string MfnMshWithMfiNoMfe =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||MFN^M01|MSG00001|P|2.5\r" +
        "MFI|LOC^Location^HL70450|||UPD";

    private const string MfnMshWithMfiAndMfe =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||MFN^M01|MSG00001|P|2.5\r" +
        "MFI|LOC^Location^HL70450|||UPD\r" +
        "MFE|MAD|||1";

    // OMG -> [PID, ORC, OBR]
    private const string OmgMshWithOrcAndObrNoPid =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||OMG^O19|MSG00001|P|2.5\r" +
        "ORC|NW|ORDER001\r" +
        "OBR|1|||CBC^Complete Blood Count";

    private const string OmgMshWithPidAndObrNoOrc =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||OMG^O19|MSG00001|P|2.5\r" +
        "PID|1||123456^^^MRN||DOE^JOHN\r" +
        "OBR|1|||CBC^Complete Blood Count";

    private const string OmgMshWithPidAndOrcNoObr =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||OMG^O19|MSG00001|P|2.5\r" +
        "PID|1||123456^^^MRN||DOE^JOHN\r" +
        "ORC|NW|ORDER001";

    private const string OmgMshWithPidOrcAndObr =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||OMG^O19|MSG00001|P|2.5\r" +
        "PID|1||123456^^^MRN||DOE^JOHN\r" +
        "ORC|NW|ORDER001\r" +
        "OBR|1|||CBC^Complete Blood Count";

    // OML -> [PID, ORC, OBR]
    private const string OmlMshWithOrcAndObrNoPid =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||OML^O21|MSG00001|P|2.5\r" +
        "ORC|NW|ORDER001\r" +
        "OBR|1|||CBC^Complete Blood Count";

    private const string OmlMshWithPidAndObrNoOrc =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||OML^O21|MSG00001|P|2.5\r" +
        "PID|1||123456^^^MRN||DOE^JOHN\r" +
        "OBR|1|||CBC^Complete Blood Count";

    private const string OmlMshWithPidAndOrcNoObr =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||OML^O21|MSG00001|P|2.5\r" +
        "PID|1||123456^^^MRN||DOE^JOHN\r" +
        "ORC|NW|ORDER001";

    private const string OmlMshWithPidOrcAndObr =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||OML^O21|MSG00001|P|2.5\r" +
        "PID|1||123456^^^MRN||DOE^JOHN\r" +
        "ORC|NW|ORDER001\r" +
        "OBR|1|||CBC^Complete Blood Count";

    // RAS -> [PID, ORC, RXA, RXR]
    private const string RasMshWithOrcRxaRxrNoPid =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||RAS^O17|MSG00001|P|2.5\r" +
        "ORC|NW|ORDER001\r" +
        "RXA|0|1|20260709120000||PENICILLIN^Penicillin||\r" +
        "RXR|IM^Intramuscular";

    private const string RasMshWithPidRxaRxrNoOrc =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||RAS^O17|MSG00001|P|2.5\r" +
        "PID|1||123456^^^MRN||DOE^JOHN\r" +
        "RXA|0|1|20260709120000||PENICILLIN^Penicillin||\r" +
        "RXR|IM^Intramuscular";

    private const string RasMshWithPidOrcRxrNoRxa =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||RAS^O17|MSG00001|P|2.5\r" +
        "PID|1||123456^^^MRN||DOE^JOHN\r" +
        "ORC|NW|ORDER001\r" +
        "RXR|IM^Intramuscular";

    private const string RasMshWithPidOrcRxaNoRxr =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||RAS^O17|MSG00001|P|2.5\r" +
        "PID|1||123456^^^MRN||DOE^JOHN\r" +
        "ORC|NW|ORDER001\r" +
        "RXA|0|1|20260709120000||PENICILLIN^Penicillin||";

    private const string RasMshWithPidOrcRxaAndRxr =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||RAS^O17|MSG00001|P|2.5\r" +
        "PID|1||123456^^^MRN||DOE^JOHN\r" +
        "ORC|NW|ORDER001\r" +
        "RXA|0|1|20260709120000||PENICILLIN^Penicillin||\r" +
        "RXR|IM^Intramuscular";

    // RDE -> [PID, ORC, RXE, RXR]
    private const string RdeMshWithOrcRxeRxrNoPid =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||RDE^O11|MSG00001|P|2.5\r" +
        "ORC|NW|ORDER001\r" +
        "RXE|^^^20260709120000|PENICILLIN^Penicillin\r" +
        "RXR|IM^Intramuscular";

    private const string RdeMshWithPidRxeRxrNoOrc =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||RDE^O11|MSG00001|P|2.5\r" +
        "PID|1||123456^^^MRN||DOE^JOHN\r" +
        "RXE|^^^20260709120000|PENICILLIN^Penicillin\r" +
        "RXR|IM^Intramuscular";

    private const string RdeMshWithPidOrcRxrNoRxe =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||RDE^O11|MSG00001|P|2.5\r" +
        "PID|1||123456^^^MRN||DOE^JOHN\r" +
        "ORC|NW|ORDER001\r" +
        "RXR|IM^Intramuscular";

    private const string RdeMshWithPidOrcRxeNoRxr =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||RDE^O11|MSG00001|P|2.5\r" +
        "PID|1||123456^^^MRN||DOE^JOHN\r" +
        "ORC|NW|ORDER001\r" +
        "RXE|^^^20260709120000|PENICILLIN^Penicillin";

    private const string RdeMshWithPidOrcRxeAndRxr =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||RDE^O11|MSG00001|P|2.5\r" +
        "PID|1||123456^^^MRN||DOE^JOHN\r" +
        "ORC|NW|ORDER001\r" +
        "RXE|^^^20260709120000|PENICILLIN^Penicillin\r" +
        "RXR|IM^Intramuscular";

    // SIU -> [SCH, RGS]
    private const string SiuMshWithRgsNoSch =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||SIU^S12|MSG00001|P|2.5\r" +
        "RGS|1|A";

    private const string SiuMshWithSchNoRgs =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||SIU^S12|MSG00001|P|2.5\r" +
        "SCH|10001|||||||30^MIN";

    private const string SiuMshWithSchAndRgs =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||SIU^S12|MSG00001|P|2.5\r" +
        "SCH|10001|||||||30^MIN\r" +
        "RGS|1|A";

    // VXU -> [PID, ORC, RXA]
    private const string VxuMshWithOrcAndRxaNoPid =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||VXU^V04|MSG00001|P|2.5\r" +
        "ORC|NW|ORDER001\r" +
        "RXA|0|1|20260709120000||PENICILLIN^Penicillin||";

    private const string VxuMshWithPidAndRxaNoOrc =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||VXU^V04|MSG00001|P|2.5\r" +
        "PID|1||123456^^^MRN||DOE^JOHN\r" +
        "RXA|0|1|20260709120000||PENICILLIN^Penicillin||";

    private const string VxuMshWithPidAndOrcNoRxa =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||VXU^V04|MSG00001|P|2.5\r" +
        "PID|1||123456^^^MRN||DOE^JOHN\r" +
        "ORC|NW|ORDER001";

    private const string VxuMshWithPidOrcAndRxa =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||VXU^V04|MSG00001|P|2.5\r" +
        "PID|1||123456^^^MRN||DOE^JOHN\r" +
        "ORC|NW|ORDER001\r" +
        "RXA|0|1|20260709120000||PENICILLIN^Penicillin||";

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
        var message = CreateMessage(AdtMshWithEvnNoPid);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("PID", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenAdtMessageHasNoEvnSegment()
    {
        var message = CreateMessage(AdtMshWithPidNoEvn);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("EVN", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsNoIssues_WhenAdtMessageHasEvnAndPidSegments()
    {
        var message = CreateMessage(AdtMshWithEvnAndPid);

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
        var message = CreateMessage(OruMshWithObrAndObxNoPid);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("PID", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenOruMessageHasNoObrSegment()
    {
        var message = CreateMessage(OruMshWithPidAndObxNoObr);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("OBR", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenOruMessageHasNoObxSegment()
    {
        var message = CreateMessage(OruMshWithPidAndObrNoObx);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("OBX", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsNoIssues_WhenOruMessageHasPidObrAndObxSegments()
    {
        var message = CreateMessage(OruMshWithPidObrAndObx);

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
        var message = CreateMessage(MdmMshWithEvnAndTxaNoPid);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("PID", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenMdmMessageHasNoTxaSegment()
    {
        var message = CreateMessage(MdmMshWithEvnAndPidNoTxa);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("TXA", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenMdmMessageHasNoEvnSegment()
    {
        var message = CreateMessage(MdmMshWithPidAndTxaNoEvn);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("EVN", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsNoIssues_WhenMdmMessageHasEvnPidAndTxaSegments()
    {
        var message = CreateMessage(MdmMshWithEvnPidAndTxa);

        var issues = _rule.Evaluate(message);

        Assert.Empty(issues);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenAckMessageHasNoMsaSegment()
    {
        var message = CreateMessage(AckMshNoMsa);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("MSA", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsNoIssues_WhenAckMessageHasMsaSegment()
    {
        var message = CreateMessage(AckMshWithMsa);

        var issues = _rule.Evaluate(message);

        Assert.Empty(issues);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenBarMessageHasNoEvnSegment()
    {
        var message = CreateMessage(BarMshWithPidAndDg1NoEvn);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("EVN", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenBarMessageHasNoPidSegment()
    {
        var message = CreateMessage(BarMshWithEvnAndDg1NoPid);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("PID", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenBarMessageHasNoDg1Segment()
    {
        var message = CreateMessage(BarMshWithEvnAndPidNoDg1);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("DG1", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsNoIssues_WhenBarMessageHasEvnPidAndDg1Segments()
    {
        var message = CreateMessage(BarMshWithEvnPidAndDg1);

        var issues = _rule.Evaluate(message);

        Assert.Empty(issues);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenDftMessageHasNoPidSegment()
    {
        var message = CreateMessage(DftMshWithFt1NoPid);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("PID", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenDftMessageHasNoFt1Segment()
    {
        var message = CreateMessage(DftMshWithPidNoFt1);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("FT1", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsNoIssues_WhenDftMessageHasPidAndFt1Segments()
    {
        var message = CreateMessage(DftMshWithPidAndFt1);

        var issues = _rule.Evaluate(message);

        Assert.Empty(issues);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenMfnMessageHasNoMfiSegment()
    {
        var message = CreateMessage(MfnMshWithMfeNoMfi);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("MFI", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenMfnMessageHasNoMfeSegment()
    {
        var message = CreateMessage(MfnMshWithMfiNoMfe);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("MFE", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsNoIssues_WhenMfnMessageHasMfiAndMfeSegments()
    {
        var message = CreateMessage(MfnMshWithMfiAndMfe);

        var issues = _rule.Evaluate(message);

        Assert.Empty(issues);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenOmgMessageHasNoPidSegment()
    {
        var message = CreateMessage(OmgMshWithOrcAndObrNoPid);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("PID", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenOmgMessageHasNoOrcSegment()
    {
        var message = CreateMessage(OmgMshWithPidAndObrNoOrc);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("ORC", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenOmgMessageHasNoObrSegment()
    {
        var message = CreateMessage(OmgMshWithPidAndOrcNoObr);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("OBR", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsNoIssues_WhenOmgMessageHasPidOrcAndObrSegments()
    {
        var message = CreateMessage(OmgMshWithPidOrcAndObr);

        var issues = _rule.Evaluate(message);

        Assert.Empty(issues);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenOmlMessageHasNoPidSegment()
    {
        var message = CreateMessage(OmlMshWithOrcAndObrNoPid);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("PID", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenOmlMessageHasNoOrcSegment()
    {
        var message = CreateMessage(OmlMshWithPidAndObrNoOrc);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("ORC", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenOmlMessageHasNoObrSegment()
    {
        var message = CreateMessage(OmlMshWithPidAndOrcNoObr);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("OBR", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsNoIssues_WhenOmlMessageHasPidOrcAndObrSegments()
    {
        var message = CreateMessage(OmlMshWithPidOrcAndObr);

        var issues = _rule.Evaluate(message);

        Assert.Empty(issues);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenRasMessageHasNoPidSegment()
    {
        var message = CreateMessage(RasMshWithOrcRxaRxrNoPid);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("PID", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenRasMessageHasNoOrcSegment()
    {
        var message = CreateMessage(RasMshWithPidRxaRxrNoOrc);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("ORC", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenRasMessageHasNoRxaSegment()
    {
        var message = CreateMessage(RasMshWithPidOrcRxrNoRxa);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("RXA", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenRasMessageHasNoRxrSegment()
    {
        var message = CreateMessage(RasMshWithPidOrcRxaNoRxr);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("RXR", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsNoIssues_WhenRasMessageHasPidOrcRxaAndRxrSegments()
    {
        var message = CreateMessage(RasMshWithPidOrcRxaAndRxr);

        var issues = _rule.Evaluate(message);

        Assert.Empty(issues);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenRdeMessageHasNoPidSegment()
    {
        var message = CreateMessage(RdeMshWithOrcRxeRxrNoPid);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("PID", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenRdeMessageHasNoOrcSegment()
    {
        var message = CreateMessage(RdeMshWithPidRxeRxrNoOrc);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("ORC", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenRdeMessageHasNoRxeSegment()
    {
        var message = CreateMessage(RdeMshWithPidOrcRxrNoRxe);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("RXE", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenRdeMessageHasNoRxrSegment()
    {
        var message = CreateMessage(RdeMshWithPidOrcRxeNoRxr);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("RXR", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsNoIssues_WhenRdeMessageHasPidOrcRxeAndRxrSegments()
    {
        var message = CreateMessage(RdeMshWithPidOrcRxeAndRxr);

        var issues = _rule.Evaluate(message);

        Assert.Empty(issues);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenSiuMessageHasNoSchSegment()
    {
        var message = CreateMessage(SiuMshWithRgsNoSch);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("SCH", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenSiuMessageHasNoRgsSegment()
    {
        var message = CreateMessage(SiuMshWithSchNoRgs);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("RGS", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsNoIssues_WhenSiuMessageHasSchAndRgsSegments()
    {
        var message = CreateMessage(SiuMshWithSchAndRgs);

        var issues = _rule.Evaluate(message);

        Assert.Empty(issues);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenVxuMessageHasNoPidSegment()
    {
        var message = CreateMessage(VxuMshWithOrcAndRxaNoPid);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("PID", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenVxuMessageHasNoOrcSegment()
    {
        var message = CreateMessage(VxuMshWithPidAndRxaNoOrc);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("ORC", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsRequiredSegmentMissingError_WhenVxuMessageHasNoRxaSegment()
    {
        var message = CreateMessage(VxuMshWithPidAndOrcNoRxa);

        var issues = _rule.Evaluate(message);

        ValidationIssue issue = Assert.Single(issues);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("RXA", issue.Location);
        Assert.Equal("REQUIRED_SEGMENT_MISSING", issue.Code);
    }

    [Fact]
    public void Evaluate_ReturnsNoIssues_WhenVxuMessageHasPidOrcAndRxaSegments()
    {
        var message = CreateMessage(VxuMshWithPidOrcAndRxa);

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
