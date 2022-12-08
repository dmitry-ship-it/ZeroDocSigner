using ZeroDocSigner.Common.Algorithm;

namespace ZeroDocSigner.Common.Manager
{
    public interface ISigner
    {
        public SignatureInfo CreateSignature(SignatureParameters parameters, bool force = false);

        public SignatureInfo AddSignature(SignatureParameters parameters);

        public SignatureInfo RemoveSignature(Signature signature);
    }
}
