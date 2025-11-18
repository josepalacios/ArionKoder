using AutoMapper;
using DocumentManagement.Application.Mappings;
using DocumentManagement.Application.Services.Implementations;
using DocumentManagement.Application.Services.Interfaces;
using DocumentManagement.Application.Validators;
using FluentValidation;

namespace DocumentManagement.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Register validators
           services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

            // Register services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<IDocumentShareService, DocumentShareService>();
            services.AddScoped<IAuditService, AuditService>();

            // AutoMapper
            //services.AddAutoMapper(typeof(ServiceCollectionExtensions).Assembly);
            services.AddAutoMapper(typeof(MappingProfile).Assembly);

            return services;
        }
    }
}
