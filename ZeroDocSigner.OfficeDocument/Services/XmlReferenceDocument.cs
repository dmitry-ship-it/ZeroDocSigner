using System.Text;
using System.Xml;
using ZeroDocSigner.Shared.Constants;

namespace ZeroDocSigner.OfficeDocument.Services;

internal class XmlReferenceDocument
{
    private const string Template = """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"></Relationships>
        """;

    private int nextReferenceId;
    private readonly XmlDocument document;

    public XmlReferenceDocument(XmlDocument document)
    {
        this.document = document;
        nextReferenceId = GetNextReferenceId();
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

        signatureNode.SetAttribute(XmlRelationshipConstants.Attributes.Id, $"rId{nextReferenceId}");
        signatureNode.SetAttribute(XmlRelationshipConstants.Attributes.Type, type);
        signatureNode.SetAttribute(XmlRelationshipConstants.Attributes.Target, target);

        nextReferenceId++;
        targetNode.AppendChild(signatureNode);
    }

    public static XmlReferenceDocument CreateEmpty()
    {
        var result = new XmlDocument()
        {
            PreserveWhitespace = true
        };

        result.LoadXml(Template);

        return new XmlReferenceDocument(result)
        {
            nextReferenceId = 1
        };
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

    private int GetNextReferenceId()
    {
        var maxId = 0;

        foreach (XmlElement node in document.GetElementsByTagName(XmlRelationshipConstants.Nodes.Relationship))
        {
            // get "Id" attribute and parse its number part (default value is 0 if attribute is missing):
            // "rId4" => "4" => int.Parse("4") => 4
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
