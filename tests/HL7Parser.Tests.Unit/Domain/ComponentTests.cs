// Copyright (c) Christopher Wenzlick. All rights reserved.

using HL7Parser.Domain;
using HL7Parser.Domain.Common;

namespace HL7Parser.Tests.Unit.Domain;

public class ComponentTests
{
    [Fact]
    public void Create_ReturnsSingleEmptySubcomponent_WhenValueIsEmpty()
    {
        Result<Component> result = Component.Create(string.Empty);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Subcomponents);
        Assert.Equal(string.Empty, result.Value.Subcomponents.First().RawValue);
    }

    [Fact]
    public void Create_ReturnsSingleSubcomponent_WhenValueContainsNoDelimiters()
    {
        Result<Component> result = Component.Create("John");

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Subcomponents);
    }

    [Fact]
    public void Create_ReturnsSuccess_WhenValueContainsNonDelimiterSpecialCharacters()
    {
        Result<Component> result = Component.Create("John-Paul");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsMultipleSubcomponentsInCorrectOrder_WhenValueContainsSubcomponentSeparator()
    {
        var subcomponentStrings = new List<string> { "John", "Douglas", "Smith" };
        var testString = string.Join("&", subcomponentStrings);

        Result<Component> result = Component.Create(testString);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Subcomponents.Count);
        Assert.True(subcomponentStrings.SequenceEqual(result.Value.Subcomponents.Select(x => x.RawValue)));
    }

    [Fact]
    public void Create_ReturnsCorrectNumberOfSubcomponents_WhenValueContainsTrailingSubcomponentSeparator()
    {
        var subcomponentStrings = new List<string> { "John", "Douglas", string.Empty };
        var testString = string.Join("&", subcomponentStrings);

        Result<Component> result = Component.Create(testString);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Subcomponents.Count);
        Assert.True(subcomponentStrings.SequenceEqual(result.Value.Subcomponents.Select(x => x.RawValue)));
    }

    [Fact]
    public void Create_PreservesDuplicateSubcomponents_WhenValueContainsDuplicates()
    {
        Result<Component> result = Component.Create("Smith&Smith");

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Subcomponents.Count);
    }

    [Fact]
    public void ToHl7String_ReturnsRawValue()
    {
        Result<Component> result = Component.Create("John&Smith");

        Assert.True(result.IsSuccess);
        Assert.Equal("John&Smith", result.Value.ToHl7String());
    }

    [Fact]
    public void Create_ReturnsFailure_WhenValueIsNull()
    {
        Result<Component> result = Component.Create(null);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsFailure_WhenValueContainsFieldSeparatorDelimiter()
    {
        Result<Component> result = Component.Create("Smith|John");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsFailure_WhenValueContainsComponentSeparatorDelimiter()
    {
        Result<Component> result = Component.Create("Smith^John");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsFailure_WhenValueContainsRepetitionSeparatorDelimiter()
    {
        Result<Component> result = Component.Create("Smith~John");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsFailure_WhenValueContainsEscapeCharacter()
    {
        Result<Component> result = Component.Create("John\\Smith");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsFailureWithIndexConstant_WhenSubcomponentIsInvalid()
    {
        Result<Component> result = Component.Create("Smith&Sm|ith");

        Assert.False(result.IsSuccess);
        Assert.Contains("1", result.Error);
        Assert.Contains("|", result.Error);
    }
}
