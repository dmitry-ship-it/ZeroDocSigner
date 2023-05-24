using System.Xml;

namespace ZeroDocSigner.Shared.Extensions;

public static class XmlNodeListExtension
{
    public static IEnumerable<XmlNode> AsEnumerable(this XmlNodeList nodeList)
    {
        for (var i = 0; i < nodeList.Count; i++)
        {
            yield return nodeList[i]!;
        }
    }

    public static IEnumerable<XmlNode> OrderByAttributeName(this XmlNodeList nodeList, string attributeName)
    {
        return nodeList.AsEnumerable().OrderBy(node => node.Attributes?.GetNamedItem(attributeName)?.Value);
    }
}
