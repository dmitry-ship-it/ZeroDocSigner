namespace ZeroDocSigner.OfficeDocument.Services.Abstractions;

internal interface IXmlReferenceNodeFactory
{
    public IXmlReferenceNode Create(string entryName, byte[] fileData);
}
