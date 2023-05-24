using System.Xml;

namespace ZeroDocSigner.OfficeDocument.Services.Abstractions;

internal interface IXmlReferenceNode
{
    XmlElement GetXmlElement();
}
