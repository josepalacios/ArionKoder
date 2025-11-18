using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocumentManagement.Infrastructure.Data.Configurations
{
    public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");

            builder.HasKey(al => al.Id);

            builder.Property(al => al.UserEmail)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(al => al.Action)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(al => al.EntityType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(al => al.EntityId);

            builder.Property(al => al.IpAddress)
                .HasMaxLength(45); // IPv6 max length

            builder.Property(al => al.Details)
                .HasMaxLength(2000);

            builder.Property(al => al.CreatedAt)
                .IsRequired();

            // Optional relationship to Document
            builder.HasOne(al => al.Document)
                .WithMany(d => d.AuditLogs)
                .HasForeignKey(al => al.DocumentId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes for querying
            builder.HasIndex(al => al.UserEmail);
            builder.HasIndex(al => al.CreatedAt);
            builder.HasIndex(al => al.EntityType);
            builder.HasIndex(al => al.DocumentId);
            builder.HasIndex(al => new { al.UserEmail, al.CreatedAt });
        }
    }
}
