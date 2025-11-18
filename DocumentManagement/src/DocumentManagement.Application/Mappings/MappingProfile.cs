using AutoMapper;
using DocumentManagement.Application.DTOs.Requests;
using DocumentManagement.Application.DTOs.Responses;

namespace DocumentManagement.Application.Mappings
{
    public sealed class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateDocumentMaps();
            CreateDocumentShareMaps();
            CreateAuditLogMaps();
            CreateUploadDocumentMaps();
        }

        private void CreateDocumentMaps()
        {
            CreateMap<Document, DocumentResponse>()
                .ForMember(dest => dest.Tags,
                    opt => opt.MapFrom(src => src.DocumentTags.Select(dt => dt.Tag.Name)));

            CreateMap<Document, DocumentDetailResponse>()
                .ForMember(dest => dest.Tags,
                    opt => opt.MapFrom(src => src.DocumentTags.Select(dt => dt.Tag.Name)))
                .ForMember(dest => dest.Shares,
                    opt => opt.MapFrom(src => src.DocumentShares.Where(ds => ds.RevokedAt == null)));


        }

        private void CreateDocumentShareMaps()
        {
            CreateMap<DocumentShare, DocumentShareResponse>();
        }

        private void CreateAuditLogMaps()
        {
            CreateMap<AuditLog, AuditLogResponse>();
        }

        private void CreateUploadDocumentMaps()
        {
            CreateMap<UploadDocumentRequest, Document>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.DocumentTags, opt => opt.Ignore())
                .ForMember(dest => dest.DocumentShares, opt => opt.Ignore());   
                //.ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.File.FileName))
                //.ForMember(dest => dest.ContentType, opt => opt.MapFrom(src => src.File.ContentType))
                //.ForMember(dest => dest.FileSizeBytes, opt => opt.MapFrom(src => src.File.Length));
        }
    }
}
