using System.Security.Cryptography.Xml;
using System.Xml;
using ZeroDocSigner.Common.V2.Constants;
using ZeroDocSigner.Common.V2.Extensions;

namespace ZeroDocSigner.Common.V2.Helpers;

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
        if (nodeList is null)
        {
            return;
        }

        foreach (XmlNode node in nodeList)
        {
            var targetAttribute = node.Attributes?[XmlSignatureConstants.InnerXmlAttributes.Type];
            if (targetAttribute?.Value.StartsWith(TypeAttributeValueStart) != true &&
                 (targetAttribute?.Value.StartsWith(TypeAttributeValueStartOffice) != true ||
                  targetAttribute?.Value.Contains("relationships") != true) ||
                targetAttribute.Value.EndsWith("extended-properties") ||
                targetAttribute.Value.EndsWith("customXml"))
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
        var inputDocument = new XmlDocument
        {
            PreserveWhitespace = true
        };

        switch (obj)
        {
            case XmlDocument xmlDoc:
                inputDocument = xmlDoc;
                break;
            case XmlNodeList nodeList:
                if (nodeList[0] is null) throw new ArgumentException(ExceptionMessages.DocumentIsEmpty);
                inputDocument.LoadXml(nodeList[0]!.OuterXml);
                break;
            case Stream stream:
                inputDocument.Load(stream);
                break;
            case byte[] byteArray:
                using (var stream = new MemoryStream(byteArray))
                {
                    inputDocument.Load(stream);
                }

                break;
            default:
                throw new ArgumentException(ExceptionMessages.InvalidInputType, nameof(obj));
        }

        var relationships = inputDocument.SelectNodes("//r:Relationship", GetNameTable(inputDocument))
            ?? throw new ArgumentException(ExceptionMessages.InvalidDocumentStructure);

        var outputDocument = new XmlDocument();
        var root = outputDocument.CreateElement(XmlRelationshipConstants.Nodes.Relationships, RelationshipsNamespace);
        outputDocument.AppendChild(root);

        foreach (var relationship in relationships.AsEnumerable()
            .OrderBy(node => node.Attributes?.GetNamedItem(XmlSignatureConstants.InnerXmlAttributes.Id)?.Value))
        {
            var idAttribute = relationship.Attributes?[XmlSignatureConstants.InnerXmlAttributes.Id];

            if (idAttribute is null || !SourceIds.Contains(idAttribute.Value))
            {
                continue;
            }

            var imported = outputDocument.ImportNode(relationship, true);
            if (imported.Attributes is not null &&
                imported.Attributes.GetNamedItem(XmlSignatureConstants.InnerXmlAttributes.TargetMode) is null)
            {
                var targetModeAttribute = outputDocument.CreateAttribute(
                    XmlSignatureConstants.InnerXmlAttributes.TargetMode);

                targetModeAttribute.Value = TargetModeAttributeValue;
                imported.Attributes.Append(targetModeAttribute);
            }

            root.AppendChild(imported);
        }

        // put back <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        var xmlDeclaration = outputDocument.CreateXmlDeclaration(
            version: "1.0",
            encoding: "UTF-8",
            standalone: "yes");

        outputDocument.PrependChild(xmlDeclaration);

        innerTransform.LoadInput(outputDocument);
    }

    protected override XmlNodeList GetInnerXml()
    {
        var document = new XmlDocument();

        var element = document.CreateElement(XmlSignatureConstants.Nodes.Transform);
        element.SetAttribute(XmlSignatureConstants.Attributes.Algorithm, Algorithm);

        foreach (var id in SourceIds)
        {
            var innerElement = document.CreateElement(
                XmlSignatureConstants.NodePrefixes.Mdssi,
                XmlSignatureConstants.Nodes.RelationshipReference,
                XmlSignatureConstants.Uris.PackageDigitalSignature);

            element.AppendChild(innerElement);

            var attribute = document.CreateAttribute(XmlSignatureConstants.Attributes.SourceId);
            attribute.Value = id;
            innerElement.Attributes.Append(attribute);
        }

        document.AppendChild(element);

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
}
