using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;
using ZeroDocSigner.OfficeDocument.Services.Abstractions;
using ZeroDocSigner.Shared.Constants;

namespace ZeroDocSigner.OfficeDocument.Services.Implementation;

internal class XmlFileReferenceNode : IXmlReferenceNode
{
    private readonly byte[] fileData;
    private readonly string nodeUri;

    public XmlFileReferenceNode(byte[] fileData, string nodeUri)
    {
        this.fileData = fileData;
        this.nodeUri = nodeUri;
    }

    public XmlElement GetXmlElement()
    {
        var reference = new Reference(nodeUri)
        {
            DigestMethod = DigestMethodsUris.Sha256,
            DigestValue = SHA256.HashData(fileData)
        };

        // All code above is just to remove XML namespace
        var result = new XmlDocument
        {
            PreserveWhitespace = true
        };

        result.LoadXml(reference.GetXml().OuterXml
            .Replace(" xmlns=\"http://www.w3.org/2000/09/xmldsig#\"", string.Empty));

        return (XmlElement)result.GetElementsByTagName(XmlSignatureConstants.Nodes.Reference)[0]!;
    }
}
