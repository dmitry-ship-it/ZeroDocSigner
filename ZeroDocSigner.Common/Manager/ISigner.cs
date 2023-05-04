using ZeroDocSigner.Common.Algorithm;

namespace ZeroDocSigner.Common.Manager;

public interface ISigner
{
    public void CreateSignature(bool force = false);

    public void AddSignature();

    public void RemoveSignature(Signature signature);
}
