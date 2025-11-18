using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocumentManagement.Infrastructure.Data.Configurations
{
    public sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            builder.ToTable("Tags");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(t => t.CreatedAt)
                .IsRequired();

            // Unique constraint on tag name (case-insensitive)
            builder.HasIndex(t => t.Name)
                .IsUnique();
        }
    }
}
