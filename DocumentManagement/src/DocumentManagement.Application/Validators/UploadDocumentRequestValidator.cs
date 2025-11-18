using DocumentManagement.Application.DTOs.Requests;
using FluentValidation;

namespace DocumentManagement.Application.Validators
{
    public sealed class UploadDocumentRequestValidator : AbstractValidator<UploadDocumentRequest>
    {
        private static readonly string[] AllowedContentTypes =
        [
            "application/pdf",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "text/plain"
        ];

        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB

        public UploadDocumentRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(255).WithMessage("Title cannot exceed 255 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            RuleFor(x => x.Tags)
                .Must(tags => tags == null || tags.All(t => !string.IsNullOrWhiteSpace(t)))
                .WithMessage("Tags cannot be empty or whitespace.")
                .Must(tags => tags == null || tags.All(t => t.Length <= 50))
                .WithMessage("Each tag cannot exceed 50 characters.")
                .When(x => x.Tags != null);

            RuleFor(x => x.AccessType)
                .IsInEnum().WithMessage("Invalid access type.");
        }

        public static bool IsValidContentType(string contentType) =>
            AllowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase);

        public static bool IsValidFileSize(long fileSizeBytes) =>
            fileSizeBytes > 0 && fileSizeBytes <= MaxFileSizeBytes;

        public static string GetMaxFileSizeMessage() =>
            $"File size cannot exceed {MaxFileSizeBytes / 1024 / 1024}MB.";
    }
}
