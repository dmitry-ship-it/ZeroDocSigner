using ZeroDocSigner.Common.V2.Services.Abstractions;

namespace ZeroDocSigner.Common.V2.Services.Implementation;

public class XmlReferenceNodeUriFactory : IXmlReferenceNodeUriFactory
{
    public string Create(string entryName,
        IDictionary<string, string> overrideContentTypes,
        IDictionary<string, string> defaultContentTypes)
    {
        return overrideContentTypes.TryGetValue(entryName, out var value)
            ? $"/{entryName}?ContentType={value}"
            : $"/{entryName}?ContentType={defaultContentTypes.First(contentType =>
                entryName.EndsWith(contentType.Key, StringComparison.InvariantCultureIgnoreCase)).Value}";
    }
}
