using System.IO.Compression;
using System.Text;
using ZeroDocSigner.Common.Algorithm;
using ZeroDocSigner.Common.Extensions;

namespace ZeroDocSigner.Common.Manager;

public class DocumentBuilder
{
    private readonly byte[] originalData;
    private readonly DocumentType documentType;
    private byte[]? modifiedData;
    private SignatureInfo? signatureInfo;

    public DocumentBuilder(byte[] originalData, DocumentType documentType)
    {
        this.originalData = originalData;
        this.documentType = documentType;
    }

    public void SetContent(byte[] content)
    {
        modifiedData = content;
    }

    public void SetSignatureInfo(SignatureInfo signatureInfo)
    {
        this.signatureInfo = signatureInfo;
    }

    public byte[] Build() =>
        documentType switch
        {
            DocumentType.Binary => BuildBinary(),
            DocumentType.Archive => BuildArchive(),
            _ => throw new InvalidOperationException("Unknown document type.")
        };

    private byte[] BuildBinary()
    {
        if (modifiedData is null)
        {
            throw new InvalidOperationException("File content was not privided.");
        }

        if (signatureInfo is null)
        {
            return modifiedData;
        }

        var signatureBytes = Encoding.Default.GetBytes(signatureInfo.ToString());

        return modifiedData.StickWith(signatureBytes);
    }

    private byte[] BuildArchive()
    {
        if (signatureInfo is null)
        {
            return originalData;
        }

        using var memory = new MemoryStream();
        memory.Write(originalData);
        using (var archive = new ZipArchive(memory, ZipArchiveMode.Update))
        {
            archive.Comment = signatureInfo.Serialize();
        }

        return memory.ToArray();
    }
}
