using Microsoft.Extensions.Logging;
using ZeroDocSigner.AnyDocument.Interfaces;
using ZeroDocSigner.AnyDocument.Models;
using ZeroDocSigner.Shared.Interfaces;

namespace ZeroDocSigner.AnyDocument;

public class AnyDocumentService : IAnyDocumentSignatureService
{
    private readonly ICertificateHolder certificateHolder;
    private readonly ILogger<AnyDocumentService> logger;

    public AnyDocumentService(ICertificateHolder certificateHolder, ILogger<AnyDocumentService> logger)
    {
        this.certificateHolder = certificateHolder;
        this.logger = logger;
    }

    public byte[] Sign(AnySignatureInfo signatureInfo)
    {
        logger.LogInformation("Signing document. Signer is '{Signer}', size={Size} bytes",
            signatureInfo.SignerRole,
            signatureInfo.Document.Length);

        using var document = new Services.AnyDocument(signatureInfo.Document);
        document.Sign(certificateHolder.CertificateStream, certificateHolder.Password, signatureInfo);

        return document.GetData();
    }

    public AnyDocumentVerificationInfo[] Verify(byte[] document)
    {
        logger.LogInformation("Verifying document. Size={Size} bytes", document.Length);

        using var doc = new Services.AnyDocument(document);

        return doc.Verify();
    }
}
