using DocumentManagement.Domain.Enums;

namespace DocumentManagement.Domain.Interfaces
{
    public interface IDocumentRepository : IRepository<Document>
    {
        Task<(IEnumerable<Document> Documents, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm,
            string userEmail,
            UserRole userRole,
            CancellationToken cancellationToken = default);

        Task<Document?> GetByIdWithDetailsAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<Document>> SearchAsync(
            string searchTerm,
            string userEmail,
            UserRole userRole,
            CancellationToken cancellationToken = default);

        Task<bool> HasAccessAsync(
            int documentId,
            string userEmail,
            UserRole userRole,
            CancellationToken cancellationToken = default);

        Task<bool> HasWriteAccessAsync(
            int documentId,
            string userEmail,
            UserRole userRole,
            CancellationToken cancellationToken = default);
    }
}
