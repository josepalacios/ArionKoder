using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocumentManagement.Infrastructure.Data.Configurations
{
    public sealed class DocumentConfiguration : IEntityTypeConfiguration<Document>
    {
        public void Configure(EntityTypeBuilder<Document> builder)
        {
            builder.ToTable("Documents");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Title)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(d => d.Description)
                .HasMaxLength(2000);

            builder.Property(d => d.FileName)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(d => d.StoragePath)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(d => d.ContentType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(d => d.UploadedBy)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(d => d.AccessType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Property(d => d.CreatedAt)
                .IsRequired();

            builder.Property(d => d.UpdatedAt);

            // Indexes for performance
            builder.HasIndex(d => d.UploadedBy);
            builder.HasIndex(d => d.AccessType);
            builder.HasIndex(d => d.CreatedAt);
            builder.HasIndex(d => new { d.Title, d.UploadedBy });
        }
    }
}
