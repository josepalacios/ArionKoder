using DocumentManagement.Domain.Enums;
using DocumentManagement.Domain.Interfaces;
using DocumentManagement.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DocumentManagement.Infrastructure.Repositories
{
    public sealed class DocumentRepository(ApplicationDbContext context)
    : Repository<Document>(context), IDocumentRepository
    {
        public async Task<(IEnumerable<Document> Documents, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm,
            string userEmail,
            UserRole userRole,
            CancellationToken cancellationToken = default)
        {
            var query = BuildAccessibleDocumentsQuery(userEmail, userRole);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = ApplySearchFilter(query, searchTerm);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var documents = await query
                .OrderByDescending(d => d.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(d => d.DocumentTags)
                    .ThenInclude(dt => dt.Tag)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return (documents, totalCount);
        }

        public async Task<Document?> GetByIdWithDetailsAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await DbSet
                .Include(d => d.DocumentTags)
                    .ThenInclude(dt => dt.Tag)
                .Include(d => d.DocumentShares.Where(ds => ds.RevokedAt == null))
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<Document>> SearchAsync(
            string searchTerm,
            string userEmail,
            UserRole userRole,
            CancellationToken cancellationToken = default)
        {
            var query = BuildAccessibleDocumentsQuery(userEmail, userRole);
            query = ApplySearchFilter(query, searchTerm);

            return await query
                .OrderByDescending(d => d.CreatedAt)
                .Include(d => d.DocumentTags)
                    .ThenInclude(dt => dt.Tag)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> HasAccessAsync(
            int documentId,
            string userEmail,
            UserRole userRole,
            CancellationToken cancellationToken = default)
        {
            return await BuildAccessibleDocumentsQuery(userEmail, userRole)
                .AnyAsync(d => d.Id == documentId, cancellationToken);
        }

        public async Task<bool> HasWriteAccessAsync(
            int documentId,
            string userEmail,
            UserRole userRole,
            CancellationToken cancellationToken = default)
        {
            // Admin and Manager have write access to all documents
            if (userRole is UserRole.Admin or UserRole.Manager)
            {
                return await DbSet.AnyAsync(d => d.Id == documentId, cancellationToken);
            }

            // Owner has write access
            var document = await DbSet
                .FirstOrDefaultAsync(d => d.Id == documentId, cancellationToken);

            if (document == null)
                return false;

            if (document.UploadedBy == userEmail)
                return true;

            // Check if user has Write permission through sharing
            return await Context.DocumentShares
                .AnyAsync(ds =>
                    ds.DocumentId == documentId &&
                    ds.SharedWithEmail == userEmail &&
                    ds.PermissionLevel == PermissionLevel.Write &&
                    ds.RevokedAt == null,
                    cancellationToken);
        }

        private IQueryable<Document> BuildAccessibleDocumentsQuery(string userEmail, UserRole userRole)
        {
            // Admin and Manager can see all documents
            if (userRole is UserRole.Admin or UserRole.Manager)
            {
                return DbSet;
            }

            // Others can see: Public, their own, and shared with them
            return DbSet.Where(d =>
                d.AccessType == AccessType.Public ||
                d.UploadedBy == userEmail ||
                d.DocumentShares.Any(ds =>
                    ds.SharedWithEmail == userEmail &&
                    ds.RevokedAt == null));
        }

        private static IQueryable<Document> ApplySearchFilter(IQueryable<Document> query, string searchTerm)
        {
            var lowerSearchTerm = searchTerm.ToLower();

            return query.Where(d =>
                d.Title.ToLower().Contains(lowerSearchTerm) ||
                (d.Description != null && d.Description.ToLower().Contains(lowerSearchTerm)) ||
                d.ContentType.ToLower().Contains(lowerSearchTerm) ||
                d.DocumentTags.Any(dt => dt.Tag.Name.ToLower().Contains(lowerSearchTerm)));
        }
    }
}
