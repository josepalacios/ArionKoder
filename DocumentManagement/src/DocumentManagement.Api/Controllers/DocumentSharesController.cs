using DocumentManagement.Api.Extensions;
using DocumentManagement.Application.DTOs.Requests;
using DocumentManagement.Application.DTOs.Responses;
using DocumentManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocumentManagement.Api.Controllers
{
    [ApiController]
    [Route("api/v1/documents/{documentId}/shares")]
    [Authorize]
    [Produces("application/json")]
    public class DocumentSharesController(
     IDocumentShareService shareService,
     IAuditService auditService) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(typeof(DocumentShareResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ShareDocument(
            int documentId,
            [FromBody] ShareDocumentRequest request)
        {
            var userEmail = User.GetUserEmail();
            var userRole = User.GetUserRole();

            var result = await shareService.ShareDocumentAsync(documentId, request, userEmail, userRole);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode ?? 400, new { error = result.Error });
            }

            await auditService.LogAsync(
                userEmail,
                "Document Shared",
                "DocumentShare",
                result.Data!.Id,
                HttpContext.GetIpAddress(),
                $"Shared with {request.SharedWithEmail} ({request.PermissionLevel})");

            return CreatedAtAction(
                nameof(GetDocumentShares),
                new { documentId },
                result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DocumentShareResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDocumentShares(int documentId)
        {
            var userEmail = User.GetUserEmail();
            var userRole = User.GetUserRole();

            var result = await shareService.GetDocumentSharesAsync(documentId, userEmail, userRole);

            return result.IsSuccess
                ? Ok(result.Data)
                : StatusCode(result.StatusCode ?? 400, new { error = result.Error });
        }

        [HttpDelete("{sharedWithEmail}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RevokeShare(int documentId, string sharedWithEmail)
        {
            var userEmail = User.GetUserEmail();
            var userRole = User.GetUserRole();

            var result = await shareService.RevokeShareAsync(documentId, sharedWithEmail, userEmail, userRole);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode ?? 400, new { error = result.Error });
            }

            await auditService.LogAsync(
                userEmail,
                "Share Revoked",
                "DocumentShare",
                documentId,
                HttpContext.GetIpAddress(),
                $"Revoked share from {sharedWithEmail}");

            return NoContent();
        }
    }
}
