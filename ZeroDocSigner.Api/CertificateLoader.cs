using System;
using System.Security.Cryptography.X509Certificates;

namespace ZeroDocSigner.Api
{
    public static class CertificatesLoader
    {
        public static Func<IServiceProvider, X509Certificate2> CertificateProvider => GetCertificate;

        private static X509Certificate2 GetCertificate(IServiceProvider _)
        {
            using var storage = new X509Store();
            storage.Open(OpenFlags.ReadOnly);
            var cert = storage.Certificates.First();
            storage.Close();
            return cert;
        }
    }
}
