using System.Security.Cryptography;
using ZeroDocSigner.OfficeDocument.Services.Abstractions;
using ZeroDocSigner.Shared.Constants;

namespace ZeroDocSigner.OfficeDocument.Services.Implementation;

internal class BclSha256ComputeProvider : IHashComputeProvider, IDisposable
{
    private bool disposed;

    public HashAlgorithm HashAlgorithm { get; } = SHA256.Create();

    public string HashAlgorithmUri => DigestMethodsUris.Sha256;

    public byte[] Compute(byte[] data) => HashAlgorithm.ComputeHash(data);

    public byte[] Compute(Stream stream) => HashAlgorithm.ComputeHash(stream);

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                HashAlgorithm.Dispose();
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
