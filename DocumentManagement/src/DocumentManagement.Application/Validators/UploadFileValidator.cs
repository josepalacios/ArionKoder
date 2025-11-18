using DocumentManagement.Application.DTOs.Requests;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace DocumentManagement.Application.Validators
{
    public sealed class UploadFileValidator : AbstractValidator<IFormFile>
    {
        public UploadFileValidator()
        {
            RuleFor(f => f)
                .NotNull().WithMessage("File is required.");

            RuleFor(f => f!.Length)
                .GreaterThan(0).WithMessage("File cannot be empty.");

            RuleFor(f => f!.Length)
                .LessThanOrEqualTo(10 * 1024 * 1024)
                .WithMessage("File size cannot exceed 10MB.");

            RuleFor(f => f!.ContentType)
                .Must(UploadDocumentRequestValidator.IsValidContentType)
                .WithMessage("Invalid file type.");
        }
    }
}
