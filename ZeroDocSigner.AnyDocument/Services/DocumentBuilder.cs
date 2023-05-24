using System.IO.Compression;
using System.Text;
using ZeroDocSigner.AnyDocument.Constants;
using ZeroDocSigner.AnyDocument.Extensions;
using ZeroDocSigner.AnyDocument.Models;

namespace ZeroDocSigner.AnyDocument.Services;

internal class DocumentBuilder
{
    private readonly DocumentType documentType;

    public DocumentBuilder(DocumentType documentType, byte[] data, List<JsonSignature> signatures)
    {
        this.documentType = documentType;
        Data = data;
        Signatures = signatures;
    }

    public byte[] Data { get; set; }
    public List<JsonSignature> Signatures { get; }

    public byte[] Build() =>
        documentType switch
        {
            DocumentType.Binary => BuildBinary(),
            DocumentType.Archive => BuildArchive(),
            _ => throw new InvalidOperationException("Unknown document type.")
        };

    private byte[] BuildBinary()
    {
        if (Data is null || Data.Length == 0)
        {
            throw new InvalidOperationException("File content was not provided.");
        }

        if (Signatures is null || Signatures.Count == 0)
        {
            return Data;
        }

        var startMarkBytes = Encoding.UTF8.GetBytes(JsonSignatureConstants.SignaturesStartMark);

        return Data.StickWith(startMarkBytes.StickWith(BuildSignatureForBinary()));
    }

    private byte[] BuildArchive()
    {
        if (Signatures is null || Signatures.Count == 0)
        {
            return Data;
        }

        using var memory = new MemoryStream();
        memory.Write(Data);
        using (var archive = new ZipArchive(memory, ZipArchiveMode.Update))
        {
            archive.Comment = Signatures.SerializeWithoutEscaping();
        }

        return memory.ToArray();
    }

    private byte[] BuildSignatureForBinary()
    {
        return Encoding.UTF8.GetBytes(
            Environment.NewLine +
            Signatures.SerializeWithoutEscaping());
    }
}
