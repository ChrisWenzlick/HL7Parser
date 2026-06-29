// Copyright (c) Christopher Wenzlick. All rights reserved.

using HL7Parser.Domain;
using HL7Parser.Domain.Common;

namespace HL7Parser.Tests.Unit.Domain;

public class RepetitionTests
{
    private static readonly EncodingCharacters DefaultEncodingCharacters =
        EncodingCharacters.Create("|^~\\&").Value;

    [Fact]
    public void Create_ReturnsSingleEmptyComponent_WhenValueIsEmpty()
    {
        Result<Repetition> result = Repetition.Create(string.Empty, DefaultEncodingCharacters);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Components);
        Assert.Equal(string.Empty, result.Value.Components[0].ToHl7String());
    }

    [Fact]
    public void Create_ReturnsSingleComponent_WhenValueContainsNoDelimiters()
    {
        Result<Repetition> result = Repetition.Create("John", DefaultEncodingCharacters);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Components);
    }

    [Fact]
    public void Create_ReturnsSuccess_WhenValueContainsNonDelimiterSpecialCharacters()
    {
        Result<Repetition> result = Repetition.Create("John-Paul", DefaultEncodingCharacters);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsMultipleComponentsInCorrectOrder_WhenValueContainsComponentSeparator()
    {
        var componentStrings = new List<string> { "John", "Douglas", "Smith" };
        var testString = string.Join("^", componentStrings);

        Result<Repetition> result = Repetition.Create(testString, DefaultEncodingCharacters);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Components.Count);
        Assert.True(componentStrings.SequenceEqual(result.Value.Components.Select(x => x.ToHl7String())));
    }

    [Fact]
    public void Create_ReturnsCorrectNumberOfComponents_WhenValueContainsTrailingComponentSeparator()
    {
        var componentStrings = new List<string> { "John", "Douglas", string.Empty };
        var testString = string.Join("^", componentStrings);

        Result<Repetition> result = Repetition.Create(testString, DefaultEncodingCharacters);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Components.Count);
        Assert.True(componentStrings.SequenceEqual(result.Value.Components.Select(x => x.ToHl7String())));
    }

    [Fact]
    public void Create_PreservesDuplicateComponents_WhenValueContainsDuplicates()
    {
        Result<Repetition> result = Repetition.Create("Smith^Smith", DefaultEncodingCharacters);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Components.Count);
        Assert.Equal(result.Value.Components[0], result.Value.Components[1]);
    }

    [Fact]
    public void ToHl7String_ReturnsOriginalValue_WhenValueContainsComponentAndSubcomponentSeparators()
    {
        Result<Repetition> result = Repetition.Create("John&Smith^Male", DefaultEncodingCharacters);

        Assert.True(result.IsSuccess);
        Assert.Equal("John&Smith^Male", result.Value.ToHl7String());
    }

    [Fact]
    public void Create_ReturnsFailure_WhenValueIsNull()
    {
        Result<Repetition> result = Repetition.Create(null, DefaultEncodingCharacters);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsFailureWithIndexConstant_WhenComponentIsInvalid()
    {
        Result<Repetition> result = Repetition.Create("Smith^Sm|ith", DefaultEncodingCharacters);

        Assert.False(result.IsSuccess);
        Assert.Contains("1", result.Error);
        Assert.Contains("|", result.Error);
    }
}
