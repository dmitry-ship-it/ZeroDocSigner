using ZeroDocSigner.AnyDocument;
using ZeroDocSigner.AnyDocument.Interfaces;
using ZeroDocSigner.OfficeDocument;
using ZeroDocSigner.OfficeDocument.Models;
using ZeroDocSigner.OpenDocument;
using ZeroDocSigner.OpenDocument.Models;
using ZeroDocSigner.PdfDocument;
using ZeroDocSigner.PdfDocument.Models;
using ZeroDocSigner.Shared.Interfaces;
using ZeroDocSigner.Shared.Services;

namespace ZeroDocSigner.Api.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ICertificateHolder, CertificateHolder>((provider) =>
        {
            var (path, password) = CertificatesLoader.CertificateProvider(provider);

            return new CertificateHolder(path, password);
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
}
