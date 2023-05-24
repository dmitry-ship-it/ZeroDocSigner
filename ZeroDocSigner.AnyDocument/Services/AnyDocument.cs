using System.IO.Compression;
using ZeroDocSigner.AnyDocument.Interfaces;
using ZeroDocSigner.AnyDocument.Models;
using ZeroDocSigner.Shared.Extensions;
using ZeroDocSigner.Shared.Interfaces;

namespace ZeroDocSigner.AnyDocument.Services;

public class AnyDocument : IDocument<AnySignatureInfo>, IDocumentSignatureVerifier
{
    private bool disposed;

    private readonly MemoryStream documentStream;
    private readonly DocumentParser documentParser;
    private readonly DocumentBuilder documentBuilder;

    public AnyDocument(byte[] document)
    {
        documentStream = new MemoryStream(document);
        var documentType = GetDocumentType(document);

        documentParser = new DocumentParser(documentStream, documentType);
        documentBuilder = new DocumentBuilder(
            documentType,
            documentParser.DocumentContent ?? documentParser.DataToSign,
            documentParser.Signatures);
    }

    public bool ContainsSignatures => documentBuilder.Signatures.Count > 0;

    public void Sign(Stream certificate, string? certificatePassword, AnySignatureInfo signatureInfo)
    {
        using var certificateInstance = certificate.ReadAsX509Certificate2(certificatePassword);

        AnyDocumentSigner.Sign(documentBuilder, certificateInstance, signatureInfo, documentParser.DataToSign);
    }

    public AnyDocumentVerificationInfo[] Verify()
    {
        return AnyDocumentSigner.Verify(documentBuilder, documentParser.DataToSign);
    }

    public byte[] GetData()
    {
        return documentBuilder.Build();
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

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                documentStream?.Dispose();
            }

            disposed = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
