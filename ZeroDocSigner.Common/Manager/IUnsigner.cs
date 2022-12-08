using ZeroDocSigner.Common.Algorithm;

namespace ZeroDocSigner.Common.Manager
{
    public interface IUnsigner
    {
        public byte[] RemoveSignature(Signature signature);

        public byte[] RemoveAllSignatures();
    }
}
