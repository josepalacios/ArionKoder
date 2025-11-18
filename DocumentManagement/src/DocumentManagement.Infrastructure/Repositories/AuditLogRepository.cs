using DocumentManagement.Domain.Interfaces;
using DocumentManagement.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DocumentManagement.Infrastructure.Repositories
{
    public sealed class AuditLogRepository(ApplicationDbContext context)
    : Repository<AuditLog>(context), IAuditLogRepository
    {
        public async Task<(IEnumerable<AuditLog> Logs, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? userEmail,
            DateTime? fromDate,
            DateTime? toDate,
            CancellationToken cancellationToken = default)
        {
            var query = DbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(userEmail))
            {
                query = query.Where(al => al.UserEmail == userEmail);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(al => al.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(al => al.CreatedAt <= toDate.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var logs = await query
                .OrderByDescending(al => al.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return (logs, totalCount);
        }

        public async Task<IEnumerable<AuditLog>> GetDocumentAuditTrailAsync(
            int documentId,
            CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Where(al => al.DocumentId == documentId)
                .OrderByDescending(al => al.CreatedAt)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}
