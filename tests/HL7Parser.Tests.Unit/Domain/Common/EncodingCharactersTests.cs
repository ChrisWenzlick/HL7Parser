// Copyright (c) Christopher Wenzlick. All rights reserved.

using HL7Parser.Domain.Common;

namespace HL7Parser.Tests.Unit.Domain.Common;

public class EncodingCharactersTests
{
    [Fact]
    public void Create_FromTwoStrings_ReturnsCorrectProperties_WhenProvidedCorrectNumberOfUniqueCharacters()
    {
        var msh1 = "|";
        var msh2 = "^~\\&";
        Result<EncodingCharacters> result = EncodingCharacters.Create(msh1, msh2);

        Assert.True(result.IsSuccess);
        Assert.Equal("|", result.Value.FieldSeparator.ToString());
        Assert.Equal("^", result.Value.ComponentSeparator.ToString());
        Assert.Equal("~", result.Value.RepetitionSeparator.ToString());
        Assert.Equal("\\", result.Value.EscapeCharacter.ToString());
        Assert.Equal("&", result.Value.SubcomponentSeparator.ToString());
    }

    [Fact]
    public void Create_FromTwoStrings_ReturnsFailure_WhenMsh1IsEmpty()
    {
        var msh1 = string.Empty;
        var msh2 = "^~\\&";
        Result<EncodingCharacters> result = EncodingCharacters.Create(msh1, msh2);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_FromTwoStrings_ReturnsFailure_WhenMsh1IsLongerThanOneCharacter()
    {
        var msh1 = "|%";
        var msh2 = "^~\\&";
        Result<EncodingCharacters> result = EncodingCharacters.Create(msh1, msh2);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_FromTwoStrings_ReturnsFailure_WhenMsh2IsEmpty()
    {
        var msh1 = "|";
        var msh2 = string.Empty;
        Result<EncodingCharacters> result = EncodingCharacters.Create(msh1, msh2);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_FromTwoStrings_ReturnsFailure_WhenMsh2IsShorterThanFourCharacters()
    {
        var msh1 = "|";
        var msh2 = "^~\\";
        Result<EncodingCharacters> result = EncodingCharacters.Create(msh1, msh2);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_FromTwoStrings_ReturnsFailure_WhenMsh2IsLongerThanFourCharacters()
    {
        var msh1 = "|";
        var msh2 = "^~\\&%";
        Result<EncodingCharacters> result = EncodingCharacters.Create(msh1, msh2);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_FromTwoStrings_ReturnsFailure_WhenMsh2ContainsMsh1Character()
    {
        var msh1 = "|";
        var msh2 = "^~\\|";
        Result<EncodingCharacters> result = EncodingCharacters.Create(msh1, msh2);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_FromTwoStrings_ReturnsFailure_WhenMsh2ContainsDuplicateCharacters()
    {
        var msh1 = "|";
        var msh2 = "^~\\^";
        Result<EncodingCharacters> result = EncodingCharacters.Create(msh1, msh2);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_FromCombinedString_ReturnsCorrectProperties_WhenProvidedCorrectNumberOfUniqueCharacters()
    {
        var characterString = "|^~\\&";
        Result<EncodingCharacters> result = EncodingCharacters.Create(characterString);

        Assert.True(result.IsSuccess);
        Assert.Equal("|", result.Value.FieldSeparator.ToString());
        Assert.Equal("^", result.Value.ComponentSeparator.ToString());
        Assert.Equal("~", result.Value.RepetitionSeparator.ToString());
        Assert.Equal("\\", result.Value.EscapeCharacter.ToString());
        Assert.Equal("&", result.Value.SubcomponentSeparator.ToString());
    }

    [Fact]
    public void Create_FromCombinedString_ReturnsFailure_WhenValueIsEmpty()
    {
        var characterString = string.Empty;
        Result<EncodingCharacters> result = EncodingCharacters.Create(characterString);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_FromCombinedString_ReturnsFailure_WhenValueIsShorterThanFiveCharacters()
    {
        var characterString = "|^~\\";
        Result<EncodingCharacters> result = EncodingCharacters.Create(characterString);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_FromCombinedString_ReturnsFailure_WhenValueIsLongerThanFiveCharacters()
    {
        var characterString = "|^~\\&%";
        Result<EncodingCharacters> result = EncodingCharacters.Create(characterString);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void Create_FromCombinedString_ReturnsFailure_WhenValueContainsDuplicateCharacters()
    {
        var characterString = "|^~\\^";
        Result<EncodingCharacters> result = EncodingCharacters.Create(characterString);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public void ToHl7String_ReturnsCorrectCombinedFiveCharacterString()
    {
        var characterString = "|^~\\&";
        Result<EncodingCharacters> result = EncodingCharacters.Create(characterString);

        Assert.True(result.IsSuccess);
        Assert.Equal(characterString, result.Value.ToHl7String());
    }
}
