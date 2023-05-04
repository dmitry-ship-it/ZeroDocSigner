using System.Security.Cryptography;
using ZeroDocSigner.Common.V2.Services.Abstractions;

namespace ZeroDocSigner.Common.V2.Services.Implementation;

public class BclSha256ComputeProvider : IHashComputeProvider, IDisposable
{
    private bool disposed;

    public HashAlgorithm HashAlgorithm { get; } = SHA256.Create();

    public byte[] Compute(byte[] data) => SHA256.HashData(data);

    public byte[] Compute(Stream stream) => SHA256.HashData(stream);

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
