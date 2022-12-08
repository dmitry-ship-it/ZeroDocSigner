using System.IO.Compression;
using System.Text;
using ZeroDocSigner.Common.Algorithm;
using ZeroDocSigner.Common.Extensions;

namespace ZeroDocSigner.Common.Manager
{
    public class DocumentBuilder
    {
        private readonly byte[] _originalData;
        private readonly DocumentType _documentType;
        private byte[]? _modifiedData;
        private SignatureInfo? _signatureInfo;

        public DocumentBuilder(byte[] orignalData, DocumentType documentType)
        {
            _originalData = orignalData;
            _documentType = documentType;
        }

        public void SetContent(byte[] content)
        {
            _modifiedData = content;
        }

        public void SetSignatureInfo(SignatureInfo signatureInfo)
        {
            _signatureInfo = signatureInfo;
        }

        public byte[] BuildDocument()
        {
            return _documentType switch
            {
                DocumentType.Binary => BuildBinaryDocument(),
                DocumentType.Archive => BuildArchiveDocument(),
                _ => throw new InvalidOperationException("Unknown document type.")
            };
        }

        private byte[] BuildBinaryDocument()
        {
            if (_modifiedData is null)
            {
                throw new InvalidOperationException("File content was not privided.");
            }

            if (_signatureInfo is null)
            {
                return _modifiedData;
            }

            var signatureBytes = Encoding.Default.GetBytes(_signatureInfo.ToString());

            return _modifiedData.StickWith(signatureBytes);
        }

        private byte[] BuildArchiveDocument()
        {
            if (_signatureInfo is null)
            {
                return _originalData;
            }

            using var memory = new MemoryStream(_originalData);
            using var archive = new ZipArchive(memory);

            var signFile = archive.Entries.FirstOrDefault(
                    entry => entry.Name == SignatureInfo.SignaturesFileName) 
                ?? archive.CreateEntry(SignatureInfo.SignaturesFileName);

            using var file = signFile.Open();
            using var writer = new StreamWriter(file);

            writer.Write(_signatureInfo.ToString());
            writer.Flush();

            memory.Position = 0;
            return memory.ToArray();
        }
    }
}
