using System.Security.Cryptography.X509Certificates;
using ZeroDocSigner.Common.Algorithm;
using ZeroDocSigner.Common.Extensions;

namespace ZeroDocSigner.Common.Manager
{
    public class SignatureManager : ISigner, IVerifier, IUnsigner
    {
        private readonly DocumentParser _documentParser;
        private readonly X509Certificate2 _certificate;

        public SignatureManager(
            byte[] data,
            DocumentType documentType,
            X509Certificate2 certificate)
        {
            _documentParser = new DocumentParser(data, documentType);
            _certificate = certificate;
        }

        public SignatureInfo AddSignature(SignatureParameters parameters)
        {
            var signatures = _documentParser.SignatureInfo;
            if (signatures is null)
            {
                return SignatureInfo.GetNewSignatureInfo(
                    _documentParser.FileContent, _certificate, parameters);
            }

            var newSignature = Signature.Create(
                _documentParser.FileContent, _certificate, parameters);

            signatures.Signatures = signatures.Signatures.Add(newSignature);

            return signatures;
        }

        public SignatureInfo CreateSignature(
            SignatureParameters parameters,
            bool force = false)
        {
            if (!force && _documentParser.SignatureInfo is not null)
            {
                throw new InvalidOperationException(
                    "This data already contains signature(s).");
            }

            var fileData = _documentParser.FileContent;

            // TODO: Remake with DocumentType
            return SignatureInfo.GetNewSignatureInfo(
                fileData, _certificate, parameters);
        }

        public bool Verify()
        {
            if (_documentParser.SignatureInfo is null)
            {
                throw new InvalidOperationException("Nothing to verify.");
            }

            var signatures = _documentParser.SignatureInfo!;
            return signatures.Signatures.Any(Verify);
        }

        public bool Verify(Signature signature)
        {
            var verifier = SignatureAlgorithm.Create(
                signature.Parameters,
                _certificate);

            var hash = Hashing.Compute(
                _documentParser.FileContent,
                signature.Parameters.HashAlgorithmName);

            return verifier.VerifySignature(hash, signature);
        }

        public SignatureInfo RemoveSignature(Signature signature)
        {
            var signInfo = GetSignatures()
                ?? throw new InvalidOperationException("No signatures to remove.");

            signInfo.Signatures = signInfo.Signatures.RemoveOne(
                el => el.Sequence.SequenceEqual(signature.Sequence));

            return signInfo;
        }
    }
}
