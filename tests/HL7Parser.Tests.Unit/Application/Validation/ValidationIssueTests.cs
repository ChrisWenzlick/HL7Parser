// Copyright (c) Christopher Wenzlick. All rights reserved.

using HL7Parser.Application.Validation;

namespace HL7Parser.Tests.Unit.Application.Validation;

public class ValidationIssueTests
{
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenLocationIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ValidationIssue(ValidationSeverity.Error, null!, "CODE", "Description"));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenCodeIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ValidationIssue(ValidationSeverity.Error, "MSH-7", null!, "Description"));
    }

    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenDescriptionIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ValidationIssue(ValidationSeverity.Error, "MSH-7", "CODE", null!));
    }
}
