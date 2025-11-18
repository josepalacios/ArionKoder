using DocumentManagement.Application.DTOs.Requests;
using FluentValidation;

namespace DocumentManagement.Application.Validators
{
    public sealed class ShareDocumentRequestValidator : AbstractValidator<ShareDocumentRequest>
    {
        public ShareDocumentRequestValidator()
        {
            RuleFor(x => x.SharedWithEmail)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.")
                .MaximumLength(255).WithMessage("Email cannot exceed 255 characters.");

            RuleFor(x => x.PermissionLevel)
                .IsInEnum().WithMessage("Invalid permission level.");
        }
    }

}
