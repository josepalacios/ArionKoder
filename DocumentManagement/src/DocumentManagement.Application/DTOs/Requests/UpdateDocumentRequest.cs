using DocumentManagement.Domain.Enums;
namespace DocumentManagement.Application.DTOs.Requests
{
    public record UpdateDocumentRequest(string? Title, string? Description, IEnumerable<string>? Tags, AccessType? AccessType);
}
