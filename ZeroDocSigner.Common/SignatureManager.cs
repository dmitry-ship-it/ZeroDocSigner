using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using ZeroDocSigner.Common.Algorithm;

namespace ZeroDocSigner.Common
{
    public class SignatureManager : ISigner, IVerifier
    {
        private readonly byte[] _data;
        private readonly X509Certificate2 _certificate;
        private readonly long _signaturesStart;

        public SignatureManager(
            byte[] data,
            X509Certificate2 certificate)
        {
            _data = data;
            _certificate = certificate;

            var start = Encoding.Default.GetBytes(SignatureInfo.StartSequence);
            _signaturesStart = GetSignaturesPos(_data);
        }

        private static long GetSignaturesPos(byte[] data)
        {
            var start = Encoding.Default.GetBytes(SignatureInfo.StartSequence);
            for (var i = 0; i < data.Length; i++)
            {
                if (data[i] == start[0])
                {
                    var j = 0;

                    while (i != data.Length
                        && j != start.Length
                        && data[i] == start[j])
                    {
                        i++;
                        j++;
                    }

                    if (j == start.Length)
                    {
                        return i + 2;
                    }
                }
            }

            return -1;
        }

        public byte[] AddSignature()
        {
            throw new NotImplementedException();
        }

        public byte[] CreateSignature(
            SignatureParameters parameters,
            bool force = false)
        {
            if (!force && _signaturesStart != -1)
            {
                throw new ArgumentException("This file already contains signature(s).");
            }

            var fileData = GetContent();
            var signatures = SignatureInfo.GetNewSignatureInfo(fileData, _certificate, parameters);

            return fileData.Concat(
                Encoding.Default.GetBytes(signatures.ToString()))
                    .ToArray();
        }

        public bool Verify()
        {
            if (_signaturesStart == -1)
            {
                throw new InvalidOperationException("Nothing to verify.");
            }

            var signatures = GetSignatures()!;
            return signatures.Signatures.Any(Verify);
        }

        public bool Verify(Signature signature)
        {
            var verifier = SignatureAlgorithm.Create(
                signature.Parameters,
                _certificate);

            var data = GetContent();
            using var hasher = HashAlgorithm.Create(signature.Parameters.HashAlgorithmName.Name!)!;

            return verifier.VerifySignature(hasher.ComputeHash(data), signature);
        }

        private SignatureInfo? GetSignatures()
        {
            if (_signaturesStart == -1)
            {
                return null;
            }

            var signBytes = new byte[_data.Length - _signaturesStart];
            Array.Copy(_data, _signaturesStart, signBytes, 0, signBytes.Length);

            return JsonSerializer.Deserialize<SignatureInfo>(
                Encoding.Default.GetString(signBytes));
        }

        private byte[] GetContent()
        {
            if (_signaturesStart == -1)
            {
                return _data;
            }

            var data = new byte[_signaturesStart - SignatureInfo.StartSequence.Length - 2];
            Array.Copy(_data, data, data.Length);

            return data;
        }
    }
}
