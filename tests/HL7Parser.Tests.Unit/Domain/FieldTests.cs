// Copyright (c) Christopher Wenzlick. All rights reserved.

using HL7Parser.Domain;
using HL7Parser.Domain.Common;

namespace HL7Parser.Tests.Unit.Domain;

public class FieldTests
{
    [Fact]
    public void Create_ReturnsSingleEmptyRepetition_WhenValueIsEmpty()
    {
        Result<Field> result = Field.Create(string.Empty);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Repetitions);
        Assert.Equal(string.Empty, result.Value.Repetitions.First().ToHl7String());
    }

    [Fact]
    public void Create_ReturnsSingleRepetition_WhenValueContainsNoDelimiters()
    {
        Result<Field> result = Field.Create("John");

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Repetitions);
    }

    [Fact]
    public void Create_ReturnsSuccess_WhenValueContainsNonDelimiterSpecialCharacters()
    {
        Result<Field> result = Field.Create("John-Paul");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsMultipleRepetitionsInCorrectOrder_WhenValueContainsRepetitionSeparator()
    {
        var repetitionStrings = new List<string> { "John", "Douglas", "Smith" };
        var testString = string.Join("~", repetitionStrings);

        Result<Field> result = Field.Create(testString);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Repetitions.Count);
        Assert.True(repetitionStrings.SequenceEqual(result.Value.Repetitions.Select(x => x.ToHl7String())));
    }

    [Fact]
    public void Create_ReturnsCorrectNumberOfRepetitions_WhenValueContainsTrailingRepetitionSeparator()
    {
        var repetitionStrings = new List<string> { "John", "Douglas", string.Empty };
        var testString = string.Join("~", repetitionStrings);

        Result<Field> result = Field.Create(testString);

        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value.Repetitions.Count);
        Assert.True(repetitionStrings.SequenceEqual(result.Value.Repetitions.Select(x => x.ToHl7String())));
    }

    [Fact]
    public void Create_PreservesDuplicateRepetitions_WhenValueContainsDuplicates()
    {
        Result<Field> result = Field.Create("Smith~Smith");

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Repetitions.Count);
        Assert.Equal(result.Value.Repetitions[0], result.Value.Repetitions[1]);
    }

    [Fact]
    public void ToHl7String_ReturnsRawValue()
    {
        Result<Field> result = Field.Create("John&Smith^Male~Jane&Smith^Female");

        Assert.True(result.IsSuccess);
        Assert.Equal("John&Smith^Male~Jane&Smith^Female", result.Value.ToHl7String());
    }

    [Fact]
    public void Create_ReturnsFailure_WhenValueIsNull()
    {
        Result<Field> result = Field.Create(null);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsFailure_WhenValueContainsFieldSeparatorDelimiter()
    {
        Result<Field> result = Field.Create("Smith|John");

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_ReturnsFailureWithIndexConstant_WhenRepetitionIsInvalid()
    {
        Result<Field> result = Field.Create("Smith~Sm|ith");

        Assert.False(result.IsSuccess);
        Assert.Contains("1", result.Error);
        Assert.Contains("|", result.Error);
    }
}
