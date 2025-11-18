using AutoMapper;
using DocumentManagement.Application.Common;
using DocumentManagement.Application.DTOs.Common;
using DocumentManagement.Application.DTOs.Requests;
using DocumentManagement.Application.DTOs.Responses;
using DocumentManagement.Application.Services.Interfaces;
using DocumentManagement.Application.Validators;
using DocumentManagement.Domain.Enums;
using DocumentManagement.Domain.Interfaces;
using FluentValidation;

namespace DocumentManagement.Application.Services.Implementations;

public sealed class DocumentService(
    IUnitOfWork unitOfWork,
    IFileStorageService fileStorageService,
    IMapper mapper,
    IValidator<UploadDocumentRequest> uploadValidator,
    IValidator<UpdateDocumentRequest> updateValidator) : IDocumentService
{
    public async Task<Result<DocumentResponse>> UploadDocumentAsync(
        UploadDocumentRequest request,
        Stream fileStream,
        string fileName,
        string contentType,
        long fileSizeBytes,
        string userEmail,
        CancellationToken cancellationToken = default)
    {
        // Validate request
        var validationResult = await uploadValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<DocumentResponse>.Failure(
                validationResult.Errors.First().ErrorMessage,
                400);
        }

        // Additional file validations
        if (!UploadDocumentRequestValidator.IsValidContentType(contentType))
        {
            return Result<DocumentResponse>.Failure(
                "Invalid file type. Only PDF, DOCX, and TXT files are allowed.",
                400);
        }

        if (!UploadDocumentRequestValidator.IsValidFileSize(fileSizeBytes))
        {
            return Result<DocumentResponse>.Failure(
                UploadDocumentRequestValidator.GetMaxFileSizeMessage(),
                400);
        }

        try
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);

            // Save file to storage
            var storagePath = await fileStorageService.SaveFileAsync(
                fileStream,
                fileName,
                contentType,
                cancellationToken);

            // Create document entity
            var document = new Document
            {
                Title = request.Title,
                Description = request.Description,
                FileName = fileName,
                StoragePath = storagePath,
                ContentType = contentType,
                FileSizeBytes = fileSizeBytes,
                UploadedBy = userEmail,
                AccessType = request.AccessType
            };

            // Add document
            await unitOfWork.Documents.AddAsync(document, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // Handle tags if provided
            if (request.Tags != null && request.Tags.Any())
            {
                var tags = await unitOfWork.Tags.GetOrCreateTagsAsync(
                    request.Tags,
                    cancellationToken);

                await unitOfWork.SaveChangesAsync(cancellationToken);

                // Create document-tag relationships
                foreach (var tag in tags)
                {
                    document.DocumentTags.Add(new DocumentTag
                    {
                        DocumentId = document.Id,
                        TagId = tag.Id
                    });
                }

                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            await unitOfWork.CommitTransactionAsync(cancellationToken);

            // Reload document with tags
            var documentWithTags = await unitOfWork.Documents.GetByIdWithDetailsAsync(
                document.Id,
                cancellationToken);

            var response = mapper.Map<DocumentResponse>(documentWithTags);
            return Result<DocumentResponse>.Success(response);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);

            // Clean up file if it was saved
            // This is a best effort cleanup, ignore errors
            try
            {
                if (!string.IsNullOrEmpty(ex.Message))
                {
                    // File cleanup logic here if needed
                }
            }
            catch { /* Ignore cleanup errors */ }

            return Result<DocumentResponse>.Failure(
                $"Failed to upload document: {ex.Message}",
                500);
        }
    }

    public async Task<Result<PagedResult<DocumentResponse>>> GetDocumentsAsync(
        SearchDocumentsRequest request,
        string userEmail,
        UserRole userRole,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var (documents, totalCount) = await unitOfWork.Documents.GetPagedAsync(
                request.PageNumber,
                request.PageSize,
                request.SearchTerm,
                userEmail,
                userRole,
                cancellationToken);

            var documentResponses = mapper.Map<IEnumerable<DocumentResponse>>(documents);

            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var pagedResult = new PagedResult<DocumentResponse>(
                documentResponses,
                request.PageNumber,
                request.PageSize,
                totalCount,
                totalPages);

            return Result<PagedResult<DocumentResponse>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            return Result<PagedResult<DocumentResponse>>.Failure(
                $"Failed to retrieve documents: {ex.Message}",
                500);
        }
    }

    public async Task<Result<DocumentDetailResponse>> GetDocumentByIdAsync(
        int id,
        string userEmail,
        UserRole userRole,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if user has access
            var hasAccess = await unitOfWork.Documents.HasAccessAsync(
                id,
                userEmail,
                userRole,
                cancellationToken);

            if (!hasAccess)
            {
                return Result<DocumentDetailResponse>.Failure(
                    "You don't have permission to access this document.",
                    403);
            }

            var document = await unitOfWork.Documents.GetByIdWithDetailsAsync(
                id,
                cancellationToken);

            if (document == null)
            {
                return Result<DocumentDetailResponse>.Failure(
                    $"Document with ID {id} not found.",
                    404);
            }

            var response = mapper.Map<DocumentDetailResponse>(document);
            return Result<DocumentDetailResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<DocumentDetailResponse>.Failure(
                $"Failed to retrieve document: {ex.Message}",
                500);
        }
    }

    public async Task<Result<Stream>> DownloadDocumentAsync(
        int id,
        string userEmail,
        UserRole userRole,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if user has access
            var hasAccess = await unitOfWork.Documents.HasAccessAsync(
                id,
                userEmail,
                userRole,
                cancellationToken);

            if (!hasAccess)
            {
                return Result<Stream>.Failure(
                    "You don't have permission to download this document.",
                    403);
            }

            var document = await unitOfWork.Documents.GetByIdAsync(id, cancellationToken);

            if (document == null)
            {
                return Result<Stream>.Failure(
                    $"Document with ID {id} not found.",
                    404);
            }

            // Get file stream
            var fileStream = await fileStorageService.GetFileStreamAsync(
                document.StoragePath,
                cancellationToken);

            return Result<Stream>.Success(fileStream);
        }
        catch (FileNotFoundException)
        {
            return Result<Stream>.Failure(
                "Document file not found in storage.",
                404);
        }
        catch (Exception ex)
        {
            return Result<Stream>.Failure(
                $"Failed to download document: {ex.Message}",
                500);
        }
    }

    public async Task<Result<DocumentResponse>> UpdateDocumentAsync(
        int id,
        UpdateDocumentRequest request,
        string userEmail,
        UserRole userRole,
        CancellationToken cancellationToken = default)
    {
        // Validate request
        var validationResult = await updateValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<DocumentResponse>.Failure(
                validationResult.Errors.First().ErrorMessage,
                400);
        }

        try
        {
            // Check if user has write access
            var hasWriteAccess = await unitOfWork.Documents.HasWriteAccessAsync(
                id,
                userEmail,
                userRole,
                cancellationToken);

            if (!hasWriteAccess)
            {
                return Result<DocumentResponse>.Failure(
                    "You don't have permission to update this document.",
                    403);
            }

            var document = await unitOfWork.Documents.GetByIdWithDetailsAsync(
                id,
                cancellationToken);

            if (document == null)
            {
                return Result<DocumentResponse>.Failure(
                    $"Document with ID {id} not found.",
                    404);
            }

            await unitOfWork.BeginTransactionAsync(cancellationToken);

            // Update fields
            if (request.Title != null)
            {
                document.Title = request.Title;
            }

            if (request.Description != null)
            {
                document.Description = request.Description;
            }

            if (request.AccessType.HasValue)
            {
                document.AccessType = request.AccessType.Value;
            }

            document.UpdatedAt = DateTime.UtcNow;

            // Handle tags update
            if (request.Tags != null)
            {
                // Remove existing tags
                document.DocumentTags.Clear();

                // Add new tags
                var tags = await unitOfWork.Tags.GetOrCreateTagsAsync(
                    request.Tags,
                    cancellationToken);

                await unitOfWork.SaveChangesAsync(cancellationToken);

                foreach (var tag in tags)
                {
                    document.DocumentTags.Add(new DocumentTag
                    {
                        DocumentId = document.Id,
                        TagId = tag.Id
                    });
                }
            }

            await unitOfWork.Documents.UpdateAsync(document, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            // Reload document with updated tags
            var updatedDocument = await unitOfWork.Documents.GetByIdWithDetailsAsync(
                id,
                cancellationToken);

            var response = mapper.Map<DocumentResponse>(updatedDocument);
            return Result<DocumentResponse>.Success(response);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);

            return Result<DocumentResponse>.Failure(
                $"Failed to update document: {ex.Message}",
                500);
        }
    }

    public async Task<Result> DeleteDocumentAsync(
        int id,
        string userEmail,
        UserRole userRole,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if user has write access (delete requires write)
            var hasWriteAccess = await unitOfWork.Documents.HasWriteAccessAsync(
                id,
                userEmail,
                userRole,
                cancellationToken);

            if (!hasWriteAccess)
            {
                return Result.Failure(
                    "You don't have permission to delete this document.",
                    403);
            }

            var document = await unitOfWork.Documents.GetByIdAsync(id, cancellationToken);

            if (document == null)
            {
                return Result.Failure(
                    $"Document with ID {id} not found.",
                    404);
            }

            await unitOfWork.BeginTransactionAsync(cancellationToken);

            // Delete from database
            await unitOfWork.Documents.DeleteAsync(document, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // Delete file from storage
            await fileStorageService.DeleteFileAsync(
                document.StoragePath,
                cancellationToken);

            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);

            return Result.Failure(
                $"Failed to delete document: {ex.Message}",
                500);
        }
    }

    public async Task<Result<IEnumerable<DocumentResponse>>> SearchDocumentsAsync(
        string searchTerm,
        string userEmail,
        UserRole userRole,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return Result<IEnumerable<DocumentResponse>>.Failure(
                "Search term cannot be empty.",
                400);
        }

        try
        {
            var documents = await unitOfWork.Documents.SearchAsync(
                searchTerm,
                userEmail,
                userRole,
                cancellationToken);

            var response = mapper.Map<IEnumerable<DocumentResponse>>(documents);
            return Result<IEnumerable<DocumentResponse>>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<DocumentResponse>>.Failure(
                $"Failed to search documents: {ex.Message}",
                500);
        }
    }
}