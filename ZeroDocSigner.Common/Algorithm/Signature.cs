using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;

namespace ZeroDocSigner.Common.Algorithm
{
    [Serializable]
    public readonly struct Signature
    {
        public SignatureParameters Parameters { get; init; }

        public byte[] Sequence { get; init; }

        public static Signature Create(byte[] data, X509Certificate2 certificate, SignatureParameters parameters)
        {
            using var hasher = HashAlgorithm.Create(parameters.HashAlgorithmName.Name);

            if (hasher is null)
            {
                throw new ArgumentException("Invalid hash algorithm name.", nameof(parameters));
            }

            var hash = hasher.ComputeHash(data);

            var algorithm = SignatureAlgorithm.Create(parameters, certificate);

            return new Signature()
            {
                Parameters = parameters,
                Sequence = algorithm.CreateSignature(hash)
            };
        }

        [JsonIgnore]
        public string SignatureBase64 => Convert.ToBase64String(Sequence);
    }
}
