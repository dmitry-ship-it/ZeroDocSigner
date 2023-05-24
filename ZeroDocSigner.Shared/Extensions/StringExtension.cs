using System.Text;
using System.Xml;

namespace ZeroDocSigner.Shared.Extensions;

public static class StringExtension
{
    public static string Format(this string s, params object[] arguments)
    {
        ArgumentException.ThrowIfNullOrEmpty(s);

        var result = new StringBuilder(s);

        for (var i = 0; i < arguments.Length; i++)
        {
            result = result.Replace("{" + i + "}", arguments[i].ToString());
        }

        return result.ToString();
    }

    public static XmlDocument ToXmlDocument(this string s, bool preserveWhitespace = true)
    {
        var document = new XmlDocument()
        {
            PreserveWhitespace = preserveWhitespace
        };

        document.LoadXml(s);

        return document;
    }
}
