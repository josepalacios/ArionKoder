using DocumentManagement.Domain.Enums;
namespace DocumentManagement.Application.DTOs.Requests
{
    public record ShareDocumentRequest(string SharedWithEmail, PermissionLevel PermissionLevel);
}
