﻿using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using ZeroDocSigner.OfficeDocument.Models;
using ZeroDocSigner.OfficeDocument.Services.Abstractions;
using ZeroDocSigner.OfficeDocument.Services.Implementation;
using ZeroDocSigner.Shared.Constants;
using ZeroDocSigner.Shared.Extensions;
using ZeroDocSigner.Shared.Interfaces;

namespace ZeroDocSigner.OfficeDocument.Services;

internal class OfficeDocumentSignature : IXmlDocumentSignature
{
    private readonly string packageObjectTemplate = """
        <Object Id="idPackageObject"><Manifest></Manifest><SignatureProperties><SignatureProperty Id="idSignatureTime" Target="#idPackageSignature"><mdssi:SignatureTime xmlns:mdssi="http://schemas.openxmlformats.org/package/2006/digital-signature"><mdssi:Format>YYYY-MM-DDThh:mm:ssTZD</mdssi:Format><mdssi:Value>{0}</mdssi:Value></mdssi:SignatureTime></SignatureProperty></SignatureProperties></Object>
        """;

    private readonly string officeObjectTemplate = """
        <Object Id="idOfficeObject"><SignatureProperties><SignatureProperty Id="idOfficeV1Details" Target="#idPackageSignature"><SignatureInfoV1 xmlns="http://schemas.microsoft.com/office/2006/digsig"><SetupID/><SignatureText/><SignatureImage/><SignatureComments>{0}</SignatureComments><WindowsVersion>10.0</WindowsVersion><OfficeVersion>16.0.14332/22</OfficeVersion><ApplicationVersion>16.0.14332</ApplicationVersion><Monitors>1</Monitors><HorizontalResolution>1920</HorizontalResolution><VerticalResolution>1080</VerticalResolution><ColorDepth>32</ColorDepth><SignatureProviderId>{00000000-0000-0000-0000-000000000000}</SignatureProviderId><SignatureProviderUrl/><SignatureProviderDetails>9</SignatureProviderDetails><SignatureType>1</SignatureType></SignatureInfoV1><SignatureInfoV2 xmlns="http://schemas.microsoft.com/office/2006/digsig"><Address1>{1}</Address1><Address2>{2}</Address2></SignatureInfoV2></SignatureProperty></SignatureProperties></Object>
        """;

    private readonly string signedPropertiesTemplate = """
        <Object><xd:QualifyingProperties xmlns:xd="http://uri.etsi.org/01903/v1.3.2#" Target="#idPackageSignature"><xd:SignedProperties Id="idSignedProperties"><xd:SignedSignatureProperties><xd:SigningTime>{0}</xd:SigningTime><xd:SigningCertificate><xd:Cert><xd:CertDigest><DigestMethod Algorithm="http://www.w3.org/2001/04/xmlenc#sha256"/><DigestValue>{1}</DigestValue></xd:CertDigest><xd:IssuerSerial><X509IssuerName>{2}</X509IssuerName><X509SerialNumber>{3}</X509SerialNumber></xd:IssuerSerial></xd:Cert></xd:SigningCertificate><xd:SignaturePolicyIdentifier><xd:SignaturePolicyImplied/></xd:SignaturePolicyIdentifier><xd:SignatureProductionPlace><xd:City>{4}</xd:City><xd:StateOrProvince>{5}</xd:StateOrProvince><xd:PostalCode>{6}</xd:PostalCode><xd:CountryName>{7}</xd:CountryName></xd:SignatureProductionPlace><xd:SignerRole><xd:ClaimedRoles><xd:ClaimedRole>{8}</xd:ClaimedRole></xd:ClaimedRoles></xd:SignerRole></xd:SignedSignatureProperties><xd:SignedDataObjectProperties><xd:CommitmentTypeIndication><xd:CommitmentTypeId><xd:Identifier>http://uri.etsi.org/01903/v1.2.2#ProofOfOrigin</xd:Identifier><xd:Description>{9}</xd:Description></xd:CommitmentTypeId><xd:AllSignedDataObjects/><xd:CommitmentTypeQualifiers><xd:CommitmentTypeQualifier>{10}</xd:CommitmentTypeQualifier></xd:CommitmentTypeQualifiers></xd:CommitmentTypeIndication></xd:SignedDataObjectProperties></xd:SignedProperties></xd:QualifyingProperties></Object>
        """;

    private readonly IXmlReferenceNodeFactory referenceNodeFactory;

    private readonly X509Certificate2 certificate;
    private readonly OfficeSignatureInfo signatureInfo;
    private readonly IList<SignaturePart> signatureParts;

    public OfficeDocumentSignature(
        X509Certificate2 certificate,
        OfficeSignatureInfo signatureInfo,
        IList<SignaturePart> signatureParts,
        Dictionary<string, string> overrideContentTypes,
        Dictionary<string, string> defaultContentTypes)
    {
        this.certificate = certificate;
        this.signatureInfo = signatureInfo;
        this.signatureParts = signatureParts;

        referenceNodeFactory = new XmlReferenceNodeFactory(overrideContentTypes, defaultContentTypes);
    }

    public XmlDocument CreateSignatureDocument()
    {
        var dataElements = GetDataObjects();

        // reference to package object node
        var packageObjectReference = new Reference(XmlSignatureConstants.DataObjects.PackageReferenceUri)
        {
            Type = XmlSignatureConstants.Uris.XmlObjectScheme,
            DigestMethod = DigestMethodsUris.Sha256,
        };

        // reference to office object node
        var officeObjectReference = new Reference(XmlSignatureConstants.DataObjects.OfficeReferenceUri)
        {
            Type = XmlSignatureConstants.Uris.XmlObjectScheme,
            DigestMethod = DigestMethodsUris.Sha256,
        };

        // reference to signed properties object node
        var signedPropertiesReference = new Reference(XmlSignatureConstants.DataObjects.SignedPropertiesReferenceUri)
        {
            Type = XmlSignatureConstants.Uris.XmlSignedPropertiesScheme,
            DigestMethod = DigestMethodsUris.Sha256,
        };

        // only this object should be transformed with XML-C14N on signing or checking signature
        signedPropertiesReference.AddTransform(new XmlDsigC14NTransform());

        var document = new XmlDocument
        {
            PreserveWhitespace = true
        };

        // using simple string concatenation instead of XmlDocument constructing
        // because XmlDocument applies wrong format on element appending
        document.LoadXml(XmlSignatureConstants.XmlDeclaration
            + "<Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\" Id=\"idPackageSignature\">"
            + dataElements.Package.OuterXml
            + dataElements.Office.OuterXml
            + dataElements.SignedProperties.OuterXml
            + "</Signature>");

        var signedXml = new SignedXml(document);

        // fill signedXml with all default values
        signedXml.AddReference(packageObjectReference);
        signedXml.AddReference(officeObjectReference);
        signedXml.AddReference(signedPropertiesReference);

        signedXml.Signature.Id = XmlSignatureConstants.DataObjects.RootNodeId;
        signedXml.SignedInfo.CanonicalizationMethod = XmlSignatureConstants.Uris.XmlCanonicalizationMethod;
        signedXml.SignedInfo.SignatureMethod = XmlSignatureConstants.Uris.XmlSignatureMethod;
        signedXml.KeyInfo.AddClause(new KeyInfoX509Data(certificate));
        signedXml.SigningKey = certificate.GetRSAPrivateKey();

        // this call computes missing digests and signature value
        signedXml.ComputeSignature();

        var output = new XmlDocument();
        output.LoadXml(XmlSignatureConstants.XmlDeclaration + signedXml.GetXml().OuterXml);

        // now we using XmlDocument constructing because we don't care about format of result doc
        // when MS Office checks signature it is by itself formats this doc properly
        var root = (XmlElement)output.GetElementsByTagName(XmlSignatureConstants.Nodes.Signature)[0]!;
        AppendDataElements(root, dataElements);

        // the only way to remove redundant empty XML namespaces which XmlDocument adds for some reason
        output.LoadXml(output.OuterXml.Replace(" xmlns=\"\"", string.Empty));

        return output;
    }

    private OfficeSignatureObjectElements GetDataObjects()
    {
        var signingDateTime = DateTimeOffset.UtcNow.ToString(XmlSignatureConstants.DateTimeOffsetFormat);

        return new(
            GetPackageObject(signingDateTime),
            GetOfficeObject(),
            GetSignedPropertiesObject(signingDateTime)
        );
    }

    private XmlElement GetPackageObject(string signingDateTime)
    {
        var document = new XmlDocument()
        {
            PreserveWhitespace = true
        };

        document.LoadXml(packageObjectTemplate.Format(signingDateTime));

        // add references to all needed files inside archive
        var manifestElement = document.GetElementsByTagName(XmlSignatureConstants.Nodes.Manifest)[0]!;
        foreach (var signaturePart in signatureParts)
        {
            var referenceNode = referenceNodeFactory.Create(signaturePart.EntryName, signaturePart.Data);
            var referenceNodeXml = referenceNode.GetXmlElement();

            var imported = document.ImportNode(referenceNodeXml, true);
            manifestElement.AppendChild(imported);
        }

        return (XmlElement)document.GetElementsByTagName(XmlSignatureConstants.Nodes.Object)[0]!;
    }

    private XmlElement GetOfficeObject()
    {
        var document = new XmlDocument()
        {
            PreserveWhitespace = true
        };

        // seed all needed values to template and load it as XmlDocument
        document.LoadXml(officeObjectTemplate.Format(
            signatureInfo.SignatureComments,
            signatureInfo.AddressPrimary,
            signatureInfo.AddressSecondary));

        return (XmlElement)document.GetElementsByTagName(XmlSignatureConstants.Nodes.Object)[0]!;
    }

    private XmlElement GetSignedPropertiesObject(string signingDateTime)
    {
        var document = new XmlDocument()
        {
            PreserveWhitespace = true
        };

        // put all needed values to template and load it as XmlDocument
        document.LoadXml(signedPropertiesTemplate.Format(
            signingDateTime,
            Convert.ToBase64String(SHA256.HashData(certificate.RawData)),
            certificate.Issuer,
            certificate.GetFormattedSerialNumber(),
            signatureInfo.City,
            signatureInfo.StateOrProvince,
            signatureInfo.PostalCode,
            signatureInfo.CountryName,
            signatureInfo.SignerRole,
            signatureInfo.CommitmentType,
            signatureInfo.SignatureComments));

        return (XmlElement)document.GetElementsByTagName(XmlSignatureConstants.Nodes.Object)[0]!;
    }

    private static void AppendDataElements(XmlElement element, OfficeSignatureObjectElements objectElements)
    {
        var node = element.OwnerDocument.ImportNode(objectElements.Package, true);
        element.AppendChild(node);

        node = element.OwnerDocument.ImportNode(objectElements.Office, true);
        element.AppendChild(node);

        node = element.OwnerDocument.ImportNode(objectElements.SignedProperties, true);
        element.AppendChild(node);
    }
}
