using System.Security.Cryptography.X509Certificates;

namespace ZeroDocSigner.Api
{
    public static class CertificatesLoader
    {
        public static Func<IServiceProvider, X509Certificate2> CertificateProvider => GetCertificate;

        private static X509Certificate2 GetCertificate(IServiceProvider serviceProvider)
        {
            var cfg = serviceProvider.GetRequiredService<IConfiguration>();
            var section = cfg.GetSection("cert");

            var path = Path.Combine(Directory.GetCurrentDirectory(), section["file"]!);

            return new X509Certificate2(path, section["password"],
                X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
        }
    }
}
