using System.Text.Json;
using System.Text;
using ZeroDocSigner.Common.Algorithm;
using ZeroDocSigner.Common.Extensions;
using System.IO.Compression;

namespace ZeroDocSigner.Common.Manager;

public class DocumentParser
{
    private readonly DocumentType documentType;

    public DocumentParser(byte[] data, DocumentType documentType)
    {
        this.documentType = documentType;
        (FileContent, SignatureInfo) = DivideFile(data);
    }

    public byte[] FileContent { get; }
    public SignatureInfo? SignatureInfo { get; }

    private (byte[], SignatureInfo?) DivideFile(byte[] data)
    {
        var signaturesStart = data.FindLastSequenceIndex(SignatureInfo.StartSequence);
        return (GetContent(data, signaturesStart), GetSignatures(data, signaturesStart));
    }

    private byte[] GetContent(byte[] data, long signaturesStart)
    {
        return documentType switch
        {
            DocumentType.Binary => GetContentFromBinary(data, signaturesStart),
            DocumentType.Archive => GetContentFromArchive(data),
            _ => throw new InvalidOperationException("Unknown document type.")
        };
    }

    private static byte[] GetContentFromBinary(byte[] data, long signaturesStart)
    {
        if (signaturesStart == -1)
        {
            return data;
        }

        return data.Take(signaturesStart);
    }

    private static SignatureInfo? GetSignaturesFromBinary(byte[] data, long signaturesStart)
    {
        if (signaturesStart == -1)
        {
            return null;
        }

        var newLineBytes = Encoding.Default.GetByteCount(Environment.NewLine);

        return JsonSerializer.Deserialize<SignatureInfo>(
            data.TakeFrom(signaturesStart + SignatureInfo.StartSequence.Length + newLineBytes));
    }

    private static byte[] GetContentFromArchive(byte[] data)
    {
        using var memory = new MemoryStream(data);
        using var archive = new ZipArchive(memory);

        var archiveContent = Enumerable.Empty<byte>();

        foreach (var entry in archive.Entries)
        {
            using var file = entry.Open();
            using var reader = new StreamReader(file);
            archiveContent = archiveContent.Concat(
                Encoding.Default.GetBytes(reader.ReadToEnd()));
        }

        return archiveContent.ToArray();
    }

    private static SignatureInfo? GetSignaturesFromArchive(byte[] data)
    {
        using var memory = new MemoryStream(data);
        using var archive = new ZipArchive(memory);

        var signs = archive.Comment;

        if (string.IsNullOrEmpty(signs))
        {
            return null;
        }

        return JsonSerializer.Deserialize<SignatureInfo>(signs);
    }

    private SignatureInfo? GetSignatures(byte[] data, long signaturesStart)
    {
        return documentType switch
        {
            DocumentType.Binary => GetSignaturesFromBinary(data, signaturesStart),
            DocumentType.Archive => GetSignaturesFromArchive(data),
            _ => throw new InvalidOperationException("Unknown document type.")
        };
    }
}
