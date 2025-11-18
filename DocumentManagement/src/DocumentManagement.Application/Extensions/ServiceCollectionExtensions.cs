using AutoMapper;
using DocumentManagement.Application.Mappings;
using DocumentManagement.Application.Services.Implementations;
using DocumentManagement.Application.Services.Interfaces;

using DocumentManagement.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentManagement.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<IDocumentShareService, DocumentShareService>();
            services.AddScoped<IAuditService, AuditService>();

            //services.AddAutoMapper(typeof(ServiceCollectionExtensions).Assembly);
            services.AddAutoMapper(typeof(MappingProfile).Assembly);

            return services;
        }
    }
}
