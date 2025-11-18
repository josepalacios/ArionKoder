using DocumentManagement.Application.DTOs.Requests;
using FluentValidation;

namespace DocumentManagement.Application.Validators
{
    public sealed class UpdateDocumentRequestValidator : AbstractValidator<UpdateDocumentRequest>
    {
        public UpdateDocumentRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title cannot be empty.")
                .MaximumLength(255).WithMessage("Title cannot exceed 255 characters.")
                .When(x => x.Title != null);

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters.")
                .When(x => x.Description != null);

            RuleFor(x => x.Tags)
                .Must(tags => tags.All(t => !string.IsNullOrWhiteSpace(t)))
                .WithMessage("Tags cannot be empty or whitespace.")
                .Must(tags => tags.All(t => t.Length <= 50))
                .WithMessage("Each tag cannot exceed 50 characters.")
                .When(x => x.Tags != null);

            RuleFor(x => x.AccessType)
                .IsInEnum().WithMessage("Invalid access type.")
                .When(x => x.AccessType.HasValue);

            RuleFor(x => x)
                .Must(x => x.Title != null || x.Description != null ||
                           x.Tags != null || x.AccessType.HasValue)
                .WithMessage("At least one field must be provided for update.");
        }
    }
}
