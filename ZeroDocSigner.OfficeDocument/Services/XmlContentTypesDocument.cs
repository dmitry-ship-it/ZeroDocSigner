using System.Text;
using System.Xml;
using ZeroDocSigner.Shared.Constants;
using ZeroDocSigner.Shared.Extensions;

namespace ZeroDocSigner.OfficeDocument.Services;

internal class XmlContentTypesDocument
{
    private readonly XmlDocument document;

    public XmlContentTypesDocument(XmlDocument document)
    {
        this.document = document;
    }

    public void AppendOverride(string partName, string contentType)
    {
        var nodes = document.GetElementsByTagName(XmlContentTypesConstants.Nodes.Types);

        if (nodes.Count != 1)
        {
            throw new ArgumentException(ExceptionMessages.InvalidTypesNodeCount);
        }

        var targetNode = nodes[0]!;
        var overrideNode = document.CreateElement(XmlContentTypesConstants.Nodes.Override);

        overrideNode.SetAttribute(XmlContentTypesConstants.Attributes.PartName, partName);
        overrideNode.SetAttribute(XmlContentTypesConstants.Attributes.ContentType, contentType);

        targetNode.AppendChild(overrideNode);
    }

    public void AppendDefault(string extension, string contentType)
    {
        var nodes = document.GetElementsByTagName(XmlContentTypesConstants.Nodes.Default);

        if (nodes.AsEnumerable()
            .Any(node => node.Attributes?[XmlContentTypesConstants.Attributes.Extension]?.Value == extension))
        {
            return;
        }

        var lastDefaultNode = nodes[^1];

        var newElement = document.CreateElement(XmlContentTypesConstants.Nodes.Default);
        newElement.SetAttribute(XmlContentTypesConstants.Attributes.Extension, extension);
        newElement.SetAttribute(XmlContentTypesConstants.Attributes.ContentType, contentType);

        var root = document.GetElementsByTagName(XmlContentTypesConstants.Nodes.Types)[0]!;
        if (lastDefaultNode is null)
        {
            root.PrependChild(newElement);
        }
        else
        {
            root.InsertBefore(newElement, lastDefaultNode);
        }
    }

    public string GetOuterXml()
    {
        // only way to remove empty XML namespaces and self-closing node endings is to use string's Replace method
        // StringBuilder is used for memory optimization
        return new StringBuilder(document.OuterXml)
            .Replace(" xmlns=\"\"", string.Empty)
            .Replace(" />", "/>")
            .ToString();
    }
}
