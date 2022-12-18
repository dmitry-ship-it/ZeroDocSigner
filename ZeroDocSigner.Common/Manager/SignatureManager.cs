using System.IO.Compression;
using System.Security.Cryptography.X509Certificates;
using ZeroDocSigner.Common.Algorithm;
using ZeroDocSigner.Common.Extensions;

namespace ZeroDocSigner.Common.Manager
{
    public class SignatureManager : ISigner, IVerifier, IUnsigner
    {
        private readonly DocumentParser _documentParser;
        private readonly DocumentBuilder _documentBuilder;
        private readonly X509Certificate2 _certificate;

        public SignatureManager(
            byte[] data,
            X509Certificate2 certificate)
        {
            var documentType = GetDocumentType(data);

            _documentParser = new DocumentParser(data, documentType);
            _documentBuilder = new DocumentBuilder(data, documentType);
            _certificate = certificate;

            _documentBuilder.SetContent(_documentParser.FileContent);
        }

        public void AddSignature()
        {
            var signatures = _documentParser.SignatureInfo;
            if (signatures is null)
            {
                _documentBuilder.SetSignatureInfo(SignatureInfo.GetNewSignatureInfo(
                    _documentParser.FileContent, _certificate));
                return;
            }

            var newSignature = Signature.Create(
                _documentParser.FileContent, _certificate);

            if (signatures.Signatures.Any(s => s.Sequence.SequenceEqual(newSignature.Sequence)))
            {
                throw new InvalidOperationException("File is already signed");
            }

            signatures.Signatures = signatures.Signatures.Add(newSignature);

            _documentBuilder.SetSignatureInfo(signatures);
        }

        public void CreateSignature(bool force = false)
        {
            if (!force && _documentParser.SignatureInfo is not null)
            {
                throw new InvalidOperationException(
                    "This data already contains signature(s).");
            }

            var fileData = _documentParser.FileContent;

            _documentBuilder.SetSignatureInfo(SignatureInfo.GetNewSignatureInfo(
                fileData, _certificate));
        }

        public bool Verify()
        {
            if (_documentParser.SignatureInfo is null)
            {
                return false;
            }

            var signatures = _documentParser.SignatureInfo!;
            return signatures.Signatures.Any(Verify);
        }

        public bool Verify(Signature signature)
        {
            var verifier = SignatureAlgorithm.Create(_certificate);

            var hash = Hashing.Compute(
                _documentParser.FileContent,
                verifier.HashAlgorithm);

            return verifier.VerifySignature(hash, signature);
        }

        public void RemoveSignature(Signature signature)
        {
            var signInfo = _documentParser.SignatureInfo
                ?? throw new InvalidOperationException("No signatures to remove.");

            signInfo.Signatures = signInfo.Signatures.RemoveOne(
                el => el.Sequence.SequenceEqual(signature.Sequence));

            _documentBuilder.SetSignatureInfo(signInfo);
        }

        public byte[] BuildFile()
        {
            return _documentBuilder.Build();
        }

        private static DocumentType GetDocumentType(byte[] data)
        {
            try
            {
                using var memory = new MemoryStream(data);
                using var archive = new ZipArchive(memory);
            }
            catch (InvalidDataException)
            {
                return DocumentType.Binary;
            }

            return DocumentType.Archive;
        }
    }
}
