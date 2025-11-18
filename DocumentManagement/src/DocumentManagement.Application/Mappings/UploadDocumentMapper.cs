using DocumentManagement.Application.DTOs.Requests;

namespace DocumentManagement.Application.Mappings
{
    public static class UploadDocumentMapper
    {
        public static UploadDocumentRequest ToRequest(this UploadDocumentFormRequest form)
            => new UploadDocumentRequest(
                form.Title ?? string.Empty,
                form.Description,
                form.Tags,
                form.AccessType
            );
    }
}
