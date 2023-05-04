using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace ZeroDocSigner.Api;

public static class CertificatesLoader
{
    public static Func<IServiceProvider, X509Certificate2> CertificateProvider => GetCertificate;

    private static X509Certificate2 GetCertificate(IServiceProvider serviceProvider)
    {
        var cfg = serviceProvider.GetRequiredService<IConfiguration>();
        var section = cfg.GetSection("cert");

        var path = Path.Combine(Directory.GetCurrentDirectory(), section["file"]!);

        if (!File.Exists(path) && section.GetValue<bool>("createIfMissing"))
        {
            using var rsa = RSA.Create();
            var certificateRequest = new CertificateRequest(
                "CN=ZeroDocSigner",
                rsa,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            File.WriteAllBytes(path, certificateRequest.CreateSelfSigned(
                    DateTimeOffset.Now,
                    DateTimeOffset.Now.AddYears(1))
                .Export(X509ContentType.Pfx, section["password"]));
        }

        return new X509Certificate2(path, section["password"],
            X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
    }
}
