using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocumentManagement.Infrastructure.Data.Configurations
{
    public sealed class DocumentTagConfiguration : IEntityTypeConfiguration<DocumentTag>
    {
        public void Configure(EntityTypeBuilder<DocumentTag> builder)
        {
            builder.ToTable("DocumentTags");

            // Composite primary key
            builder.HasKey(dt => new { dt.DocumentId, dt.TagId });

            builder.Property(dt => dt.CreatedAt)
                .IsRequired();

            // Relationships
            builder.HasOne(dt => dt.Document)
                .WithMany(d => d.DocumentTags)
                .HasForeignKey(dt => dt.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(dt => dt.Tag)
                .WithMany(t => t.DocumentTags)
                .HasForeignKey(dt => dt.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(dt => dt.DocumentId);
            builder.HasIndex(dt => dt.TagId);
        }
    }

}
