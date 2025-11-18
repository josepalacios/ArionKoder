using DocumentManagement.Application.DTOs.Common;
using DocumentManagement.Application.DTOs.Requests;
using DocumentManagement.Application.DTOs.Responses;
using DocumentManagement.Application.Services.Interfaces;
using DocumentManagement.Domain.Enums;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace DocumentManagement.Application.Services.Implementations
{
    public sealed class AuthService(IConfiguration configuration) : IAuthService
    {
        private static readonly Dictionary<string, (string Password, string Name, UserRole Role)> MockUsers = new()
        {
            ["admin@company.com"] = ("Admin@123", "Admin User", UserRole.Admin),
            ["manager@company.com"] = ("Manager@123", "Manager User", UserRole.Manager),
            ["contributor@company.com"] = ("Contributor@123", "Contributor User", UserRole.Contributor),
            ["viewer@company.com"] = ("Viewer@123", "Viewer User", UserRole.Viewer)
        };

        public Task<Result<AuthResponse>> LoginAsync(
            LoginRequest request,
            CancellationToken cancellationToken = default)
        {
            if (!MockUsers.TryGetValue(request.Email, out var user))
            {
                return Task.FromResult(
                    Result<AuthResponse>.Failure("Invalid email or password.", 401));
            }

            if (user.Password != request.Password)
            {
                return Task.FromResult(
                    Result<AuthResponse>.Failure("Invalid email or password.", 401));
            }

            var token = GenerateJwtToken(request.Email, user.Name, user.Role);
            var response = new AuthResponse(
                token,
                new UserResponse(request.Email, user.Name, user.Role));

            return Task.FromResult(Result<AuthResponse>.Success(response));
        }

        public Result<UserResponse> GetCurrentUser(string token)
        {
            try
            {
                var principal = ValidateTokenAndGetPrincipal(token);
                if (principal == null)
                {
                    return Result<UserResponse>.Failure("Invalid token.", 401);
                }

                var email = principal.FindFirst(ClaimTypes.Email)?.Value;
                var name = principal.FindFirst(ClaimTypes.Name)?.Value;
                var roleStr = principal.FindFirst(ClaimTypes.Role)?.Value;

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name) ||
                    string.IsNullOrEmpty(roleStr) || !Enum.TryParse<UserRole>(roleStr, out var role))
                {
                    return Result<UserResponse>.Failure("Invalid token payload.", 401);
                }

                return Result<UserResponse>.Success(new UserResponse(email, name, role));
            }
            catch
            {
                return Result<UserResponse>.Failure("Invalid token.", 401);
            }
        }

        public bool ValidateToken(string token)
        {
            return ValidateTokenAndGetPrincipal(token) != null;
        }

        private string GenerateJwtToken(string email, string name, UserRole role)
        {
            var jwtSettings = configuration.GetSection("Jwt");
            var secretKey = jwtSettings["SecretKey"] ?? "your-secret-key-min-32-chars-long-for-hs256";
            var issuer = jwtSettings["Issuer"] ?? "DocumentManagementApi";
            var audience = jwtSettings["Audience"] ?? "DocumentManagementClient";
            var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, email),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Role, role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private ClaimsPrincipal? ValidateTokenAndGetPrincipal(string token)
        {
            try
            {
                var jwtSettings = configuration.GetSection("Jwt");
                var secretKey = jwtSettings["SecretKey"] ?? "your-secret-key-min-32-chars-long-for-hs256";
                var issuer = jwtSettings["Issuer"] ?? "DocumentManagementApi";
                var audience = jwtSettings["Audience"] ?? "DocumentManagementClient";

                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                return tokenHandler.ValidateToken(token, validationParameters, out _);
            }
            catch
            {
                return null;
            }
        }
    }
}
