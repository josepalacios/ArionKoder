namespace DocumentManagement.Domain.Interfaces
{
    public interface IAuditLogRepository : IRepository<AuditLog>
    {
        Task<(IEnumerable<AuditLog> Logs, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? userEmail,
            DateTime? fromDate,
            DateTime? toDate,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<AuditLog>> GetDocumentAuditTrailAsync(
            int documentId,
            CancellationToken cancellationToken = default);
    }
}
