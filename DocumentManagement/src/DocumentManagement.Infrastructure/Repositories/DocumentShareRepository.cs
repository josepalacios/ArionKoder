using DocumentManagement.Domain.Interfaces;
using DocumentManagement.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DocumentManagement.Infrastructure.Repositories
{
    public sealed class DocumentShareRepository(ApplicationDbContext context)
    : Repository<DocumentShare>(context), IDocumentShareRepository
    {
        public async Task<IEnumerable<DocumentShare>> GetActiveSharesForDocumentAsync(
            int documentId,
            CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Where(ds => ds.DocumentId == documentId && ds.RevokedAt == null)
                .OrderByDescending(ds => ds.CreatedAt)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<DocumentShare?> GetShareAsync(
            int documentId,
            string sharedWithEmail,
            CancellationToken cancellationToken = default)
        {
            return await DbSet
                .FirstOrDefaultAsync(ds =>
                    ds.DocumentId == documentId &&
                    ds.SharedWithEmail == sharedWithEmail &&
                    ds.RevokedAt == null,
                    cancellationToken);
        }

        public async Task RevokeShareAsync(
            int documentId,
            string sharedWithEmail,
            CancellationToken cancellationToken = default)
        {
            var share = await GetShareAsync(documentId, sharedWithEmail, cancellationToken);

            if (share != null)
            {
                share.RevokedAt = DateTime.UtcNow;
                DbSet.Update(share);
            }
        }
    }
}
