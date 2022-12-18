using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;

namespace ZeroDocSigner.Common.Algorithm
{
    [Serializable]
    public readonly struct Signature
    {
        public byte[] Sequence { get; init; }

        public static Signature Create(
            byte[] data,
            X509Certificate2 certificate)
        {
            var algorithm = SignatureAlgorithm.Create(certificate);
            var hash = Hashing.Compute(data, algorithm.HashAlgorithm);

            return new()
            {
                Sequence = algorithm.CreateSignature(hash)
            };
        }

        [JsonIgnore]
        public string SignatureBase64 => Convert.ToBase64String(Sequence);
    }
}
