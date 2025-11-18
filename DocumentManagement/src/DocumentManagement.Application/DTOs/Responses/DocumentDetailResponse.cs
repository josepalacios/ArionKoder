using DocumentManagement.Domain.Enums;

namespace DocumentManagement.Application.DTOs.Responses
{
    public record DocumentDetailResponse(
    int Id,
    string Title,
    string? Description,
    string FileName,
    string ContentType,
    long FileSizeBytes,
    string UploadedBy,
    AccessType AccessType,
    IEnumerable<string> Tags,
    IEnumerable<DocumentShareResponse> Shares,
    DateTime CreatedAt,
    DateTime? UpdatedAt);


}
