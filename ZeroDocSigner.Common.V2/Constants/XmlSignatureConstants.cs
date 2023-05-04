namespace ZeroDocSigner.Common.V2.Constants;

public static class XmlSignatureConstants
{
    public const string DateTimeOffsetFormat = "yyyy-MM-ddTHH:mm:ssZ";
    public const string XmlDeclaration = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";

    public readonly struct Nodes
    {
        public const string Signature = "Signature";
        public const string Transform = "Transform";
        public const string Reference = "Reference";
        public const string Manifest = "Manifest";
        public const string Object = "Object";
        public const string RelationshipReference = "RelationshipReference";
    }

    public readonly struct Attributes
    {
        public const string Algorithm = "Algorithm";
        public const string SourceId = "SourceId";
        public const string Id = "Id";

        public readonly struct Values
        {
            public const string IdPackageSignature = "idPackageSignature";
        }
    }

    public readonly struct NodePrefixes
    {
        public const string Mdssi = "mdssi";
        public const string R = "r";
    }

    public readonly struct Uris
    {
        public const string PackageDigitalSignatureRelationship = "http://schemas.openxmlformats.org/package/2006/relationships/digital-signature/signature";
        public const string PackageDigitalSignature = "http://schemas.openxmlformats.org/package/2006/digital-signature";
        public const string DigitalSignatureOrigin = "http://schemas.openxmlformats.org/package/2006/relationships/digital-signature/origin";

        public const string XmlObjectScheme = "http://www.w3.org/2000/09/xmldsig#Object";
        public const string XmlSignedPropertiesScheme = "http://uri.etsi.org/01903#SignedProperties";
        public const string XmlCanonicalizationMethod = "http://www.w3.org/TR/2001/REC-xml-c14n-20010315";
        public const string XmlSignatureMethod = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";
    }

    public readonly struct InnerXmlAttributes
    {
        public const string Type = "Type";
        public const string Id = "Id";
        public const string TargetMode = "TargetMode";
    }

    public readonly struct EntryFullNames
    {
        public const string RootRelationship = "_rels/.rels";
        public const string ContentTypes = "[Content_Types].xml";
        public const string AllSignaturesOrigin = "_xmlsignatures/origin.sigs";
        public const string SingleSignatureOrigin = "_xmlsignatures/_rels/origin.sigs.rels";
    }

    public readonly struct ContentTypes
    {
        public const string PackageDigitalSignature = "application/vnd.openxmlformats-package.digital-signature-xmlsignature+xml";
        public const string PackageDigitalSignatureOrigin = "application/vnd.openxmlformats-package.digital-signature-origin";
    }

    public readonly struct DataObjects
    {
        public const string RootNodeId = "idPackageSignature";

        public const string PackageReferenceUri = "#" + PackageId;
        public const string OfficeReferenceUri = "#" + OfficeId;
        public const string SignedPropertiesReferenceUri = "#" + SignedPropertiesId;

        public const string PackageId = "idPackageObject";
        public const string OfficeId = "idOfficeObject";
        public const string SignedPropertiesId = "idSignedProperties";
    }
}
