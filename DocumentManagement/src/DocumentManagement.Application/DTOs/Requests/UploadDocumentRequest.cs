using DocumentManagement.Domain.Enums;
namespace DocumentManagement.Application.DTOs.Requests
{
    public record UploadDocumentRequest(string Title, string? Description, IEnumerable<string>? Tags, AccessType AccessType);
}
