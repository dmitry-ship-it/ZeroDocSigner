using System.Xml;

namespace ZeroDocSigner.Shared.Extensions;

public static class XmlDocumentExtension
{
    public static void AddXmlDeclaration(this XmlDocument document, string version, string? encoding, string? standalone = null)
    {
        var declaration = document.CreateXmlDeclaration(version, encoding, standalone);
        document.PrependChild(declaration);
    }
}
