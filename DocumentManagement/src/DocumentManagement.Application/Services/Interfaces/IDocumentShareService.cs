using DocumentManagement.Application.DTOs.Common;
using DocumentManagement.Application.DTOs.Requests;
using DocumentManagement.Application.DTOs.Responses;
using DocumentManagement.Domain.Enums;

namespace DocumentManagement.Application.Services.Interfaces
{
    public interface IDocumentShareService
    {
        Task<Result<DocumentShareResponse>> ShareDocumentAsync(
            int documentId,
            ShareDocumentRequest request,
            string userEmail,
            UserRole userRole,
            CancellationToken cancellationToken = default);

        Task<Result<IEnumerable<DocumentShareResponse>>> GetDocumentSharesAsync(
            int documentId,
            string userEmail,
            UserRole userRole,
            CancellationToken cancellationToken = default);

        Task<Result> RevokeShareAsync(
            int documentId,
            string sharedWithEmail,
            string userEmail,
            UserRole userRole,
            CancellationToken cancellationToken = default);
    }
}
