using System.Xml;

namespace ZeroDocSigner.Common.V2.Extensions;

public static class XmlNodeListExtension
{
    public static IEnumerable<XmlNode> AsEnumerable(this XmlNodeList nodeList)
    {
        for (var i = 0; i < nodeList.Count; i++)
        {
            yield return nodeList[i]!;
        }
    }
}
