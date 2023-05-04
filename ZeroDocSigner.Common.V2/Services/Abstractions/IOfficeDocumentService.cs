using ZeroDocSigner.Common.V2.Models;

namespace ZeroDocSigner.Common.V2.Services.Abstractions;

public interface IOfficeDocumentService
{
    byte[] Sign(OfficeSignatureInfo signatureInfo);
}
