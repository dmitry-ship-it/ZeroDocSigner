using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using ZeroDocSigner.Common.Algorithm;
using ZeroDocSigner.Common.Extensions;

namespace ZeroDocSigner.Common.Manager
{
    public class SignatureManager : ISigner, IVerifier, IUnsigner
    {
        private readonly byte[] _data;
        //private readonly DocumentType _documentType;
        private readonly X509Certificate2 _certificate;
        private readonly long _signaturesStart;

        public SignatureManager(
            byte[] data,
            //DocumentType documentType,
            X509Certificate2 certificate)
        {
            _data = data;
            //_documentType = documentType;
            _certificate = certificate;
            _signaturesStart = _data.FindSequenceIndex(SignatureInfo.StartSequence);
        }

        public byte[] AddSignature(SignatureParameters parameters)
        {
            var signatures = GetSignatures();
            if (signatures is null)
            {
                return RebuildFile(SignatureInfo.GetNewSignatureInfo(
                    _data, _certificate, parameters));
            }

            var newSignature = Signature.Create(
                _data, _certificate, parameters);

            signatures.Signatures = signatures.Signatures.Add(newSignature);

            return RebuildFile(signatures);
        }

        public byte[] CreateSignature(
            SignatureParameters parameters,
            bool force = false)
        {
            if (!force && _signaturesStart != -1)
            {
                throw new InvalidOperationException(
                    "This data already contains signature(s).");
            }

            var fileData = GetContent();
            var signatures = SignatureInfo.GetNewSignatureInfo(
                fileData, _certificate, parameters);

            return RebuildFile(signatures);
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

            var hash = Hashing.Compute(
                GetContent(),
                signature.Parameters.HashAlgorithmName);

            return verifier.VerifySignature(hash, signature);
        }

        private SignatureInfo? GetSignatures()
        {
            if (_signaturesStart == -1)
            {
                return null;
            }

            return JsonSerializer.Deserialize<SignatureInfo>(
                Encoding.Default.GetString(
                        _data.TakeFrom(_signaturesStart)));
        }

        private byte[] GetContent()
        {
            if (_signaturesStart == -1)
            {
                return _data;
            }

            var newLineBytes = Encoding.Default.GetByteCount(Environment.NewLine);

            return _data.Take(_signaturesStart - SignatureInfo.StartSequence.Length - newLineBytes);
        }

        private byte[] RebuildFile(SignatureInfo signatureInfo)
        {
            var content = GetContent();
            var signatures = Encoding.Default.GetBytes(signatureInfo.ToString());

            return content.Concat(signatures);
        }

        public byte[] RemoveSignature(Signature signature)
        {
            var signInfo = GetSignatures()
                ?? throw new InvalidOperationException("No signatures to remove.");

            signInfo.Signatures = signInfo.Signatures.RemoveOne(
                el => el.Sequence.SequenceEqual(signature.Sequence));

            return RebuildFile(signInfo);
        }

        public byte[] RemoveAllSignatures()
        {
            return GetContent();
        }
    }
}
