using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using ZeroDocSigner.AnyDocument.Extensions;
using ZeroDocSigner.AnyDocument.Models;

namespace ZeroDocSigner.AnyDocument.Services;

internal static class AnyDocumentSigner
{
    public static void Sign(DocumentBuilder documentBuilder, X509Certificate2 certificate, AnySignatureInfo signatureInfo, byte[] data)
    {
        if (signatureInfo.Force)
        {
            documentBuilder.Signatures.Clear();
        }

        var signedProperties = ConvertToSignedProperties(signatureInfo);
        var signature = ComputeSignature(certificate, data, signedProperties);

        documentBuilder.Signatures.Add(signature);
    }

    public static AnyDocumentVerificationInfo[] Verify(DocumentBuilder documentBuilder, byte[] data)
    {
        if (documentBuilder.Signatures.Count == 0)
        {
            return Array.Empty<AnyDocumentVerificationInfo>();
        }

        var result = new AnyDocumentVerificationInfo[documentBuilder.Signatures.Count];
        for (var i = 0; i < result.Length; i++)
        {
            result[i] = VerifySignature(documentBuilder.Signatures[i], data);
        }

        return result;
    }

    private static JsonSignedProperties ConvertToSignedProperties(AnySignatureInfo signatureInfo)
    {
        return new JsonSignedProperties(
            signatureInfo.SignatureComments,
            signatureInfo.AddressPrimary,
            signatureInfo.AddressSecondary,
            signatureInfo.City,
            signatureInfo.StateOrProvince,
            signatureInfo.PostalCode,
            signatureInfo.CountryName,
            signatureInfo.SignerRole,
            signatureInfo.CommitmentType,
            DateTimeOffset.Now);
    }

    private static AnyDocumentVerificationInfo ConvertToVerificationInfo(JsonSignedProperties signedProperties, bool isValid)
    {
        return new AnyDocumentVerificationInfo(
            isValid,
            signedProperties.SignatureComments,
            signedProperties.AddressPrimary,
            signedProperties.AddressSecondary,
            signedProperties.City,
            signedProperties.StateOrProvince,
            signedProperties.PostalCode,
            signedProperties.CountryName,
            signedProperties.SignerRole,
            signedProperties.CommitmentType);
    }

    private static JsonSignature ComputeSignature(X509Certificate2 certificate, byte[] data, JsonSignedProperties signedProperties)
    {
        using var privateKey = certificate.GetRSAPrivateKey();
        using var hashAlgorithm = (HashAlgorithm)CryptoConfig.CreateFromName(HashAlgorithmName.SHA256.Name!)!;
        var signer = new RSAPKCS1SignatureFormatter(privateKey!);
        signer.SetHashAlgorithm(HashAlgorithmName.SHA256.Name!);

        var documentHash = hashAlgorithm.ComputeHash(data);
        var propertiesHash = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(signedProperties.SerializeWithoutEscaping()));
        var signature = signer.CreateSignature(hashAlgorithm.ComputeHash(documentHash.StickWith(propertiesHash)));

        var documentHashHolder = new JsonHashHolder(
            HashAlgorithmName.SHA256.Name!,
            Convert.ToBase64String(documentHash));
        var propertiesHashHolder = new JsonHashHolder(
            HashAlgorithmName.SHA256.Name!,
            Convert.ToBase64String(propertiesHash));

        return new JsonSignature(
            Convert.ToBase64String(Encoding.UTF8.GetBytes(signedProperties.SerializeWithoutEscaping())),
            documentHashHolder,
            propertiesHashHolder,
            Convert.ToBase64String(signature),
            Convert.ToBase64String(certificate.RawData));
    }

    private static AnyDocumentVerificationInfo VerifySignature(JsonSignature signature, byte[] data)
    {
        using var certificate = new X509Certificate2(Convert.FromBase64String(signature.Certificate));
        using var key = certificate.GetRSAPublicKey()!;
        using var hashAlgorithm = (HashAlgorithm)CryptoConfig.CreateFromName(signature.DocumentHash.HashAlgorithm)!;

        var verifier = new RSAPKCS1SignatureDeformatter(key);
        verifier.SetHashAlgorithm(signature.DocumentHash.HashAlgorithm);

        var signedPropertiesBytes = Convert.FromBase64String(signature.SignedProperties);

        // documentParser.DataToSign
        var documentHash = hashAlgorithm.ComputeHash(data);
        var propertiesHash = hashAlgorithm.ComputeHash(signedPropertiesBytes);
        var isValid = verifier.VerifySignature(
            hashAlgorithm.ComputeHash(documentHash.StickWith(propertiesHash)),
            Convert.FromBase64String(signature.Signature));

        var signedProperties = Encoding.UTF8.GetString(signedPropertiesBytes)
            .DeserializeWithoutEscaping<JsonSignedProperties>();

        return ConvertToVerificationInfo(signedProperties!, isValid);
    }
}
