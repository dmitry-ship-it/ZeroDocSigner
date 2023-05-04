namespace ZeroDocSigner.Common.V2.Services.Abstractions;

public interface IXmlReferenceNodeUriFactory
{
    string Create(string entryName,
        IDictionary<string, string> overrideContentTypes,
        IDictionary<string, string> defaultContentTypes);
}
