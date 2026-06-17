// Copyright (c) Christopher Wenzlick. All rights reserved.

using HL7Parser.Domain;
using HL7Parser.Domain.Common;

namespace HL7Parser.Tests.Unit.Domain;

public class RepetitionTests
{
    [Fact]
    public void Create_ReturnsSingleEmptyComponent_WhenValueIsEmpty()
    {
        Result<Repetition> result = Repetition.Create(string.Empty);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Components);
        Assert.Equal(string.Empty, result.Value.Components.First().ToHl7String());
    }

    [Fact]
    public void Create_ReturnsSingleComponent_WhenValueContainsNoDelimiters()
    {
        Result<Repetition> result = Repetition.Create("John");

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Components);
    }

    [Fact]
    public void Create_ReturnsSuccess_WhenValueContainsNonDelimiterSpecialCharacters()
    {
        Result<Repetition> result = Repetition.Create("John-Paul");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsMultipleComponentsInCorrectOrder_WhenValueContainsRepetitionSeparator()
    {
        var componentStrings = new List<string> { "John", "Douglas", "Smith" };
        var testString = string.Join("^", componentStrings);

        Result<Repetition> result = Repetition.Create(testString);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Components.Count);
        Assert.True(componentStrings.SequenceEqual(result.Value.Components.Select(x => x.ToHl7String())));
    }

    [Fact]
    public void Create_ReturnsCorrectNumberOfComponents_WhenValueContainsTrailingRepetitionSeparator()
    {
        var componentStrings = new List<string> { "John", "Douglas", string.Empty };
        var testString = string.Join("&", componentStrings);

        Result<Repetition> result = Repetition.Create(testString);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Components.Count);
        Assert.True(componentStrings.SequenceEqual(result.Value.Components.Select(x => x.ToHl7String())));
    }

    [Fact]
    public void Create_PreservesDuplicateComponents_WhenValueContainsDuplicates()
    {
        Result<Repetition> result = Repetition.Create("Smith^Smith");

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Components.Count);
        Assert.Equal(result.Value.Components[0], result.Value.Components[1]);
    }

    [Fact]
    public void ToHl7String_ReturnsRawValue()
    {
        Result<Repetition> result = Repetition.Create("John&Smith^Male");

        Assert.True(result.IsSuccess);
        Assert.Equal("John&Smith^Male", result.Value.ToHl7String());
    }

    [Fact]
    public void Create_ReturnsFailure_WhenValueIsNull()
    {
        Result<Repetition> result = Repetition.Create(null);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsFailureWithIndexConstant_WhenComponentIsInvalid()
    {
        Result<Repetition> result = Repetition.Create("Smith^Sm|ith");

        Assert.False(result.IsSuccess);
        Assert.Contains("1", result.Error);
        Assert.Contains("|", result.Error);
    }
}
