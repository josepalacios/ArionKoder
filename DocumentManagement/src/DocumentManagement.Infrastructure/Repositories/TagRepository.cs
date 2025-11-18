using DocumentManagement.Domain.Interfaces;
using DocumentManagement.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DocumentManagement.Infrastructure.Repositories
{
    public sealed class TagRepository(ApplicationDbContext context)
    : Repository<Tag>(context), ITagRepository
    {
        public async Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await DbSet
                .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower(), cancellationToken);
        }

        public async Task<IEnumerable<Tag>> GetOrCreateTagsAsync(
            IEnumerable<string> tagNames,
            CancellationToken cancellationToken = default)
        {
            var tags = new List<Tag>();

            foreach (var tagName in tagNames.Distinct())
            {
                var tag = await GetByNameAsync(tagName, cancellationToken);

                if (tag == null)
                {
                    tag = new Tag { Name = tagName };
                    await DbSet.AddAsync(tag, cancellationToken);
                }

                tags.Add(tag);
            }

            return tags;
        }
    }

}
