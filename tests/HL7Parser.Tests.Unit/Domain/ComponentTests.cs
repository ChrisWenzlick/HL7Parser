// Copyright (c) Christopher Wenzlick. All rights reserved.

using HL7Parser.Domain;
using HL7Parser.Domain.Common;

namespace HL7Parser.Tests.Unit.Domain;

public class ComponentTests
{
    private static readonly EncodingCharacters DefaultEncodingCharacters =
        EncodingCharacters.Create("|^~\\&").Value;

    [Fact]
    public void Create_ReturnsSingleEmptySubcomponent_WhenValueIsEmpty()
    {
        Result<Component> result = Component.Create(string.Empty, DefaultEncodingCharacters);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Subcomponents);
        Assert.Equal(string.Empty, result.Value.Subcomponents.First().RawValue);
    }

    [Fact]
    public void Create_ReturnsSingleSubcomponent_WhenValueContainsNoDelimiters()
    {
        Result<Component> result = Component.Create("John", DefaultEncodingCharacters);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Subcomponents);
    }

    [Fact]
    public void Create_ReturnsSuccess_WhenValueContainsNonDelimiterSpecialCharacters()
    {
        Result<Component> result = Component.Create("John-Paul", DefaultEncodingCharacters);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsMultipleSubcomponentsInCorrectOrder_WhenValueContainsSubcomponentSeparator()
    {
        var subcomponentStrings = new List<string> { "John", "Douglas", "Smith" };
        var testString = string.Join("&", subcomponentStrings);

        Result<Component> result = Component.Create(testString, DefaultEncodingCharacters);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Subcomponents.Count);
        Assert.True(subcomponentStrings.SequenceEqual(result.Value.Subcomponents.Select(x => x.RawValue)));
    }

    [Fact]
    public void Create_ReturnsCorrectNumberOfSubcomponents_WhenValueContainsTrailingSubcomponentSeparator()
    {
        var subcomponentStrings = new List<string> { "John", "Douglas", string.Empty };
        var testString = string.Join("&", subcomponentStrings);

        Result<Component> result = Component.Create(testString, DefaultEncodingCharacters);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Subcomponents.Count);
        Assert.True(subcomponentStrings.SequenceEqual(result.Value.Subcomponents.Select(x => x.RawValue)));
    }

    [Fact]
    public void Create_PreservesDuplicateSubcomponents_WhenValueContainsDuplicates()
    {
        Result<Component> result = Component.Create("Smith&Smith", DefaultEncodingCharacters);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Subcomponents.Count);
        Assert.Equal(result.Value.Subcomponents[0], result.Value.Subcomponents[1]);
    }

    [Fact]
    public void ToHl7String_ReturnsOriginalValue_WhenValueContainsSubcomponentSeparator()
    {
        Result<Component> result = Component.Create("John&Smith", DefaultEncodingCharacters);

        Assert.True(result.IsSuccess);
        Assert.Equal("John&Smith", result.Value.ToHl7String());
    }

    [Fact]
    public void Create_ReturnsFailureWithIndexConstant_WhenSubcomponentIsInvalid()
    {
        Result<Component> result = Component.Create("Smith&Sm|ith", DefaultEncodingCharacters);

        Assert.False(result.IsSuccess);
        Assert.Contains("1", result.Error);
        Assert.Contains("|", result.Error);
    }

    [Fact]
    public void Indexer_ReturnsSameInstance_AsSubcomponentsProperty()
    {
        Component component = Component.Create("John&Jane", DefaultEncodingCharacters).Value;

        Assert.Same(component.Subcomponents[0], component[0]);
        Assert.Same(component.Subcomponents[1], component[1]);
    }

    [Fact]
    public void Count_MatchesSubcomponentsCount_WhenComponentHasMultipleSubcomponents()
    {
        Component component = Component.Create("John&Jane&Bob", DefaultEncodingCharacters).Value;

        Assert.Equal(component.Subcomponents.Count, component.Count);
    }

    [Fact]
    public void GetEnumerator_YieldsElementsInSameOrder_AsSubcomponentsProperty()
    {
        Component component = Component.Create("John&Jane&Bob", DefaultEncodingCharacters).Value;

        Assert.True(component.Subcomponents.SequenceEqual(component));
    }
}
