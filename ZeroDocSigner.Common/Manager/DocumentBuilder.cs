using System;
using System.IO.Compression;
using System.Runtime.InteropServices;
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

        public byte[] Build()
        {
            return _documentType switch
            {
                DocumentType.Binary => BuildBinary(),
                DocumentType.Archive => BuildArchive(),
                _ => throw new InvalidOperationException("Unknown document type.")
            };
        }

        private byte[] BuildBinary()
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

        private byte[] BuildArchive()
        {
            if (_signatureInfo is null)
            {
                return _originalData;
            }

            using var memory = new MemoryStream();
            memory.Write(_originalData);
            using (var archive = new ZipArchive(memory, ZipArchiveMode.Update))
            {
                var signFile = archive.GetEntry(SignatureInfo.SignaturesFileName)
                    ?? archive.CreateEntry(SignatureInfo.SignaturesFileName);

                using var writer = new StreamWriter(signFile.Open());

                writer.Write(_signatureInfo.Serialize());
                writer.Flush();
            }
            //memory.Seek(0, SeekOrigin.Begin);
            return memory.ToArray();
        }
    }
}
