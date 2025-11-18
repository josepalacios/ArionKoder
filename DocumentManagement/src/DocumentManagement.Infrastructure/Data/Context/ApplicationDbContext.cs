using Microsoft.EntityFrameworkCore;

namespace DocumentManagement.Infrastructure.Data.Context
{
    public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
    {
        public DbSet<Document> Documents => Set<Document>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<DocumentTag> DocumentTags => Set<DocumentTag>();
        public DbSet<DocumentShare> DocumentShares => Set<DocumentShare>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}
