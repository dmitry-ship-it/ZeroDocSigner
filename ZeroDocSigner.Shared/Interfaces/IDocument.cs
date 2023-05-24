namespace ZeroDocSigner.Shared.Interfaces;

public interface IDocument<in TSignatureInfo> : IDisposable
    where TSignatureInfo : ISignatureInfo
{
    bool ContainsSignatures { get; }

    void Sign(Stream certificate, string? certificatePassword, TSignatureInfo signatureInfo);

    public byte[] GetData();
}
