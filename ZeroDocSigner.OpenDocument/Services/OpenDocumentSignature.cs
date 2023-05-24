using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using ZeroDocSigner.OpenDocument.Models;
using ZeroDocSigner.Shared.Constants;
using ZeroDocSigner.Shared.Extensions;
using ZeroDocSigner.Shared.Interfaces;

namespace ZeroDocSigner.OpenDocument.Services;

internal class OpenDocumentSignature : IXmlDocumentSignature
{
    private readonly string signedPropertiesTemplate = """
        <Object><xd:QualifyingProperties xmlns:xd="http://uri.etsi.org/01903/v1.3.2#" Target="#idPackageSignature_{0}"><xd:SignedProperties Id="idSignedProperties_{0}"><xd:SignedSignatureProperties><xd:SigningTime>{1}</xd:SigningTime><xd:SigningCertificate><xd:Cert><xd:CertDigest><DigestMethod Algorithm="http://www.w3.org/2001/04/xmlenc#sha256" /><DigestValue>{2}</DigestValue></xd:CertDigest><xd:IssuerSerial><X509IssuerName>{3}</X509IssuerName><X509SerialNumber>{4}</X509SerialNumber></xd:IssuerSerial></xd:Cert></xd:SigningCertificate><xd:SignaturePolicyIdentifier><xd:SignaturePolicyImplied /></xd:SignaturePolicyIdentifier><xd:SignatureProductionPlace><xd:City>{5}</xd:City><xd:StateOrProvince>{6}</xd:StateOrProvince><xd:PostalCode>{7}</xd:PostalCode><xd:CountryName>{8}</xd:CountryName></xd:SignatureProductionPlace><xd:SignerRole><xd:ClaimedRoles><xd:ClaimedRole>{9}</xd:ClaimedRole></xd:ClaimedRoles></xd:SignerRole></xd:SignedSignatureProperties><xd:SignedDataObjectProperties><xd:CommitmentTypeIndication><xd:CommitmentTypeId><xd:Identifier>http://uri.etsi.org/01903/v1.2.2#ProofOfOrigin</xd:Identifier><xd:Description>{10}</xd:Description></xd:CommitmentTypeId><xd:AllSignedDataObjects /><xd:CommitmentTypeQualifiers><xd:CommitmentTypeQualifier>{11}</xd:CommitmentTypeQualifier></xd:CommitmentTypeQualifiers></xd:CommitmentTypeIndication></xd:SignedDataObjectProperties></xd:SignedProperties></xd:QualifyingProperties></Object>
        """;

    private readonly string signatureId = Guid.NewGuid().ToString()
        .Replace("-", string.Empty)
        .ToLower();

    private readonly XmlDsigC14NTransform transform = new();

    private readonly X509Certificate2 certificate;
    private readonly OpenSignatureInfo signatureInfo;
    private readonly Dictionary<string, Stream> entries;

    public OpenDocumentSignature(
        X509Certificate2 certificate,
        OpenSignatureInfo signatureInfo,
        Dictionary<string, Stream> entries)
    {
        this.certificate = certificate;
        this.signatureInfo = signatureInfo;
        this.entries = entries;
    }

    public XmlDocument CreateSignatureDocument()
    {
        var signedProperties = GetFilledSignedProperties();
        var signedXml = GetConfiguredSignedXml(signedProperties);

        // placeholder
        signedXml.Signature.SignatureValue = Array.Empty<byte>();

        var result = new XmlDocument()
        {
            PreserveWhitespace = true
        };

        var imported = result.ImportNode(signedXml.GetXml(), true);
        result.AppendChild(imported);

        AppendObjectElement(result, signedProperties);
        ComputeSignature(result);

        result.LoadXml(result.OuterXml.Replace(" xmlns=\"\"", string.Empty));

        return result;
    }

    private XmlDocument GetFilledSignedProperties()
    {
        return signedPropertiesTemplate.Format(
            signatureId,
            DateTimeOffset.UtcNow.ToString(XmlSignatureConstants.DateTimeOffsetFormat),
            Convert.ToBase64String(SHA256.HashData(certificate.RawData)),
            certificate.Issuer,
            certificate.GetFormattedSerialNumber(),
            signatureInfo.City,
            signatureInfo.StateOrProvince,
            signatureInfo.PostalCode,
            signatureInfo.CountryName,
            signatureInfo.SignerRole,
            signatureInfo.CommitmentType,
            signatureInfo.SignatureComments).ToXmlDocument();
    }

    private SignedXml GetConfiguredSignedXml(XmlDocument signedProperties)
    {
        // Create signedXml and add first reference - SignedProperties
        var signedXml = new SignedXml(signedProperties);
        var signedPropertiesReference = new Reference()
        {
            Type = XmlSignatureConstants.Uris.XmlSignedPropertiesScheme,
            DigestMethod = DigestMethodsUris.Sha256,
            Uri = $"{XmlSignatureConstants.DataObjects.SignedPropertiesReferenceUri}_{signatureId}"
        };

        signedPropertiesReference.DigestValue = GetDataObjectHash(signedProperties, signedPropertiesReference.Uri);
        signedPropertiesReference.AddTransform(transform);
        signedXml.AddReference(signedPropertiesReference);

        // add other references
        using var sha256 = SHA256.Create();
        foreach (var entry in entries)
        {
            // if file is xml doc where all signatures lives we have to skip it
            if (entry.Key == XmlSignatureConstants.EntryFullNames.OpenDocumentSignaturesEntry)
            {
                continue;
            }

            var reference = new Reference(entry.Value)
            {
                Type = XmlSignatureConstants.Uris.OpenDocumentReferenceType,
                DigestMethod = DigestMethodsUris.Sha256,
                Uri = entry.Key
            };

            if (entry.Key.EndsWith(".xml"))
            {
                // if file is xml doc we should apply transform on it
                transform.LoadInput(entry.Value);
                reference.DigestValue = transform.GetDigestedOutput(sha256);
                reference.AddTransform(transform);
            }
            else
            {
                // otherwise just compute its hash
                reference.DigestValue = sha256.ComputeHash(entry.Value);
            }

            signedXml.AddReference(reference);
        }

        FillDefaultProperties(signedXml);

        return signedXml;
    }

    private void FillDefaultProperties(SignedXml signedXml)
    {
        signedXml.Signature.Id = $"{XmlSignatureConstants.DataObjects.RootNodeId}_{signatureId}";
        signedXml.SignedInfo.CanonicalizationMethod = XmlSignatureConstants.Uris.XmlCanonicalizationMethod;
        signedXml.SignedInfo.SignatureMethod = XmlSignatureConstants.Uris.XmlSignatureMethod;

        var x509KeyData = new KeyInfoX509Data(certificate);
        x509KeyData.AddIssuerSerial(certificate.Issuer, certificate.SerialNumber);
        signedXml.KeyInfo.AddClause(x509KeyData);

        signedXml.SigningKey = certificate.GetRSAPrivateKey();
    }

    private static void AppendObjectElement(XmlDocument xmlDocument, XmlDocument objectDocument)
    {
        var imported = xmlDocument.ImportNode(objectDocument.FirstChild!, true);
        xmlDocument.DocumentElement!.AppendChild(imported);
    }

    // I spent too many hours struggling with data object hash reverse-engineering
    // so I decided to let SignedXml compute it for us
    private byte[] GetDataObjectHash(XmlDocument document, string uri)
    {
        document = (XmlDocument)document.Clone();
        document.LoadXml(
            "<Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\">"
            + document.OuterXml
            + "</Signature>");

        var signedXml = new SignedXml(document);
        var signedPropertiesReference = new Reference(uri)
        {
            Type = XmlSignatureConstants.Uris.XmlSignedPropertiesScheme,
            DigestMethod = DigestMethodsUris.Sha256,
        };

        signedPropertiesReference.AddTransform(transform);
        signedXml.AddReference(signedPropertiesReference);

        FillDefaultProperties(signedXml);
        signedXml.ComputeSignature();

        var targetElement = (XmlElement)signedXml.GetXml()
            .GetElementsByTagName(XmlSignatureConstants.Nodes.DigestValue)[0]!;

        return Convert.FromBase64String(targetElement.InnerText);
    }

    private void ComputeSignature(XmlDocument document)
    {
        // get SignedInfo node and add proper namespace to it
        var target = (XmlElement)document.GetElementsByTagName(XmlSignatureConstants.Nodes.SignedInfo)[0]!.Clone();
        target.SetAttribute(
            XmlSignatureConstants.Attributes.DefaultNamespace,
            XmlSignatureConstants.Uris.XmlSignature);

        var documentForSigning = new XmlDocument()
        {
            PreserveWhitespace = true
        };

        documentForSigning.LoadXml(target.OuterXml);

        // transform and hash it
        transform.LoadInput(documentForSigning);
        using var sha256 = SHA256.Create();
        var hash = transform.GetDigestedOutput(sha256);

        // compute its signature
        using var rsa = certificate.GetRSAPrivateKey()!;
        var signature = rsa.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        // and finally write signature value to SignatureValue node as base64 text
        var signatureValueNode = document.GetElementsByTagName(XmlSignatureConstants.Nodes.SignatureValue)[0]!;
        signatureValueNode.InnerText = Convert.ToBase64String(signature);
    }
}
