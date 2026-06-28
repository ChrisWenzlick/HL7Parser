// Copyright (c) Christopher Wenzlick. All rights reserved.

using HL7Parser.Domain;
using HL7Parser.Domain.Common;
using HL7Parser.Domain.Segments;

namespace HL7Parser.Tests.Unit.Domain.Segments;

public class SegmentTests
{
    private static readonly EncodingCharacters DefaultEncodingCharacters =
        EncodingCharacters.Create("|^~\\&").Value;

    [Fact]
    public void Create_ReturnsSuccess_WhenValueIsAStandardSegment()
    {
        Result<Segment> result = Segment.Create("PID|value1|value2", DefaultEncodingCharacters);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsSuccess_WhenValueHasEmptyFields()
    {
        Result<Segment> result = Segment.Create("PID|value1||value3", DefaultEncodingCharacters);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsFailure_WhenValueIsEmpty()
    {
        Result<Segment> result = Segment.Create(string.Empty, DefaultEncodingCharacters);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsFailure_WhenSegmentIdIsInvalid()
    {
        Result<Segment> result = Segment.Create("PI|value1|value2", DefaultEncodingCharacters);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void GetField_ReturnsCorrectField_WhenGivenValidOneBasedIndex()
    {
        Result<Segment> segmentResult = Segment.Create("PID|value1|value2|value3", DefaultEncodingCharacters);

        Assert.True(segmentResult.IsSuccess);

        Result<Field> secondFieldResult = segmentResult.Value.GetField(2);

        Assert.Equal("value2", secondFieldResult.Value.ToHl7String());
    }

    [Fact]
    public void GetField_ReturnsFailure_WhenGivenIndexIsLessThanOne()
    {
        Result<Segment> segmentResult = Segment.Create("PID|value1|value2", DefaultEncodingCharacters);

        Assert.True(segmentResult.IsSuccess);

        Result<Field> invalidFieldResult = segmentResult.Value.GetField(0);

        Assert.False(invalidFieldResult.IsSuccess);
    }

    [Fact]
    public void GetField_ReturnsFailure_WhenGivenIndexGreaterThanFieldCount()
    {
        Result<Segment> segmentResult = Segment.Create("PID|value1|value2", DefaultEncodingCharacters);

        Assert.True(segmentResult.IsSuccess);

        Result<Field> invalidFieldResult = segmentResult.Value.GetField(3);

        Assert.False(invalidFieldResult.IsSuccess);
    }

    [Fact]
    public void ToHl7String_ReturnsCorrectString_ForStandardSegment()
    {
        var rawSegment = "PID|value1||value3";
        Result<Segment> segmentResult = Segment.Create(rawSegment, DefaultEncodingCharacters);
        Assert.True(segmentResult.IsSuccess);

        Assert.Equal(rawSegment, segmentResult.Value.ToHl7String());
    }
}
