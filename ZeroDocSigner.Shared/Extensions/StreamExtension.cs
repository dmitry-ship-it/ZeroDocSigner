using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace ZeroDocSigner.Shared.Extensions;

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

    public static string ReadAsString(this Stream stream)
    {
        stream.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }

    public static X509Certificate2 ReadAsX509Certificate2(this Stream stream, string? password = null)
    {
        stream.Seek(0, SeekOrigin.Begin);
        var buffer = new byte[stream.Length];
        stream.Read(buffer, 0, buffer.Length);

        return new X509Certificate2(buffer, password);
    }
}
