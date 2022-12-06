using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace ZeroDocSigner.Common.Algorithm
{
    public class SignatureAlgorithm
    {
        private readonly AsymmetricSignatureFormatter _formatter;
        private readonly AsymmetricSignatureDeformatter _deformatter;

        public SignatureAlgorithm(
            AsymmetricSignatureFormatter formatter,
            AsymmetricSignatureDeformatter deformatter)
        {
            _formatter = formatter;
            _deformatter = deformatter;
        }

        public byte[] CreateSignature(byte[] hash)
        {
            return _formatter.CreateSignature(hash);
        }

        public bool VerifySignature(byte[] hash, Signature signature) 
        {
            return _deformatter.VerifySignature(hash, signature.Sequence);
        }

        public static SignatureAlgorithm Create(
            SignatureParameters parameters,
            X509Certificate2 certificate)
        {
            AsymmetricSignatureFormatter formatter;
            AsymmetricSignatureDeformatter deformatter;

            switch (parameters.SignatureAlgorithmName)
            {
                case SignatureAlgorithmName.DSA:
                    formatter = new DSASignatureFormatter(certificate.GetDSAPrivateKey()!);
                    deformatter = new DSASignatureDeformatter(certificate.GetDSAPublicKey()!);
                    break;
                case SignatureAlgorithmName.ECDsa:
                    // TODO: Implement for ECDsa
                    throw new NotImplementedException();
                    //break;
                case SignatureAlgorithmName.RSA:
                    formatter = new RSAPKCS1SignatureFormatter(certificate.GetRSAPrivateKey()!);
                    deformatter = new RSAPKCS1SignatureDeformatter(certificate.GetRSAPublicKey()!);
                    break;
                default:
                    throw new InvalidOperationException($"Signature algorithm name '{parameters.SignatureAlgorithmName}' is not found.");
            }

            formatter.SetHashAlgorithm(parameters.HashAlgorithmName.Name);
            deformatter.SetHashAlgorithm(parameters.HashAlgorithmName.Name);

            return new(formatter, deformatter);
        }
    }
}
