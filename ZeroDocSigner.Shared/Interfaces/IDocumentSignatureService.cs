namespace ZeroDocSigner.Shared.Interfaces;

public interface IDocumentSignatureService<in TSignatureInfo>
    where TSignatureInfo : ISignatureInfo
{
    byte[] Sign(TSignatureInfo signatureInfo);
}
