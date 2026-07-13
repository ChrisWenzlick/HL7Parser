// Copyright (c) Christopher Wenzlick. All rights reserved.

using HL7Parser.Application.Transformation;
using HL7Parser.Domain;
using HL7Parser.Domain.Common;

namespace HL7Parser.Tests.Unit.Application.Transformation;

public class FieldCopyRuleTests
{
    private const string BaseMessage =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ADT^A01|MSG00001|P|2.5\r" +
        "PID|1||PATIENT123^^^MRN||ORIGINAL_PID5\r" +
        "OBX|1|ST|TEST^Result||ORIGINAL_OBX5";

    private const string MessageWithTwoObx =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ADT^A01|MSG00001|P|2.5\r" +
        "PID|1||SRC_VALUE||ORIGINAL_PID5\r" +
        "OBX|1|ST|TEST1||FIRST_OBX5\r" +
        "OBX|2|ST|TEST2||SECOND_OBX5";

    private const string MessageWithNoObx =
        "MSH|^~\\&|SENDAPP|SENDFAC|RECVAPP|RECVFAC|20260709120000||ADT^A01|MSG00001|P|2.5\r" +
        "PID|1||PATIENT123^^^MRN||ORIGINAL_PID5";

    [Fact]
    public void Apply_CopiesFieldValue_WhenSourceAndTargetSegmentsAndFieldsExist()
    {
        var rule = new FieldCopyRule("PID", 3, "OBX", 5);
        var message = CreateMessage(BaseMessage);

        Message result = rule.Apply(message);

        Result<Field> obxField = result.GetSegments("OBX")[0].GetField(5);
        Assert.True(obxField.IsSuccess);
        Assert.Equal("PATIENT123", obxField.Value[0][0][0].RawValue);
    }

    [Fact]
    public void Apply_LeavesOtherFieldsUnchanged_WhenCopyingOneField()
    {
        var rule = new FieldCopyRule("PID", 3, "OBX", 5);
        var message = CreateMessage(BaseMessage);

        Message result = rule.Apply(message);

        Result<Field> pid5 = result.GetSegments("PID")[0].GetField(5);
        Assert.True(pid5.IsSuccess);
        Assert.Equal("ORIGINAL_PID5", pid5.Value[0][0][0].RawValue);
    }

    [Fact]
    public void Apply_ReturnsInputMessageUnchanged_WhenSourceSegmentIsMissing()
    {
        var rule = new FieldCopyRule("ZZZ", 1, "OBX", 5);
        var message = CreateMessage(BaseMessage);

        Message result = rule.Apply(message);

        Assert.Same(message, result);
    }

    [Fact]
    public void Apply_ReturnsInputMessageUnchanged_WhenSourceFieldIndexIsOutOfRange()
    {
        var rule = new FieldCopyRule("PID", 99, "OBX", 5);
        var message = CreateMessage(BaseMessage);

        Message result = rule.Apply(message);

        Assert.Same(message, result);
    }

    [Fact]
    public void Apply_ReturnsInputMessageUnchanged_WhenTargetSegmentIsMissing()
    {
        var rule = new FieldCopyRule("PID", 3, "ZZZ", 1);
        var message = CreateMessage(MessageWithNoObx);

        Message result = rule.Apply(message);

        Assert.Same(message, result);
    }

    [Fact]
    public void Apply_ReturnsInputMessageUnchanged_WhenTargetFieldIndexIsOutOfRange()
    {
        var rule = new FieldCopyRule("PID", 3, "OBX", 99);
        var message = CreateMessage(BaseMessage);

        Message result = rule.Apply(message);

        Assert.Same(message, result);
    }

    [Fact]
    public void Apply_OnlyModifiesFirstMatchingSegment_WhenMessageHasMultipleTargetSegments()
    {
        var rule = new FieldCopyRule("PID", 3, "OBX", 5);
        var message = CreateMessage(MessageWithTwoObx);

        Message result = rule.Apply(message);

        var obxSegments = result.GetSegments("OBX");
        Result<Field> firstObx5 = obxSegments[0].GetField(5);
        Result<Field> secondObx5 = obxSegments[1].GetField(5);

        Assert.True(firstObx5.IsSuccess);
        Assert.Equal("SRC_VALUE", firstObx5.Value[0][0][0].RawValue);
        Assert.True(secondObx5.IsSuccess);
        Assert.Equal("SECOND_OBX5", secondObx5.Value[0][0][0].RawValue);
    }

    [Fact]
    public void Apply_DoesNotMutateOriginalMessage_WhenCopySucceeds()
    {
        var rule = new FieldCopyRule("PID", 3, "OBX", 5);
        var message = CreateMessage(BaseMessage);

        _ = rule.Apply(message);

        Result<Field> originalObx5 = message.GetSegments("OBX")[0].GetField(5);
        Assert.True(originalObx5.IsSuccess);
        Assert.Equal("ORIGINAL_OBX5", originalObx5.Value[0][0][0].RawValue);
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenSourceSegmentIdentifierIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new FieldCopyRule(null!, 1, "OBX", 5));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenTargetSegmentIdentifierIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new FieldCopyRule("PID", 3, null!, 5));
    }

    private static Message CreateMessage(string raw)
    {
        Result<Message> result = Message.Create(raw);
        Assert.True(result.IsSuccess);
        return result.Value;
    }
}
