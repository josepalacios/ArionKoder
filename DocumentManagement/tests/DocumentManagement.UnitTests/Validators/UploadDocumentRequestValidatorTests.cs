using DocumentManagement.Application.DTOs.Requests;
using DocumentManagement.Application.Validators;
using DocumentManagement.Domain.Enums;
using FluentValidation.TestHelper;
using Xunit;

namespace DocumentManagement.UnitTests.Validators;

public sealed class UploadDocumentRequestValidatorTests
{
    private readonly UploadDocumentRequestValidator _validator;

    public UploadDocumentRequestValidatorTests()
    {
        _validator = new UploadDocumentRequestValidator();
    }

    [Fact]
    public void Validate_WithValidRequest_ShouldNotHaveErrors()
    {
        // Arrange
        var request = new UploadDocumentRequest(
            "Valid Title",
            "Valid Description",
            new[] { "tag1", "tag2" },
            AccessType.Private);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithInvalidTitle_ShouldHaveError(string? title)
    {
        // Arrange
        var request = new UploadDocumentRequest(
            title!,
            null,
            null,
            AccessType.Private);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validate_WithTitleTooLong_ShouldHaveError()
    {
        // Arrange
        var longTitle = new string('a', 256);
        var request = new UploadDocumentRequest(
            longTitle,
            null,
            null,
            AccessType.Private);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title cannot exceed 255 characters.");
    }

    [Fact]
    public void Validate_WithDescriptionTooLong_ShouldHaveError()
    {
        // Arrange
        var longDescription = new string('a', 2001);
        var request = new UploadDocumentRequest(
            "Valid Title",
            longDescription,
            null,
            AccessType.Private);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description cannot exceed 2000 characters.");
    }

    [Fact]
    public void Validate_WithEmptyTags_ShouldHaveError()
    {
        // Arrange
        var request = new UploadDocumentRequest(
            "Valid Title",
            null,
            new[] { "valid", "", "tags" },
            AccessType.Private);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Tags);
    }

    [Theory]
    [InlineData("pdf")]
    [InlineData("docx")]
    [InlineData("txt")]
    public void IsValidContentType_WithAllowedTypes_ReturnsTrue(string extension)
    {
        // Arrange
        var contentType = extension switch
        {
            "pdf" => "application/pdf",
            "docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "txt" => "text/plain",
            _ => throw new ArgumentException()
        };

        // Act
        var result = UploadDocumentRequestValidator.IsValidContentType(contentType);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("application/x-msdownload")]
    [InlineData("image/jpeg")]
    [InlineData("video/mp4")]
    public void IsValidContentType_WithDisallowedTypes_ReturnsFalse(string contentType)
    {
        // Act
        var result = UploadDocumentRequestValidator.IsValidContentType(contentType);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(1024)]
    [InlineData(10485760)] // Exactly 10MB
    public void IsValidFileSize_WithinLimit_ReturnsTrue(long fileSize)
    {
        // Act
        var result = UploadDocumentRequestValidator.IsValidFileSize(fileSize);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(10485761)] // 10MB + 1 byte
    public void IsValidFileSize_OutsideLimit_ReturnsFalse(long fileSize)
    {
        // Act
        var result = UploadDocumentRequestValidator.IsValidFileSize(fileSize);

        // Assert
        Assert.False(result);
    }
}