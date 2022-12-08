using ZeroDocSigner.Common.Algorithm;

namespace ZeroDocSigner.Common.Manager
{
    public interface IVerifier
    {
        public bool Verify();

        public bool Verify(Signature signature);
    }
}
