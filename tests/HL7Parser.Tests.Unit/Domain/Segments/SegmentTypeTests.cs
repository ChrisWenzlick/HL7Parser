// Copyright (c) Christopher Wenzlick. All rights reserved.

using HL7Parser.Domain.Common;
using HL7Parser.Domain.Segments;

namespace HL7Parser.Tests.Unit.Domain.Segments;

public class SegmentTypeTests
{
    [Fact]
    public void Create_ReturnsSuccess_ForStandardSegmentId()
    {
        Result<SegmentType> result = SegmentType.Create("PID");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsSuccess_ForAlphanumericSegmentId()
    {
        Result<SegmentType> result = SegmentType.Create("PV1");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsSuccess_ForZSegment()
    {
        Result<SegmentType> result = SegmentType.Create("ZPD");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsFailure_WhenValueIsEmpty()
    {
        Result<SegmentType> result = SegmentType.Create(string.Empty);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsFailure_WhenValueIsShorterThanThreeCharacters()
    {
        Result<SegmentType> result = SegmentType.Create("PV");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsFailure_WhenValueIsLongerThanThreeCharacters()
    {
        Result<SegmentType> result = SegmentType.Create("PVDR");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsFailure_WhenValueContainsLowercaseCharacters()
    {
        Result<SegmentType> result = SegmentType.Create("pv1");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsFailure_WhenValueContainsSpecialCharacters()
    {
        Result<SegmentType> result = SegmentType.Create("PR$");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsFailure_WhenValueContainsWhitespace()
    {
        Result<SegmentType> result = SegmentType.Create("P V");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void ValueProperty_ReturnsOriginalValue()
    {
        Result<SegmentType> result = SegmentType.Create("PV1");

        Assert.True(result.IsSuccess);
        Assert.Equal("PV1", result.Value.Identifier);
    }
}
