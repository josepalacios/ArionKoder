using DocumentManagement.Domain.Enums;
using Microsoft.AspNetCore.Http;
namespace DocumentManagement.Application.DTOs.Requests
{
    public sealed class UploadDocumentFormRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<string>? Tags { get; set; }
        public AccessType AccessType { get; set; }

        public IFormFile File { get; set; }
    }
}
