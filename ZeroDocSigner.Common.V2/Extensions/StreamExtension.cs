using System.Xml;

namespace ZeroDocSigner.Common.V2.Extensions;

public static class StreamExtension
{
    public static XmlDocument ReadAsXmlDocument(this Stream stream)
    {
        stream.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(stream);

        var document = new XmlDocument()
        {
            PreserveWhitespace = true
        };

        document.LoadXml(reader.ReadToEnd());

        return document;
    }
}
