using DocumentManagement.Domain.Enums;

public sealed class DocumentShare : BaseEntity
{
    public int DocumentId { get; set; }
    public Document Document { get; set; } = null!;

    public required string SharedWithEmail { get; set; }
    public PermissionLevel PermissionLevel { get; set; }
    public required string SharedByEmail { get; set; }
    public DateTime? RevokedAt { get; set; }

    public bool IsActive => RevokedAt == null;
}