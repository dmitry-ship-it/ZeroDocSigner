using System.Security.Cryptography;

namespace ZeroDocSigner.OfficeDocument.Services.Abstractions;

public interface IHashComputeProvider
{
    byte[] Compute(byte[] data);

    byte[] Compute(Stream stream);

    HashAlgorithm HashAlgorithm { get; }

    string HashAlgorithmUri { get; }
}
