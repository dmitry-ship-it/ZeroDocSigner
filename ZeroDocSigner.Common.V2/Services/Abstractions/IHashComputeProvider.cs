using System.Security.Cryptography;

namespace ZeroDocSigner.Common.V2.Services.Abstractions;

public interface IHashComputeProvider
{
    byte[] Compute(byte[] data);

    byte[] Compute(Stream stream);

    HashAlgorithm HashAlgorithm { get; }
}
