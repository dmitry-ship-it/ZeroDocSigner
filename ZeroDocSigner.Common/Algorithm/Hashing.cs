using System.Security.Cryptography;

#pragma warning disable SYSLIB0045 // Type or member is obsolete

namespace ZeroDocSigner.Common.Algorithm
{
    public static class Hashing
    {
        public static byte[] Compute(byte[] input, HashAlgorithmName hashAlgorithm)
        {
            using var hasher = HashAlgorithm.Create(hashAlgorithm.Name!);
            if (hasher is null)
            {
                throw new ArgumentException(
                    "Invalid hash algorithm name.", nameof(hashAlgorithm));
            }

            return hasher.ComputeHash(input);
        }
    }
}

#pragma warning restore SYSLIB0045 // Type or member is obsolete
