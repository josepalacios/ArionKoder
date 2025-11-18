using DocumentManagement.Application.Common;
using DocumentManagement.Application.DTOs.Common;
using DocumentManagement.Application.DTOs.Responses;
using DocumentManagement.Domain.Enums;

namespace DocumentManagement.Application.Services.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(
            string userEmail,
            string action,
            string entityType,
            int? entityId,
            string? ipAddress,
            string? details = null,
            CancellationToken cancellationToken = default);

        Task<Result<PagedResult<AuditLogResponse>>> GetAuditLogsAsync(
            int pageNumber,
            int pageSize,
            string? userEmail,
            DateTime? fromDate,
            DateTime? toDate,
            CancellationToken cancellationToken = default);

        Task<Result<IEnumerable<AuditLogResponse>>> GetDocumentAuditTrailAsync(
            int documentId,
            string userEmail,
            UserRole userRole,
            CancellationToken cancellationToken = default);
    }
}
