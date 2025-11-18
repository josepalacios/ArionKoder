using AutoMapper;
using DocumentManagement.Application.Common;
using DocumentManagement.Application.DTOs.Common;
using DocumentManagement.Application.DTOs.Responses;
using DocumentManagement.Application.Services.Interfaces;
using DocumentManagement.Domain.Enums;
using DocumentManagement.Domain.Interfaces;

namespace DocumentManagement.Application.Services.Implementations
{
    public sealed class AuditService(
     IUnitOfWork unitOfWork,
     IMapper mapper) : IAuditService
    {
        public async Task LogAsync(
            string userEmail,
            string action,
            string entityType,
            int? entityId,
            string? ipAddress,
            string? details = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    UserEmail = userEmail,
                    Action = action,
                    EntityType = entityType,
                    EntityId = entityId,
                    IpAddress = ipAddress,
                    Details = details,
                    DocumentId = entityType == "Document" ? entityId : null
                };

                await unitOfWork.AuditLogs.AddAsync(auditLog, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception)
            {
                // Log error but don't throw - auditing failure shouldn't break the app
                // In production, this should log to a monitoring service
            }
        }

        public async Task<Result<PagedResult<AuditLogResponse>>> GetAuditLogsAsync(
            int pageNumber,
            int pageSize,
            string? userEmail,
            DateTime? fromDate,
            DateTime? toDate,
            CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1)
            {
                return Result<PagedResult<AuditLogResponse>>.Failure(
                    "Page number must be greater than 0.",
                    400);
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return Result<PagedResult<AuditLogResponse>>.Failure(
                    "Page size must be between 1 and 100.",
                    400);
            }

            try
            {
                var (logs, totalCount) = await unitOfWork.AuditLogs.GetPagedAsync(
                    pageNumber,
                    pageSize,
                    userEmail,
                    fromDate,
                    toDate,
                    cancellationToken);

                var logResponses = mapper.Map<IEnumerable<AuditLogResponse>>(logs);

                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var pagedResult = new PagedResult<AuditLogResponse>(
                    logResponses,
                    pageNumber,
                    pageSize,
                    totalCount,
                    totalPages);

                return Result<PagedResult<AuditLogResponse>>.Success(pagedResult);
            }
            catch (Exception ex)
            {
                return Result<PagedResult<AuditLogResponse>>.Failure(
                    $"Failed to retrieve audit logs: {ex.Message}",
                    500);
            }
        }

        public async Task<Result<IEnumerable<AuditLogResponse>>> GetDocumentAuditTrailAsync(
            int documentId,
            string userEmail,
            UserRole userRole,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Check if user has access to the document
                var hasAccess = await unitOfWork.Documents.HasAccessAsync(
                    documentId,
                    userEmail,
                    userRole,
                    cancellationToken);

                if (!hasAccess)
                {
                    return Result<IEnumerable<AuditLogResponse>>.Failure(
                        "You don't have permission to view audit trail for this document.",
                        403);
                }

                // Verify document exists
                var document = await unitOfWork.Documents.GetByIdAsync(documentId, cancellationToken);
                if (document == null)
                {
                    return Result<IEnumerable<AuditLogResponse>>.Failure(
                        $"Document with ID {documentId} not found.",
                        404);
                }

                var logs = await unitOfWork.AuditLogs.GetDocumentAuditTrailAsync(
                    documentId,
                    cancellationToken);

                var response = mapper.Map<IEnumerable<AuditLogResponse>>(logs);
                return Result<IEnumerable<AuditLogResponse>>.Success(response);
            }
            catch (Exception ex)
            {
                return Result<IEnumerable<AuditLogResponse>>.Failure(
                    $"Failed to retrieve document audit trail: {ex.Message}",
                    500);
            }
        }
    }
}
