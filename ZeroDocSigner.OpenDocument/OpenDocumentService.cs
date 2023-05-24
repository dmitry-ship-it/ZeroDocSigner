using Microsoft.Extensions.Logging;
using ZeroDocSigner.OpenDocument.Models;
using ZeroDocSigner.Shared.Interfaces;

namespace ZeroDocSigner.OpenDocument;

/// <summary>
/// Use this class for DI with <see cref="IDocumentSignatureService{TSignatureInfo}"/>.
/// It's generic argument should be <see cref="OpenSignatureInfo"/>.
/// </summary>
public class OpenDocumentService : IDocumentSignatureService<OpenSignatureInfo>
{
    private readonly ICertificateHolder certificateHolder;
    private readonly ILogger<OpenDocumentService> logger;

    public OpenDocumentService(ICertificateHolder certificateHolder, ILogger<OpenDocumentService> logger)
    {
        this.certificateHolder = certificateHolder;
        this.logger = logger;
    }

    public byte[] Sign(OpenSignatureInfo signatureInfo)
    {
        logger.LogInformation("Signing open document, signer role is {SignerRole}, file size is {Size} bytes",
            signatureInfo.SignerRole, signatureInfo.Document.Length);

        using var document = new Services.OpenDocument(signatureInfo.Document);
        document.Sign(certificateHolder.CertificateStream, certificateHolder.Password, signatureInfo);

        return document.GetData();
    }
}
