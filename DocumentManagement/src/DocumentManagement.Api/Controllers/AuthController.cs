using DocumentManagement.Application.DTOs.Requests;
using DocumentManagement.Application.DTOs.Responses;
using DocumentManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocumentManagement.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Produces("application/json")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await authService.LoginAsync(request);

            return result.IsSuccess
                ? Ok(result.Data)
                : StatusCode(result.StatusCode ?? 400, new { error = result.Error });
        }

        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetCurrentUser()
        {
            var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var result = authService.GetCurrentUser(token);

            return result.IsSuccess
                ? Ok(result.Data)
                : StatusCode(result.StatusCode ?? 401, new { error = result.Error });
        }

        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Logout()
        {
            // Token disposal is handled client-side
            return Ok(new { message = "Logged out successfully" });
        }
    }
}
