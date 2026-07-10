// Copyright (c) Christopher Wenzlick. All rights reserved.

using HL7Parser.Application.Validation;

namespace HL7Parser.Tests.Unit.Application.Validation;

public class ValidationResultTests
{
    private static readonly ValidationIssue WarningIssue = new (ValidationSeverity.Warning, "MSH-4", "CODE", "Warning description");
    private static readonly ValidationIssue ErrorIssue = new (ValidationSeverity.Error, "MSH-7", "CODE", "Error description");

    [Fact]
    public void Create_ReturnsValidResult_WhenNoIssues()
    {
        ValidationResult result = ValidationResult.Create([]);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Create_ReturnsValidResult_WhenOnlyWarnings()
    {
        ValidationResult result = ValidationResult.Create([WarningIssue]);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Create_ReturnsInvalidResult_WhenAnyError()
    {
        ValidationResult result = ValidationResult.Create([ErrorIssue]);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Create_ReturnsInvalidResult_WhenMixedErrorsAndWarnings()
    {
        ValidationResult result = ValidationResult.Create([WarningIssue, ErrorIssue]);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Create_ThrowsArgumentNullException_WhenIssuesIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => ValidationResult.Create(null!));
    }
}
