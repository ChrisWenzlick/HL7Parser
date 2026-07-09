// Copyright (c) Christopher Wenzlick. All rights reserved.

using HL7Parser.Domain;
using HL7Parser.Domain.Common;
using HL7Parser.Domain.Segments;

namespace HL7Parser.Tests.Unit.Domain.Segments;

public class MshSegmentTests
{
    [Fact]
    public void Create_ReturnsSuccess_WhenValueIsAValidMshSegment()
    {
        Result<MshSegment> result = MshSegment.Create("MSH|^~\\&|sending|receiving");
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsFailure_WhenValueIsEmpty()
    {
        Result<MshSegment> result = MshSegment.Create(string.Empty);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsFailure_WhenValueDoesNotStartWithMsh()
    {
        Result<MshSegment> result = MshSegment.Create("PID|^~\\&|value3");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsFailure_WhenMsh2IsMissing()
    {
        Result<MshSegment> result = MshSegment.Create("MSH|");
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void GetField_ReturnsCorrectMsh1Field_WhenGivenIndex1()
    {
        Result<MshSegment> segmentResult = MshSegment.Create("MSH|^~\\&|sending|receiving");
        Assert.True(segmentResult.IsSuccess);

        Result<Field> msh1Result = segmentResult.Value.GetField(1);
        Assert.Equal("|", msh1Result.Value.ToHl7String());
    }

    [Fact]
    public void GetField_ReturnsCorrectMsh2Field_WhenGivenIndex2()
    {
        Result<MshSegment> segmentResult = MshSegment.Create("MSH|^~\\&|sending|receiving");
        Assert.True(segmentResult.IsSuccess);

        Result<Field> msh2Result = segmentResult.Value.GetField(2);
        Assert.Equal("^~\\&", msh2Result.Value.ToHl7String());
    }

    [Fact]
    public void GetField_ReturnsFailure_WhenGivenIndexIsLessThanOne()
    {
        Result<MshSegment> segmentResult = MshSegment.Create("MSH|^~\\&|sending|receiving");
        Assert.True(segmentResult.IsSuccess);

        Result<Field> invalidFieldResult = segmentResult.Value.GetField(0);
        Assert.False(invalidFieldResult.IsSuccess);
    }

    [Fact]
    public void GetField_ReturnsFailure_WhenGivenIndexGreaterThanFieldCount()
    {
        Result<MshSegment> segmentResult = MshSegment.Create("MSH|^~\\&|sending|receiving");
        Assert.True(segmentResult.IsSuccess);

        Result<Field> invalidFieldResult = segmentResult.Value.GetField(5);
        Assert.False(invalidFieldResult.IsSuccess);
    }

    [Fact]
    public void ToHl7String_ReturnsCorrectString_ForMshSegment()
    {
        var rawSegment = "MSH|^~\\&|sending|receiving";
        Result<MshSegment> segmentResult = MshSegment.Create(rawSegment);
        Assert.True(segmentResult.IsSuccess);

        Assert.Equal(rawSegment, segmentResult.Value.ToHl7String());
    }

    [Fact]
    public void SegmentType_ReturnsMshType_ForMshSegment()
    {
        var rawSegment = "MSH|^~\\&|sending|receiving";
        Result<MshSegment> segmentResult = MshSegment.Create(rawSegment);
        Assert.True(segmentResult.IsSuccess);

        Assert.Equal("MSH", segmentResult.Value.SegmentType.Identifier);
    }
}
