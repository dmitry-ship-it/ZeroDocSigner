namespace ZeroDocSigner.Common.V2.Services.Abstractions;

public interface IXmlReferenceNodeFactory
{
    public IXmlReferenceNode Create(string entryName, byte[] fileData);
}
