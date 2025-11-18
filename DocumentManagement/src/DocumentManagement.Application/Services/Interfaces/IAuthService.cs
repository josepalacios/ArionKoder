using DocumentManagement.Application.DTOs.Common;
using DocumentManagement.Application.DTOs.Requests;
using DocumentManagement.Application.DTOs.Responses;

namespace DocumentManagement.Application.Services.Interfaces
{
    public interface IAuthService
    {
        Task<Result<AuthResponse>> LoginAsync(LoginRequest request,CancellationToken cancellationToken = default);
        Result<UserResponse> GetCurrentUser(string token);
        bool ValidateToken(string token);
    }
}
