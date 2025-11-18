public sealed class AuditLog : BaseEntity
{
    public required string UserEmail { get; set; }
    public required string Action { get; set; }
    public required string EntityType { get; set; }
    public int? EntityId { get; set; }
    public string? IpAddress { get; set; }
    public string? Details { get; set; }

    // Optional foreign key for documents
    public int? DocumentId { get; set; }
    public Document? Document { get; set; }
}