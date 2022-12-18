using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace ZeroDocSigner.Common.Algorithm
{
    public class SignatureAlgorithm
    {
        private readonly AsymmetricSignatureFormatter _formatter;
        private readonly AsymmetricSignatureDeformatter _deformatter;

        public SignatureAlgorithm(
            AsymmetricSignatureFormatter formatter,
            AsymmetricSignatureDeformatter deformatter,
            HashAlgorithmName hashAlgorithmName)
        {
            _formatter = formatter;
            _deformatter = deformatter;
            HashAlgorithm = hashAlgorithmName;
        }

        public HashAlgorithmName HashAlgorithm { get; }

        public byte[] CreateSignature(byte[] hash)
        {
            return _formatter.CreateSignature(hash);
        }

        public bool VerifySignature(byte[] hash, Signature signature)
        {
            return _deformatter.VerifySignature(hash, signature.Sequence);
        }

        public static SignatureAlgorithm Create(X509Certificate2 certificate)
        {
            var privateKey = GetKeyFromCertificate(certificate, true);
            var publicKey = GetKeyFromCertificate(certificate, false);

            AsymmetricSignatureFormatter formatter;
            AsymmetricSignatureDeformatter deformatter;

            var algorithmName = certificate.SignatureAlgorithm.FriendlyName!;
            if (algorithmName.Contains("RSA", StringComparison.CurrentCultureIgnoreCase))
            {
                formatter = new RSAPKCS1SignatureFormatter(privateKey);
                deformatter = new RSAPKCS1SignatureDeformatter(publicKey);
            }
            else if (algorithmName.Contains("DSA", StringComparison.CurrentCultureIgnoreCase))
            {
                formatter = new DSASignatureFormatter(privateKey);
                deformatter = new DSASignatureDeformatter(publicKey);
            }
            else
            {
                throw new ArgumentException("Unknown key algorithm");
            }

            var signer = new CmsSigner(certificate);
            var hashAlgorithm = HashAlgorithmName.FromOid(signer.DigestAlgorithm.Value!);

            formatter.SetHashAlgorithm(hashAlgorithm.Name!);
            deformatter.SetHashAlgorithm(hashAlgorithm.Name!);

            return new(formatter, deformatter, hashAlgorithm);
        }

        private static AsymmetricAlgorithm GetKeyFromCertificate(X509Certificate2 certificate, bool getPrivate)
        {
            var key = getPrivate
                ? certificate.GetRSAPrivateKey()
                    ?? certificate.GetDSAPrivateKey() as AsymmetricAlgorithm
                    ?? certificate.GetECDsaPrivateKey()
                : certificate.GetRSAPublicKey()
                    ?? certificate.GetDSAPublicKey() as AsymmetricAlgorithm
                    ?? certificate.GetECDsaPublicKey();

            return key!;
        }
    }
}
