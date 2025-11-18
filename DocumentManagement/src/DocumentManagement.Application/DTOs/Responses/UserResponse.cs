using DocumentManagement.Domain.Enums;

namespace DocumentManagement.Application.DTOs.Responses
{
    public record UserResponse(string Email, string Name, UserRole Role);
}
