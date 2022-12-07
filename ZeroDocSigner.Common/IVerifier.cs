using ZeroDocSigner.Common.Algorithm;

namespace ZeroDocSigner.Common
{
    public interface IVerifier
    {
        public bool Verify();

        public bool Verify(Signature signature);

        public bool DataContainsSignature { get; }
    }
}
