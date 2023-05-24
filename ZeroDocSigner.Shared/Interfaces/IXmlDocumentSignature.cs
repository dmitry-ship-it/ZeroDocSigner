using System.Xml;

namespace ZeroDocSigner.Shared.Interfaces;

public interface IXmlDocumentSignature
{
    XmlDocument CreateSignatureDocument();
}
