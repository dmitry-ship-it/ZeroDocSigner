using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using ZeroDocSigner.OfficeDocument.Helpers;
using ZeroDocSigner.OfficeDocument.Services.Abstractions;
using ZeroDocSigner.Shared.Constants;

namespace ZeroDocSigner.OfficeDocument.Services.Implementation;

internal class XmlRelationshipReferenceNode : IXmlReferenceNode
{
    private readonly XmlDocument xmlDocument;
    private readonly string nodeUri;

    public XmlRelationshipReferenceNode(byte[] fileData, string nodeUri)
    {
        var document = new XmlDocument();
        document.LoadXml(Encoding.UTF8.GetString(fileData));
        xmlDocument = document;

        this.nodeUri = nodeUri;
    }

    public XmlElement GetXmlElement()
    {
        var reference = new Reference(nodeUri);

        var relationshipTransform = new RelationshipTransform();
        relationshipTransform.LoadInnerXml(xmlDocument.SelectNodes("//*")!);
        relationshipTransform.LoadInput(xmlDocument);

        reference.AddTransform(relationshipTransform);
        reference.AddTransform(new XmlDsigC14NTransform());
        reference.DigestMethod = DigestMethodsUris.Sha256;

        using var outStream = (Stream)relationshipTransform.GetOutput();
        reference.DigestValue = SHA256.HashData(outStream);

        // Just to remove namespace
        var result = new XmlDocument
        {
            PreserveWhitespace = true
        };

        result.LoadXml(reference.GetXml().OuterXml
            .Replace(" xmlns=\"http://www.w3.org/2000/09/xmldsig#\"", string.Empty));

        return (XmlElement)result.GetElementsByTagName(XmlSignatureConstants.Nodes.Reference)[0]!;
    }
}
