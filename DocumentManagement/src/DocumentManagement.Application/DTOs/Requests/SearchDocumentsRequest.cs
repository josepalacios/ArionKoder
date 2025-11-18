namespace DocumentManagement.Application.DTOs.Requests
{
    public record SearchDocumentsRequest(string? SearchTerm, int PageNumber = 1, int PageSize = 20);
}
