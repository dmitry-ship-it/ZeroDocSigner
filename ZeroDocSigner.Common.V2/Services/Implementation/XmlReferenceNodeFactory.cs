using ZeroDocSigner.Common.V2.Services.Abstractions;

namespace ZeroDocSigner.Common.V2.Services.Implementation;

public class XmlReferenceNodeFactory : IXmlReferenceNodeFactory
{
    private readonly IDictionary<string, string> overrideContentTypes;
    private readonly IDictionary<string, string> defaultContentTypes;
    private readonly IXmlReferenceNodeUriFactory uriFactory;

    public XmlReferenceNodeFactory(
        IDictionary<string, string> overrideContentTypes,
        IDictionary<string, string> defaultContentTypes)
    {
        this.overrideContentTypes = overrideContentTypes;
        this.defaultContentTypes = defaultContentTypes;

        uriFactory = new XmlReferenceNodeUriFactory();
    }

    public IXmlReferenceNode Create(string entryName, byte[] fileData)
    {
        var nodeUri = uriFactory.Create(entryName, overrideContentTypes, defaultContentTypes);

        const string relationshipFileExtension = ".rels";

        return entryName.EndsWith(relationshipFileExtension)
            ? new XmlRelationshipReferenceNode(fileData, nodeUri)
            : new XmlFileReferenceNode(fileData, nodeUri);
    }
}
