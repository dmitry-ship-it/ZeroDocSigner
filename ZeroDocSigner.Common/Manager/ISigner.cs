using ZeroDocSigner.Common.Algorithm;

namespace ZeroDocSigner.Common.Manager
{
    public interface ISigner
    {
        public void CreateSignature(SignatureParameters parameters, bool force = false);

        public void AddSignature(SignatureParameters parameters);

        public void RemoveSignature(Signature signature);
    }
}
