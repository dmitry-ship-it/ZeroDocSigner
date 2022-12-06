using ZeroDocSigner.Common.Algorithm;

namespace ZeroDocSigner.Common
{
    public interface ISigner
    {
        public void CreateSignature(SignatureParameters parameters, bool force = false);

        public void AddSignature();
    }
}
