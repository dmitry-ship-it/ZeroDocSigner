using ZeroDocSigner.Common.Algorithm;

namespace ZeroDocSigner.Common.Manager
{
    public interface ISigner
    {
        public byte[] CreateSignature(SignatureParameters parameters, bool force = false);

        public byte[] AddSignature(SignatureParameters parameters);

        public byte[] RemoveSignature(Signature signature);

        public byte[] RemoveAllSignatures();
    }
}
