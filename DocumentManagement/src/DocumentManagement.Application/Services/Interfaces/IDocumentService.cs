using DocumentManagement.Application.Common;
using DocumentManagement.Application.DTOs.Common;
using DocumentManagement.Application.DTOs.Requests;
using DocumentManagement.Application.DTOs.Responses;
using DocumentManagement.Domain.Enums;

namespace DocumentManagement.Application.Services.Interfaces
{
    public interface IDocumentService
    {
        Task<Result<DocumentResponse>> UploadDocumentAsync(
            UploadDocumentRequest request,
            Stream fileStream,
            string fileName,
            string contentType,
            long fileSizeBytes,
            string userEmail,
            CancellationToken cancellationToken = default);

        Task<Result<PagedResult<DocumentResponse>>> GetDocumentsAsync(
            SearchDocumentsRequest request,
            string userEmail,
            UserRole userRole,
            CancellationToken cancellationToken = default);

        Task<Result<DocumentDetailResponse>> GetDocumentByIdAsync(
            int id,
            string userEmail,
            UserRole userRole,
            CancellationToken cancellationToken = default);

        Task<Result<Stream>> DownloadDocumentAsync(
            int id,
            string userEmail,
            UserRole userRole,
            CancellationToken cancellationToken = default);

        Task<Result<DocumentResponse>> UpdateDocumentAsync(
            int id,
            UpdateDocumentRequest request,
            string userEmail,
            UserRole userRole,
            CancellationToken cancellationToken = default);

        Task<Result> DeleteDocumentAsync(
            int id,
            string userEmail,
            UserRole userRole,
            CancellationToken cancellationToken = default);

        Task<Result<IEnumerable<DocumentResponse>>> SearchDocumentsAsync(
            string searchTerm,
            string userEmail,
            UserRole userRole,
            CancellationToken cancellationToken = default);
    }
}
