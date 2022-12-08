using ZeroDocSigner.Common.Algorithm;

namespace ZeroDocSigner.Common.Manager
{
    public interface IUnsigner
    {
        public SignatureInfo RemoveSignature(Signature signature);
    }
}
