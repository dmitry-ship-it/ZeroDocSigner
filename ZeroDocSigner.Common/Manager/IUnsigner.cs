using ZeroDocSigner.Common.Algorithm;

namespace ZeroDocSigner.Common.Manager;

public interface IUnsigner
{
    public void RemoveSignature(Signature signature);
}
