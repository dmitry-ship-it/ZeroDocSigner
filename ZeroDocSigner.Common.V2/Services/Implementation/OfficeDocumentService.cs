using System.Security.Cryptography.X509Certificates;
using ZeroDocSigner.Common.V2.Models;
using ZeroDocSigner.Common.V2.Services.Abstractions;

namespace ZeroDocSigner.Common.V2.Services.Implementation;

public class OfficeDocumentService : IOfficeDocumentService
{
    private readonly X509Certificate2 certificate;

    public OfficeDocumentService(X509Certificate2 certificate)
    {
        this.certificate = certificate;
    }

    public byte[] Sign(OfficeSignatureInfo signatureInfo)
    {
        using var document = new OfficeDocument(signatureInfo.Document);
        document.Sign(certificate, signatureInfo);

        return document.GetData();
    }
}
