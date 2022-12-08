using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;

namespace ZeroDocSigner.Common.Algorithm
{
    [Serializable]
    public readonly struct Signature
    {
        public SignatureParameters Parameters { get; init; }

        public byte[] Sequence { get; init; }

        public static Signature Create(
            byte[] data,
            X509Certificate2 certificate,
            SignatureParameters parameters)
        {
            var hash = Hashing.Compute(data, parameters.HashAlgorithmName);
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
