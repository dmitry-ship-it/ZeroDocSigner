using Microsoft.Extensions.Logging;
using ZeroDocSigner.OfficeDocument.Models;
using ZeroDocSigner.Shared.Interfaces;

namespace ZeroDocSigner.OfficeDocument;

/// <summary>
/// Use this class for DI with <see cref="IDocumentSignatureService{TSignatureInfo}"/>.
/// It's generic argument should be <see cref="OfficeSignatureInfo"/>.
/// </summary>
public class OfficeDocumentService : IDocumentSignatureService<OfficeSignatureInfo>
{
    private readonly ICertificateHolder certificateHolder;
    private readonly ILogger<OfficeDocumentService> logger;

    public OfficeDocumentService(ICertificateHolder certificateHolder, ILogger<OfficeDocumentService> logger)
    {
        this.certificateHolder = certificateHolder;
        this.logger = logger;
    }

    public byte[] Sign(OfficeSignatureInfo signatureInfo)
    {
        logger.LogInformation("Signing open document, signer role is {SignerRole}, file size is {Size} bytes",
            signatureInfo.SignerRole, signatureInfo.Document.Length);

        using var document = new Services.OfficeDocument(signatureInfo.Document);
        document.Sign(certificateHolder.CertificateStream, certificateHolder.Password, signatureInfo);

        return document.GetData();
    }
}
