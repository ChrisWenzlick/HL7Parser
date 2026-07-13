// Copyright (c) Christopher Wenzlick. All rights reserved.

using HL7Parser.Domain;
using HL7Parser.Domain.Common;
using HL7Parser.Domain.Segments;

namespace HL7Parser.Tests.Unit.Domain;

public class MessageTests
{
    private const string ValidMsh = "MSH|^~\\&|sending|receiving|||||ADT^A01||P|2.5";
    private const string ValidPid = "PID|1||123456^^^MRN||DOE^JOHN";

    [Fact]
    public void Create_ReturnsSuccess_WhenMessageHasSingleMshSegment()
    {
        Result<Message> result = Message.Create(ValidMsh);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsSuccess_WhenMessageHasMultipleSegments()
    {
        Result<Message> result = Message.Create(ValidMsh + "\r" + ValidPid);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsSuccess_WhenMessageUsesCarriageReturnTerminator()
    {
        Result<Message> result = Message.Create(ValidMsh + "\r" + ValidPid + "\r");
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsSuccess_WhenMessageUsesCarriageReturnLineFeedTerminator()
    {
        Result<Message> result = Message.Create(ValidMsh + "\r\n" + ValidPid + "\r\n");
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsSuccess_WhenMessageUsesLineFeedTerminator()
    {
        Result<Message> result = Message.Create(ValidMsh + "\n" + ValidPid + "\n");
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsFailure_WhenRawMessageIsEmpty()
    {
        Result<Message> result = Message.Create(string.Empty);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsFailure_WhenFirstSegmentIsNotMsh()
    {
        Result<Message> result = Message.Create(ValidPid + "\r" + ValidMsh);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsFailure_WhenMessageContainsEmptyLine()
    {
        Result<Message> result = Message.Create(ValidMsh + "\r\r" + ValidPid);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsFailure_WhenAnySegmentFailsToParse()
    {
        Result<Message> result = Message.Create(ValidMsh + "\r" + "I|field1|field2");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Msh_ReturnsMshSegment()
    {
        Result<Message> result = Message.Create(ValidMsh);
        Assert.True(result.IsSuccess);

        Assert.IsType<MshSegment>(result.Value.Msh);
    }

    [Fact]
    public void Segments_ReturnsAllSegmentsInOriginalOrder()
    {
        Result<Message> result = Message.Create(ValidMsh + "\r" + ValidPid);
        Assert.True(result.IsSuccess);

        Assert.Equal(2, result.Value.Segments.Count);
        Assert.IsType<MshSegment>(result.Value.Segments[0]);
        Assert.IsType<Segment>(result.Value.Segments[1]);
    }

    [Fact]
    public void ToHl7String_ReturnsCorrectString_ForSingleMshSegment()
    {
        Result<Message> result = Message.Create(ValidMsh);
        Assert.True(result.IsSuccess);

        Assert.Equal(ValidMsh, result.Value.ToHl7String());
    }

    [Fact]
    public void ToHl7String_ReturnsCorrectString_ForMultipleSegments()
    {
        var rawMessage = ValidMsh + "\r" + ValidPid;
        Result<Message> result = Message.Create(rawMessage);
        Assert.True(result.IsSuccess);

        Assert.Equal(rawMessage, result.Value.ToHl7String());
    }

    [Fact]
    public void ToHl7String_PreservesCarriageReturnTerminator()
    {
        var rawMessage = ValidMsh + "\r" + ValidPid + "\r";
        Result<Message> result = Message.Create(rawMessage);
        Assert.True(result.IsSuccess);

        Assert.Equal(rawMessage, result.Value.ToHl7String());
    }

    [Fact]
    public void ToHl7String_PreservesCarriageReturnLineFeedTerminator()
    {
        var rawMessage = ValidMsh + "\r\n" + ValidPid + "\r\n";
        Result<Message> result = Message.Create(rawMessage);
        Assert.True(result.IsSuccess);

        Assert.Equal(rawMessage, result.Value.ToHl7String());
    }

    [Fact]
    public void ToHl7String_PreservesLineFeedTerminator()
    {
        var rawMessage = ValidMsh + "\n" + ValidPid + "\n";
        Result<Message> result = Message.Create(rawMessage);
        Assert.True(result.IsSuccess);

        Assert.Equal(rawMessage, result.Value.ToHl7String());
    }

    [Fact]
    public void GetSegments_ThrowsArgumentNullException_WhenIdentifierIsNull()
    {
        Message message = Message.Create(ValidMsh).Value;

        Assert.Throws<ArgumentNullException>(() => message.GetSegments(null!));
    }

    [Fact]
    public void GetSegments_ReturnsEmptyList_WhenNoSegmentsMatch()
    {
        Message message = Message.Create(ValidMsh).Value;

        IReadOnlyList<ISegment> result = message.GetSegments("PID");

        Assert.Empty(result);
    }

    [Fact]
    public void GetSegments_ReturnsSingleMatch_WhenOneSegmentMatches()
    {
        Message message = Message.Create(ValidMsh + "\r" + ValidPid).Value;

        IReadOnlyList<ISegment> result = message.GetSegments("PID");

        Assert.Single(result);
        Assert.Equal("PID", result[0].SegmentType.Identifier);
    }

    [Fact]
    public void GetSegments_ReturnsAllMatches_WhenMultipleSegmentsMatch()
    {
        const string nk1A = "NK1|1|DOE^JANE|SPO|555-1234";
        const string nk1B = "NK1|2|DOE^BOB|FTH|555-5678";
        Message message = Message.Create(ValidMsh + "\r" + ValidPid + "\r" + nk1A + "\r" + nk1B).Value;

        IReadOnlyList<ISegment> result = message.GetSegments("NK1");

        Assert.Equal(2, result.Count);
        Assert.All(result, s => Assert.Equal("NK1", s.SegmentType.Identifier));
    }
}
