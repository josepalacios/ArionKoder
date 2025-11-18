using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocumentManagement.Infrastructure.Data.Configurations
{
    public sealed class DocumentShareConfiguration : IEntityTypeConfiguration<DocumentShare>
    {
        public void Configure(EntityTypeBuilder<DocumentShare> builder)
        {
            builder.ToTable("DocumentShares");

            builder.HasKey(ds => ds.Id);

            builder.Property(ds => ds.SharedWithEmail)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(ds => ds.SharedByEmail)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(ds => ds.PermissionLevel)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(ds => ds.CreatedAt)
                .IsRequired();

            builder.Property(ds => ds.RevokedAt);

            // Relationship
            builder.HasOne(ds => ds.Document)
                .WithMany(d => d.DocumentShares)
                .HasForeignKey(ds => ds.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            builder.HasIndex(ds => ds.DocumentId);
            builder.HasIndex(ds => ds.SharedWithEmail);
            builder.HasIndex(ds => new { ds.DocumentId, ds.SharedWithEmail });
            builder.HasIndex(ds => ds.RevokedAt);

            // Unique constraint: one share per document-user pair (only active)
            builder.HasIndex(ds => new { ds.DocumentId, ds.SharedWithEmail, ds.RevokedAt })
                .IsUnique()
                .HasFilter("[RevokedAt] IS NULL");
        }
    }
}
