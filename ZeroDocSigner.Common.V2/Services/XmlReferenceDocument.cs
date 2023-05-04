using System.Xml;
using ZeroDocSigner.Common.V2.Constants;

namespace ZeroDocSigner.Common.V2.Services;

public class XmlReferenceDocument
{
    private const string Template = """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"></Relationships>
        """;

    private readonly XmlDocument document;

    public XmlReferenceDocument(XmlDocument document)
    {
        this.document = document;
    }

    public void AppendReference(string type, string target)
    {
        var nodes = document.GetElementsByTagName(XmlRelationshipConstants.Nodes.Relationships);

        if (nodes.Count != 1)
        {
            throw new ArgumentException(ExceptionMessages.InvalidRelationshipsNodeCount);
        }

        var targetNode = nodes[0]!;
        var signatureNode = document.CreateElement(
            XmlRelationshipConstants.Nodes.Relationship,
            XmlRelationshipConstants.SchemeUri);

        signatureNode.SetAttribute(XmlRelationshipConstants.Attributes.Id, $"rId{GetNextReferenceId()}");
        signatureNode.SetAttribute(XmlRelationshipConstants.Attributes.Type, type);
        signatureNode.SetAttribute(XmlRelationshipConstants.Attributes.Target, target);

        targetNode.AppendChild(signatureNode);
    }

    public static XmlReferenceDocument CreateEmpty()
    {
        var result = new XmlDocument()
        {
            PreserveWhitespace = true
        };

        result.LoadXml(Template);

        return new XmlReferenceDocument(result);
    }

    public string GetOuterXml()
    {
        return document.OuterXml
            .Replace(" xmlns=\"\"", string.Empty)
            .Replace(" />", "/>");
    }

    private int GetNextReferenceId()
    {
        var maxId = 0;

        foreach (XmlElement node in document.GetElementsByTagName(XmlRelationshipConstants.Nodes.Relationship))
        {
            var currentNodeId = int.Parse(node.Attributes
                .GetNamedItem(XmlRelationshipConstants.Attributes.Id)?
                .Value?
                .Replace("rId", string.Empty) ?? "0");

            if (currentNodeId > maxId)
            {
                maxId = currentNodeId;
            }
        }

        return maxId + 1;
    }
}
