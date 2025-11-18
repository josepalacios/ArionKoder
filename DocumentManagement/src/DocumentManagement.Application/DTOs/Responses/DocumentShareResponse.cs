using DocumentManagement.Domain.Enums;

namespace DocumentManagement.Application.DTOs.Responses
{
    public record DocumentShareResponse(
     int Id,
     string SharedWithEmail,
     PermissionLevel PermissionLevel,
     string SharedByEmail,
     DateTime CreatedAt);
}
