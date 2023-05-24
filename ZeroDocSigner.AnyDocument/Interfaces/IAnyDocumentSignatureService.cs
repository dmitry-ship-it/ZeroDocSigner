using ZeroDocSigner.AnyDocument.Models;
using ZeroDocSigner.Shared.Interfaces;

namespace ZeroDocSigner.AnyDocument.Interfaces;

public interface IAnyDocumentSignatureService : IDocumentSignatureService<AnySignatureInfo>
{
    AnyDocumentVerificationInfo[] Verify(byte[] document);
}
