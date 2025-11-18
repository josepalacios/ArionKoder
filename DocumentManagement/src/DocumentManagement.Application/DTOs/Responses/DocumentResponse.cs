using DocumentManagement.Domain.Enums;

namespace DocumentManagement.Application.DTOs.Responses
{
    public record DocumentResponse(
    int Id,
    string Title,
    string? Description,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    string UploadedBy,
    AccessType AccessType,
    IEnumerable<string> Tags,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

}
