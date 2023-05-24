using ZeroDocSigner.AnyDocument.Models;

namespace ZeroDocSigner.AnyDocument.Interfaces;

internal interface IDocumentSignatureVerifier
{
    AnyDocumentVerificationInfo[] Verify();
}
