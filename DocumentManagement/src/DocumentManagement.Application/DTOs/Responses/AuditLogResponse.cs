namespace DocumentManagement.Application.DTOs.Responses
{
    public record AuditLogResponse(
    int Id,
    string UserEmail,
    string Action,
    string EntityType,
    int? EntityId,
    string? IpAddress,
    string? Details,
    DateTime CreatedAt);
}
