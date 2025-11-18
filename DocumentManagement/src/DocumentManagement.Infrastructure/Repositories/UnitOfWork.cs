using DocumentManagement.Domain.Interfaces;
using DocumentManagement.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore.Storage;

namespace DocumentManagement.Infrastructure.Repositories
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;
        private bool _disposed;

        public IDocumentRepository Documents { get; }
        public IDocumentShareRepository DocumentShares { get; }
        public ITagRepository Tags { get; }
        public IAuditLogRepository AuditLogs { get; }

        public UnitOfWork(
            ApplicationDbContext context,
            IDocumentRepository documents,
            IDocumentShareRepository documentShares,
            ITagRepository tags,
            IAuditLogRepository auditLogs)
        {
            _context = context;
            Documents = documents;
            DocumentShares = documentShares;
            Tags = tags;
            AuditLogs = auditLogs;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _transaction?.Dispose();
                _context.Dispose();
            }
            _disposed = true;
        }
    }

}
