using ZeroDocSigner.Shared.Interfaces;

namespace ZeroDocSigner.Shared.Services;

public class CertificateHolder : ICertificateHolder
{
    private bool disposed;

    public CertificateHolder(string path, string password)
    {
        CertificateStream = new MemoryStream(File.ReadAllBytes(path));
        Password = password;
    }

    public Stream CertificateStream { get; }
    public string Password { get; }

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
