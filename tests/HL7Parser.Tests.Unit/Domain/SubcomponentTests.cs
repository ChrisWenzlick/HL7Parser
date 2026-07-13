// Copyright (c) Christopher Wenzlick. All rights reserved.

using HL7Parser.Domain;
using HL7Parser.Domain.Common;

namespace HL7Parser.Tests.Unit.Domain;

public class SubcomponentTests
{
    private static readonly EncodingCharacters DefaultEncodingCharacters =
        EncodingCharacters.Create("|^~\\&").Value;

    [Fact]
    public void Create_ReturnsSuccess_WhenValueContainsNoDelimiters()
    {
        Result<Subcomponent> result = Subcomponent.Create("Smith", DefaultEncodingCharacters);

        Assert.True(result.IsSuccess);
        Assert.Equal("Smith", result.Value.RawValue);
    }

    [Fact]
    public void Create_ReturnsSuccess_WhenValueIsEmpty()
    {
        Result<Subcomponent> result = Subcomponent.Create(string.Empty, DefaultEncodingCharacters);

        Assert.True(result.IsSuccess);
        Assert.Equal(string.Empty, result.Value.RawValue);
    }

    [Fact]
    public void Create_ReturnsSuccess_WhenValueIsWhitespace()
    {
        Result<Subcomponent> result = Subcomponent.Create(" ", DefaultEncodingCharacters);

        Assert.True(result.IsSuccess);
        Assert.Equal(" ", result.Value.RawValue);
    }

    [Fact]
    public void Create_ReturnsSuccess_WhenValueContainsNonDelimiterSpecialCharacters()
    {
        Result<Subcomponent> result = Subcomponent.Create("Smith,John-Paul", DefaultEncodingCharacters);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ToHl7String_ReturnsRawValue()
    {
        Result<Subcomponent> result = Subcomponent.Create("Smith", DefaultEncodingCharacters);

        Assert.True(result.IsSuccess);
        Assert.Equal("Smith", result.Value.ToHl7String());
    }

    [Fact]
    public void Create_ReturnsFailure_WhenValueContainsFieldSeparatorDelimiter()
    {
        Result<Subcomponent> result = Subcomponent.Create("Smith|John", DefaultEncodingCharacters);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsFailure_WhenValueContainsComponentSeparatorDelimiter()
    {
        Result<Subcomponent> result = Subcomponent.Create("Smith^John", DefaultEncodingCharacters);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsFailure_WhenValueContainsSubcomponentSeparatorDelimiter()
    {
        Result<Subcomponent> result = Subcomponent.Create("Smith&John", DefaultEncodingCharacters);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsFailure_WhenValueContainsRepetitionSeparatorDelimiter()
    {
        Result<Subcomponent> result = Subcomponent.Create("Smith~John", DefaultEncodingCharacters);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsSuccess_WhenValueContainsEscapeCharacter()
    {
        Result<Subcomponent> result = Subcomponent.Create("John\\Smith", DefaultEncodingCharacters);

        Assert.True(result.IsSuccess);
        Assert.Equal("John\\Smith", result.Value.RawValue);
    }

    [Fact]
    public void Create_ReturnsSuccess_WhenValueContainsEscapeSequenceBr()
    {
        Result<Subcomponent> result = Subcomponent.Create("\\.br\\", DefaultEncodingCharacters);

        Assert.True(result.IsSuccess);
        Assert.Equal("\\.br\\", result.Value.RawValue);
    }

    [Fact]
    public void Create_ReturnsSuccess_WhenValueContainsEscapeSequenceT()
    {
        Result<Subcomponent> result = Subcomponent.Create("\\T\\", DefaultEncodingCharacters);

        Assert.True(result.IsSuccess);
        Assert.Equal("\\T\\", result.Value.RawValue);
    }

    [Fact]
    public void Create_ReturnsSuccess_WhenValueContainsEscapeSequenceF()
    {
        Result<Subcomponent> result = Subcomponent.Create("\\F\\", DefaultEncodingCharacters);

        Assert.True(result.IsSuccess);
        Assert.Equal("\\F\\", result.Value.RawValue);
    }

    [Fact]
    public void Create_ReturnsSuccess_WhenValueContainsNonDefaultEscapeCharacter()
    {
        EncodingCharacters nonDefaultEncoding = EncodingCharacters.Create("|^~!&").Value;

        Result<Subcomponent> result = Subcomponent.Create("foo!bar!", nonDefaultEncoding);

        Assert.True(result.IsSuccess);
        Assert.Equal("foo!bar!", result.Value.RawValue);
    }

    [Fact]
    public void Create_ReturnsSuccess_WhenValueContainsDefaultEscapeCharacter_WithNonDefaultEncoding()
    {
        EncodingCharacters nonDefaultEncoding = EncodingCharacters.Create("|^~!&").Value;

        Result<Subcomponent> result = Subcomponent.Create("foo\\bar", nonDefaultEncoding);

        Assert.True(result.IsSuccess);
        Assert.Equal("foo\\bar", result.Value.RawValue);
    }
}
