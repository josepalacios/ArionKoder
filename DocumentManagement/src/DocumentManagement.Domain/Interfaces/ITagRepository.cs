namespace DocumentManagement.Domain.Interfaces
{
    public interface ITagRepository : IRepository<Tag>
    {
        Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<IEnumerable<Tag>> GetOrCreateTagsAsync(
            IEnumerable<string> tagNames,
            CancellationToken cancellationToken = default);
    }
}
