using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ZeroDocSigner.Common.Algorithm
{
    [Serializable]
    public struct SignatureParameters
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SignatureAlgorithmName SignatureAlgorithmName { get; init; }

        [JsonConverter(typeof(HashAlgorithmNameConverter))]
        public HashAlgorithmName HashAlgorithmName { get; init; }

        [JsonIgnore]
        public static SignatureParameters Default => new()
        {
            HashAlgorithmName = HashAlgorithmName.SHA256,
            SignatureAlgorithmName = SignatureAlgorithmName.RSA
        };
    }
}
