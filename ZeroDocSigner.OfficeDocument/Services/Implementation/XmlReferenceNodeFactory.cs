using ZeroDocSigner.OfficeDocument.Services.Abstractions;

namespace ZeroDocSigner.OfficeDocument.Services.Implementation;

internal class XmlReferenceNodeFactory : IXmlReferenceNodeFactory
{
    private readonly IDictionary<string, string> overrideContentTypes;
    private readonly IDictionary<string, string> defaultContentTypes;
    private readonly IDocumentReferenceNodeUriFactory uriFactory;

    public XmlReferenceNodeFactory(
        IDictionary<string, string> overrideContentTypes,
        IDictionary<string, string> defaultContentTypes)
    {
        this.overrideContentTypes = overrideContentTypes;
        this.defaultContentTypes = defaultContentTypes;

        uriFactory = new DocumentReferenceNodeUriFactory();
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
