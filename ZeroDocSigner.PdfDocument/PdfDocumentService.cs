using Microsoft.Extensions.Logging;
using ZeroDocSigner.PdfDocument.Models;
using ZeroDocSigner.Shared.Interfaces;

namespace ZeroDocSigner.PdfDocument;

/// <summary>
/// Use this class for DI with <see cref="IDocumentSignatureService{TSignatureInfo}"/>.
/// It's generic argument should be <see cref="PdfSignatureInfo"/>.
/// </summary>
public class PdfDocumentService : IDocumentSignatureService<PdfSignatureInfo>
{
    private readonly ICertificateHolder certificateHolder;
    private readonly ILogger<PdfDocumentService> logger;

    public PdfDocumentService(ICertificateHolder certificateHolder, ILogger<PdfDocumentService> logger)
    {
        this.certificateHolder = certificateHolder;
        this.logger = logger;
    }

    public byte[] Sign(PdfSignatureInfo signatureInfo)
    {
        logger.LogInformation("Signing pdf document, reason is {Reason}, file size is {Size} bytes",
            signatureInfo.Reason, signatureInfo.Document.Length);

        using var document = new Services.PdfDocument(signatureInfo.Document);
        document.Sign(certificateHolder.CertificateStream, certificateHolder.Password, signatureInfo);

        return document.GetData();
    }
}
