using System.IO.Compression;
using System.Text;
using ZeroDocSigner.AnyDocument.Constants;
using ZeroDocSigner.AnyDocument.Extensions;
using ZeroDocSigner.AnyDocument.Models;

namespace ZeroDocSigner.AnyDocument.Services;

internal class DocumentParser
{
    private readonly DocumentType documentType;

    public DocumentParser(MemoryStream documentStream, DocumentType documentType)
    {
        this.documentType = documentType;

        (DataToSign, Signatures) = DivideFile(documentStream);
        documentStream.Seek(0, SeekOrigin.Begin);
    }

    public byte[] DataToSign { get; }
    public byte[]? DocumentContent { get; private set; }
    public List<JsonSignature> Signatures { get; }

    private (byte[], List<JsonSignature>) DivideFile(MemoryStream documentStream)
    {
        return (GetContent(documentStream), GetSignatures(documentStream));
    }

    private byte[] GetContent(MemoryStream documentStream)
    {
        return documentType switch
        {
            DocumentType.Binary => GetContentFromBinary(documentStream.ToArray()),
            DocumentType.Archive => GetContentFromArchive(documentStream),
            _ => throw new InvalidOperationException("Unknown document type.")
        };
    }

    private static byte[] GetContentFromBinary(byte[] data)
    {
        var signaturesStart = GetSignaturesMarkIndex(data);

        if (signaturesStart == -1)
        {
            return data;
        }

        return data.Take(signaturesStart);
    }

    private static List<JsonSignature> GetSignaturesFromBinary(byte[] data)
    {
        var signaturesStart = GetSignaturesMarkIndex(data);

        if (signaturesStart == -1)
        {
            return new();
        }

        var newLineBytes = Encoding.Default.GetByteCount(Environment.NewLine);
        var indexAfterSignaturesMark = signaturesStart + JsonSignatureConstants.SignaturesStartMark.Length + newLineBytes;

        return Encoding.UTF8.GetString(data.TakeFrom(indexAfterSignaturesMark))
            .DeserializeWithoutEscaping<List<JsonSignature>>() ?? new();
    }

    private byte[] GetContentFromArchive(MemoryStream documentStream)
    {
        var archive = new ZipArchive(documentStream);

        var archiveContent = Enumerable.Empty<byte>();

        foreach (var entry in archive.Entries)
        {
            using var file = entry.Open();
            using var reader = new StreamReader(file);
            archiveContent = archiveContent.Concat(
                Encoding.UTF8.GetBytes(reader.ReadToEnd()));
        }

        DocumentContent = documentStream.ToArray();

        return archiveContent.ToArray();
    }

    private static List<JsonSignature> GetSignaturesFromArchive(MemoryStream documentStream)
    {
        var archive = new ZipArchive(documentStream);
        var signs = archive.Comment;

        if (string.IsNullOrEmpty(signs))
        {
            return new();
        }

        return signs.DeserializeWithoutEscaping<List<JsonSignature>>() ?? new();
    }

    private List<JsonSignature> GetSignatures(MemoryStream documentStream)
    {
        return documentType switch
        {
            DocumentType.Binary => GetSignaturesFromBinary(documentStream.ToArray()),
            DocumentType.Archive => GetSignaturesFromArchive(documentStream),
            _ => throw new InvalidOperationException("Unknown document type.")
        };
    }

    private static long GetSignaturesMarkIndex(byte[] data)
    {
        var markBytes = Encoding.UTF8.GetBytes(JsonSignatureConstants.SignaturesStartMark);
        return data.FindLastSequenceIndex(markBytes);
    }
}
