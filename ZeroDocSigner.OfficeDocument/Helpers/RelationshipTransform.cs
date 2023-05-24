using System.Security.Cryptography.Xml;
using System.Xml;
using ZeroDocSigner.Shared.Constants;
using ZeroDocSigner.Shared.Extensions;

namespace ZeroDocSigner.OfficeDocument.Helpers;

public class RelationshipTransform : Transform
{
    private const string RelationshipTransformAlgorithm = "http://schemas.openxmlformats.org/package/2006/RelationshipTransform";
    private const string RelationshipsNamespace = "http://schemas.openxmlformats.org/package/2006/relationships";

    private const string TypeAttributeValueStart = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private const string TypeAttributeValueStartOffice = "http://schemas.microsoft.com/office/";

    private const string TargetModeAttributeValue = "Internal";

    private readonly XmlDsigExcC14NTransform innerTransform = new();

    public RelationshipTransform()
    {
        SourceIds = new List<string>();
        Algorithm = RelationshipTransformAlgorithm;
    }

    public List<string> SourceIds { get; }

    public override Type[] InputTypes => new[]
    {
        typeof(XmlDocument),
        typeof(XmlNodeList),
        typeof(Stream),
        typeof(byte[])
    };

    public override Type[] OutputTypes => new[]
    {
        typeof(XmlNodeList),
        typeof(Stream),
        typeof(byte[])
    };

    public override object GetOutput()
    {
        return innerTransform.GetOutput();
    }

    public override object GetOutput(Type type)
    {
        return innerTransform.GetOutput(type);
    }

    public override void LoadInnerXml(XmlNodeList nodeList)
    {
        if (nodeList is null || nodeList.Count == 0)
        {
            return;
        }

        // add all found ids from provided doc
        foreach (XmlNode node in nodeList)
        {
            if (!IsTypeAttributeValid(node))
            {
                continue;
            }

            var idAttribute = node.Attributes?[XmlSignatureConstants.InnerXmlAttributes.Id];

            if (idAttribute is not null)
            {
                SourceIds.Add(idAttribute.Value);
            }
        }
    }

    public override void LoadInput(object obj)
    {
        var inputDocument = GetDocumentByObject(obj);

        // get all "Relationship" nodes
        var relationships = inputDocument.SelectNodes(
                xpath: $"//{XmlSignatureConstants.NodePrefixes.R}:{XmlRelationshipConstants.Nodes.Relationship}",
                nsmgr: GetNameTable(inputDocument))
            ?? throw new ArgumentException(ExceptionMessages.InvalidDocumentStructure);

        // create "Relationships" node as document root
        var outputDocument = new XmlDocument();
        var root = outputDocument.CreateElement(
            XmlRelationshipConstants.Nodes.Relationships, RelationshipsNamespace);
        outputDocument.AppendChild(root);

        // sort "Relationship" nodes by "Id" attribute value
        foreach (var relationship in relationships
            .OrderByAttributeName(XmlSignatureConstants.InnerXmlAttributes.Id))
        {
            var idAttribute = relationship.Attributes?[XmlSignatureConstants.InnerXmlAttributes.Id];

            // if "Id" attribute is missing or contains value that we don't need - just skip this node
            if (idAttribute is null || !SourceIds.Contains(idAttribute.Value))
            {
                continue;
            }

            // set "TargetMode" attribute if it not exists
            var imported = outputDocument.ImportNode(relationship, true);
            if (imported.Attributes!.GetNamedItem(XmlSignatureConstants.InnerXmlAttributes.TargetMode) is null)
            {
                var targetModeAttribute = outputDocument.CreateAttribute(
                    XmlSignatureConstants.InnerXmlAttributes.TargetMode);
                targetModeAttribute.Value = TargetModeAttributeValue;

                imported.Attributes!.Append(targetModeAttribute);
            }

            root.AppendChild(imported);
        }

        // put back <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        outputDocument.AddXmlDeclaration("1.0", "UTF-8", "yes");

        innerTransform.LoadInput(outputDocument);
    }

    protected override XmlNodeList GetInnerXml()
    {
        var document = new XmlDocument();
        SetTransformElementWithMappedIds(document);

        var transform = new XmlDsigC14NTransform();
        transform.LoadInput(document);
        using var stream = (Stream)transform.GetOutput();
        var resultDocument = stream.ReadAsXmlDocument();

        return resultDocument.SelectSingleNode("//" + XmlSignatureConstants.Nodes.Transform)!.ChildNodes;
    }

    private static XmlNamespaceManager GetNameTable(XmlDocument document)
    {
        var namespaceManager = new XmlNamespaceManager(document.NameTable);
        namespaceManager.AddNamespace(XmlSignatureConstants.NodePrefixes.R, RelationshipsNamespace);

        return namespaceManager;
    }

    private void SetTransformElementWithMappedIds(XmlDocument document)
    {
        var root = document.CreateElement(XmlSignatureConstants.Nodes.Transform);
        root.SetAttribute(XmlSignatureConstants.Attributes.Algorithm, Algorithm);

        // add references with selected ids into "Transform" node inner XML
        foreach (var id in SourceIds)
        {
            var innerElement = document.CreateElement(
                XmlSignatureConstants.NodePrefixes.Mdssi,
                XmlSignatureConstants.Nodes.RelationshipReference,
                XmlSignatureConstants.Uris.PackageDigitalSignature);

            innerElement.SetAttribute(XmlSignatureConstants.Attributes.SourceId, id);
            root.AppendChild(innerElement);
        }

        document.AppendChild(root);
    }

    private static XmlDocument GetDocumentByObject(object obj)
    {
        var document = new XmlDocument()
        {
            PreserveWhitespace = true
        };

        switch (obj)
        {
            case XmlDocument xmlDoc:
                document = (XmlDocument)xmlDoc.Clone();
                break;

            case XmlNodeList nodeList:
                if (nodeList[0] is null)
                {
                    throw new ArgumentException(ExceptionMessages.DocumentIsEmpty);
                }

                document.LoadXml(nodeList[0]!.OuterXml);
                break;

            case Stream stream:
                document.Load(stream);
                break;

            case byte[] byteArray:
                using (var stream = new MemoryStream(byteArray))
                {
                    document.Load(stream);
                }

                break;

            default:
                throw new ArgumentException(ExceptionMessages.InvalidInputType);
        }

        return document;
    }

    private static bool IsTypeAttributeValid(XmlNode node)
    {
        var targetAttribute = node.Attributes?[XmlSignatureConstants.InnerXmlAttributes.Type];

        return targetAttribute is not null &&

              // type attribute should contain OpenXml's or MS Office's relationship uri
              (targetAttribute.Value.StartsWith(TypeAttributeValueStart) || targetAttribute.Value.StartsWith(TypeAttributeValueStartOffice)) &&
               targetAttribute.Value.Contains("relationships") &&

              // extended-properties and customXml should be excluded from signature
              !targetAttribute.Value.EndsWith("extended-properties") &&
              !targetAttribute.Value.EndsWith("customXml");
    }
}
