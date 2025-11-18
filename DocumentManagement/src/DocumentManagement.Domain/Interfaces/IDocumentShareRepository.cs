namespace DocumentManagement.Domain.Interfaces
{
    public interface IDocumentShareRepository : IRepository<DocumentShare>
    {
        Task<IEnumerable<DocumentShare>> GetActiveSharesForDocumentAsync(
            int documentId,
            CancellationToken cancellationToken = default);

        Task<DocumentShare?> GetShareAsync(
            int documentId,
            string sharedWithEmail,
            CancellationToken cancellationToken = default);

        Task RevokeShareAsync(
            int documentId,
            string sharedWithEmail,
            CancellationToken cancellationToken = default);
    }
}
