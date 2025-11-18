using DocumentManagement.Api.Extensions;
using DocumentManagement.Application.Common;
using DocumentManagement.Application.DTOs.Requests;
using DocumentManagement.Application.DTOs.Responses;
using DocumentManagement.Application.Services.Interfaces;
using DocumentManagement.Application.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DocumentManagement.Application.Mappings;

namespace DocumentManagement.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class DocumentsController(
    IDocumentService documentService,
    IAuditService auditService) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(typeof(DocumentResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UploadDocument([FromForm] UploadDocumentFormRequest request)
        {
            var userEmail = User.GetUserEmail();

            var metadataValidator = new UploadDocumentRequestValidator();
            var metadata = request.ToRequest();
            var metadataResult = metadataValidator.Validate(metadata);

            if (!metadataResult.IsValid)
                return BadRequest(metadataResult.Errors);

            var fileValidator = new UploadFileValidator();
            var fileResult = fileValidator.Validate(request.File);

            if (!fileResult.IsValid)
                return BadRequest(fileResult.Errors);

            await using var stream = request.File.OpenReadStream();

            var result = await documentService.UploadDocumentAsync(
                metadata,
                stream,
                request.File.FileName,
                request.File.ContentType,
                request.File.Length,
                userEmail);

            if (!result.IsSuccess)
                return StatusCode(result.StatusCode ?? 400, new { error = result.Error });

            await auditService.LogAsync(
                userEmail,
                "Document Uploaded",
                "Document",
                result.Data!.Id,
                HttpContext.GetIpAddress());

            return CreatedAtAction(
                nameof(GetDocument),
                new { id = result.Data!.Id },
                result.Data);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<DocumentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetDocuments([FromQuery] SearchDocumentsRequest request)
        {
            var userEmail = User.GetUserEmail();
            var userRole = User.GetUserRole();

            var result = await documentService.GetDocumentsAsync(request, userEmail, userRole);

            return result.IsSuccess
                ? Ok(result.Data)
                : StatusCode(result.StatusCode ?? 400, new { error = result.Error });
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DocumentDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetDocument(int id)
        {
            var userEmail = User.GetUserEmail();
            var userRole = User.GetUserRole();

            var result = await documentService.GetDocumentByIdAsync(id, userEmail, userRole);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode ?? 400, new { error = result.Error });
            }

            await auditService.LogAsync(
                userEmail,
                "Document Viewed",
                "Document",
                id,
                HttpContext.GetIpAddress());

            return Ok(result.Data);
        }

        [HttpGet("{id}/download")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DownloadDocument(int id)
        {
            var userEmail = User.GetUserEmail();
            var userRole = User.GetUserRole();

            var documentResult = await documentService.GetDocumentByIdAsync(id, userEmail, userRole);

            if (!documentResult.IsSuccess)
            {
                return StatusCode(documentResult.StatusCode ?? 400, new { error = documentResult.Error });
            }

            var streamResult = await documentService.DownloadDocumentAsync(id, userEmail, userRole);

            if (!streamResult.IsSuccess)
            {
                return StatusCode(streamResult.StatusCode ?? 400, new { error = streamResult.Error });
            }

            await auditService.LogAsync(
                userEmail,
                "Document Downloaded",
                "Document",
                id,
                HttpContext.GetIpAddress());

            var document = documentResult.Data!;
            return File(streamResult.Data!, document.ContentType, document.FileName);
        }

        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(DocumentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateDocument(
            int id,
            [FromBody] UpdateDocumentRequest request)
        {
            var userEmail = User.GetUserEmail();
            var userRole = User.GetUserRole();

            var result = await documentService.UpdateDocumentAsync(id, request, userEmail, userRole);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode ?? 400, new { error = result.Error });
            }

            await auditService.LogAsync(
                userEmail,
                "Document Updated",
                "Document",
                id,
                HttpContext.GetIpAddress(),
                $"Updated: {string.Join(", ", GetUpdatedFields(request))}");

            return Ok(result.Data);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var userEmail = User.GetUserEmail();
            var userRole = User.GetUserRole();

            var result = await documentService.DeleteDocumentAsync(id, userEmail, userRole);

            if (!result.IsSuccess)
            {
                return StatusCode(result.StatusCode ?? 400, new { error = result.Error });
            }

            await auditService.LogAsync(
                userEmail,
                "Document Deleted",
                "Document",
                id,
                HttpContext.GetIpAddress());

            return NoContent();
        }

        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<DocumentResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchDocuments([FromQuery] string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest(new { error = "Search term is required" });
            }

            var userEmail = User.GetUserEmail();
            var userRole = User.GetUserRole();

            var result = await documentService.SearchDocumentsAsync(searchTerm, userEmail, userRole);

            return result.IsSuccess
                ? Ok(result.Data)
                : StatusCode(result.StatusCode ?? 400, new { error = result.Error });
        }

        private static List<string> GetUpdatedFields(UpdateDocumentRequest request)
        {
            var fields = new List<string>();
            if (request.Title != null) fields.Add("Title");
            if (request.Description != null) fields.Add("Description");
            if (request.Tags != null) fields.Add("Tags");
            if (request.AccessType.HasValue) fields.Add("AccessType");
            return fields;
        }
    }
}
