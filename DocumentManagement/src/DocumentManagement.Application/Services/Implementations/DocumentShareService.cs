using AutoMapper;
using DocumentManagement.Application.DTOs.Common;
using DocumentManagement.Application.DTOs.Requests;
using DocumentManagement.Application.DTOs.Responses;
using DocumentManagement.Application.Services.Interfaces;
using DocumentManagement.Domain.Enums;
using DocumentManagement.Domain.Interfaces;
using FluentValidation;

namespace DocumentManagement.Application.Services.Implementations
{
    public sealed class DocumentShareService(
     IUnitOfWork unitOfWork,
     IMapper mapper,
     IValidator<ShareDocumentRequest> validator) : IDocumentShareService
    {
        public async Task<Result<DocumentShareResponse>> ShareDocumentAsync(
            int documentId,
            ShareDocumentRequest request,
            string userEmail,
            UserRole userRole,
            CancellationToken cancellationToken = default)
        {
            // Validate request
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Result<DocumentShareResponse>.Failure(
                    validationResult.Errors.First().ErrorMessage,
                    400);
            }

            try
            {
                // Get the document
                var document = await unitOfWork.Documents.GetByIdAsync(documentId, cancellationToken);

                if (document == null)
                {
                    return Result<DocumentShareResponse>.Failure(
                        $"Document with ID {documentId} not found.",
                        404);
                }

                // Check if user can share (must be owner, manager, or admin)
                var canShare = userRole is UserRole.Admin or UserRole.Manager ||
                              document.UploadedBy == userEmail;

                if (!canShare)
                {
                    return Result<DocumentShareResponse>.Failure(
                        "You don't have permission to share this document.",
                        403);
                }

                // Prevent sharing with self
                if (request.SharedWithEmail.Equals(userEmail, StringComparison.OrdinalIgnoreCase))
                {
                    return Result<DocumentShareResponse>.Failure(
                        "You cannot share a document with yourself.",
                        400);
                }

                // Check if already shared
                var existingShare = await unitOfWork.DocumentShares.GetShareAsync(
                    documentId,
                    request.SharedWithEmail,
                    cancellationToken);

                if (existingShare != null)
                {
                    return Result<DocumentShareResponse>.Failure(
                        $"Document is already shared with {request.SharedWithEmail}.",
                        409);
                }

                // Create share
                var share = new DocumentShare
                {
                    DocumentId = documentId,
                    SharedWithEmail = request.SharedWithEmail,
                    SharedByEmail = userEmail,
                    PermissionLevel = request.PermissionLevel
                };

                await unitOfWork.DocumentShares.AddAsync(share, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                var response = mapper.Map<DocumentShareResponse>(share);
                return Result<DocumentShareResponse>.Success(response);
            }
            catch (Exception ex)
            {
                return Result<DocumentShareResponse>.Failure(
                    $"Failed to share document: {ex.Message}",
                    500);
            }
        }

        public async Task<Result<IEnumerable<DocumentShareResponse>>> GetDocumentSharesAsync(
            int documentId,
            string userEmail,
            UserRole userRole,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the document
                var document = await unitOfWork.Documents.GetByIdAsync(documentId, cancellationToken);

                if (document == null)
                {
                    return Result<IEnumerable<DocumentShareResponse>>.Failure(
                        $"Document with ID {documentId} not found.",
                        404);
                }

                // Check if user can view shares (must be owner, manager, or admin)
                var canViewShares = userRole is UserRole.Admin or UserRole.Manager ||
                                   document.UploadedBy == userEmail;

                if (!canViewShares)
                {
                    return Result<IEnumerable<DocumentShareResponse>>.Failure(
                        "You don't have permission to view document shares.",
                        403);
                }

                var shares = await unitOfWork.DocumentShares.GetActiveSharesForDocumentAsync(
                    documentId,
                    cancellationToken);

                var response = mapper.Map<IEnumerable<DocumentShareResponse>>(shares);
                return Result<IEnumerable<DocumentShareResponse>>.Success(response);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<DocumentShareResponse>>.Failure(
                    $"Failed to retrieve document shares: {ex.Message}",
                    500);
            }
        }

        public async Task<Result> RevokeShareAsync(
            int documentId,
            string sharedWithEmail,
            string userEmail,
            UserRole userRole,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Get the document
                var document = await unitOfWork.Documents.GetByIdAsync(documentId, cancellationToken);

                if (document == null)
                {
                    return Result.Failure(
                        $"Document with ID {documentId} not found.",
                        404);
                }

                // Check if user can revoke (must be owner, manager, or admin)
                var canRevoke = userRole is UserRole.Admin or UserRole.Manager ||
                               document.UploadedBy == userEmail;

                if (!canRevoke)
                {
                    return Result.Failure(
                        "You don't have permission to revoke shares for this document.",
                        403);
                }

                // Get the share
                var share = await unitOfWork.DocumentShares.GetShareAsync(
                    documentId,
                    sharedWithEmail,
                    cancellationToken);

                if (share == null)
                {
                    return Result.Failure(
                        $"No active share found for {sharedWithEmail}.",
                        404);
                }

                // Revoke the share
                await unitOfWork.DocumentShares.RevokeShareAsync(
                    documentId,
                    sharedWithEmail,
                    cancellationToken);

                await unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(
                    $"Failed to revoke share: {ex.Message}",
                    500);
            }
        }
    }
}
