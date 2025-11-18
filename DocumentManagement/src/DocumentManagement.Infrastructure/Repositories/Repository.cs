using DocumentManagement.Domain.Interfaces;
using DocumentManagement.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DocumentManagement.Infrastructure.Repositories
{
    public abstract class Repository<T>(ApplicationDbContext context) : IRepository<T>
    where T : class
    {
        protected readonly ApplicationDbContext Context = context;
        protected readonly DbSet<T> DbSet = context.Set<T>();

        public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await DbSet.FindAsync([id], cancellationToken);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await DbSet.ToListAsync(cancellationToken);
        }

        public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await DbSet.AddAsync(entity, cancellationToken);
            return entity;
        }

        public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            DbSet.Update(entity);
            return Task.CompletedTask;
        }

        public virtual Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            DbSet.Remove(entity);
            return Task.CompletedTask;
        }
    }

}
