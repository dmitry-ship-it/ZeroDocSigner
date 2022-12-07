using ZeroDocSigner.Common.Algorithm;

namespace ZeroDocSigner.Common
{
    public interface ISigner
    {
        public byte[] CreateSignature(SignatureParameters parameters, bool force = false);

        public byte[] AddSignature();
    }
}
