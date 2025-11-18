using DocumentManagement.Api.Extensions;
using DocumentManagement.Application.Common;
using DocumentManagement.Application.DTOs.Responses;
using DocumentManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocumentManagement.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize(Roles = "Admin,Manager")]
    [Produces("application/json")]
    public class AuditLogsController(IAuditService auditService) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<AuditLogResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAuditLogs(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? userEmail = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var result = await auditService.GetAuditLogsAsync(
                pageNumber,
                pageSize,
                userEmail,
                fromDate,
                toDate);

            return result.IsSuccess
                ? Ok(result.Data)
                : StatusCode(result.StatusCode ?? 400, new { error = result.Error });
        }

        [HttpGet("documents/{documentId}")]
        [ProducesResponseType(typeof(IEnumerable<AuditLogResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetDocumentAuditTrail(int documentId)
        {
            var userEmail = User.GetUserEmail();
            var userRole = User.GetUserRole();

            var result = await auditService.GetDocumentAuditTrailAsync(documentId, userEmail, userRole);

            return result.IsSuccess
                ? Ok(result.Data)
                : StatusCode(result.StatusCode ?? 400, new { error = result.Error });
        }
    }
}
