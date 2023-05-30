using System.Security.Cryptography.X509Certificates;
using ZeroDocSigner.Shared.Interfaces;

namespace ZeroDocSigner.Api.Services;

public class UserCertificateHolder : ICertificateHolder
{
    private bool disposed;

    public UserCertificateHolder(X509Certificate2 certificate)
    {
        CertificateStream = new MemoryStream();
        CertificateStream.Write(certificate.Export(X509ContentType.Pfx, Password));
        CertificateStream.Seek(0, SeekOrigin.Begin);
    }

    public Stream CertificateStream { get; }

    public string Password { get; } = Guid.NewGuid().ToString().Replace("-", string.Empty);

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                CertificateStream?.Dispose();
            }

            disposed = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
