using DocumentManagement.Domain.Enums;

public sealed class Document : BaseEntity
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required string FileName { get; set; }
    public required string StoragePath { get; set; }
    public required string ContentType { get; set; }
    public long FileSizeBytes { get; set; }
    public required string UploadedBy { get; set; } // User email
    public AccessType AccessType { get; set; } = AccessType.Private;

    // Navigation properties
    public ICollection<DocumentTag> DocumentTags { get; set; } = [];
    public ICollection<DocumentShare> DocumentShares { get; set; } = [];
    public ICollection<AuditLog> AuditLogs { get; set; } = [];
}