using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ZeroDocSigner.AnyDocument;
using ZeroDocSigner.AnyDocument.Interfaces;
using ZeroDocSigner.Api.Authentication.Services;
using ZeroDocSigner.Api.Services;
using ZeroDocSigner.OfficeDocument;
using ZeroDocSigner.OfficeDocument.Models;
using ZeroDocSigner.OpenDocument;
using ZeroDocSigner.OpenDocument.Models;
using ZeroDocSigner.PdfDocument;
using ZeroDocSigner.PdfDocument.Models;
using ZeroDocSigner.Shared.Interfaces;

namespace ZeroDocSigner.Api.Extensions;

public static class ServiceCollectionExtension
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S112:General exceptions should never be thrown", Justification = "<Pending>")]
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new SystemException("Current OS is not supported. Use Windows to host this server.");
        }

        services.AddHttpContextAccessor();

        services.AddScoped<IUserCacheService, UserCacheService>();
        services.AddScoped<IAuthenticationService, ActiveDirectoryAuthenticationService>();
        services.AddScoped<IUserCertificateService, ActiveDirectoryUserCertificateService>();

        services.AddScoped<ICertificateHolder, UserCertificateHolder>((provider) =>
        {
            var userCertificateService = provider.GetRequiredService<IUserCertificateService>();
            var httpContext = provider.GetRequiredService<IHttpContextAccessor>();

            return new UserCertificateHolder(userCertificateService.GetCertificateAsync(
                httpContext.HttpContext?.User.Identity?.Name!).Result);
        });

        // standardized signature implementation
        services.AddScoped<IDocumentSignatureService<OfficeSignatureInfo>, OfficeDocumentService>();
        services.AddScoped<IDocumentSignatureService<OpenSignatureInfo>, OpenDocumentService>();
        services.AddScoped<IDocumentSignatureService<PdfSignatureInfo>, PdfDocumentService>();
        services.AddScoped<IAnyDocumentSignatureService, AnyDocumentService>();

        return services;
    }

    public static IServiceCollection AddAllowingEverythingCors(this IServiceCollection services)
    {
        return services.AddCors(options => options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyHeader();
            policy.AllowAnyMethod();
            policy.AllowCredentials();
            policy.SetIsOriginAllowed(_ => true);
        }));
    }

    public static IServiceCollection AddJwtBearerAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var section = configuration.GetSection("JwtBearer");
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(section["Key"]!));

                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = section["Issuer"],
                    ValidateAudience = true,
                    ValidAudience = section["Audience"],
                    ValidateLifetime = true,
                    IssuerSigningKey = key,
                    ValidateIssuerSigningKey = true,
                };
            });

        return services;
    }
}
