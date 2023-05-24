using ZeroDocSigner.OfficeDocument.Services.Abstractions;

namespace ZeroDocSigner.OfficeDocument.Services.Implementation;

internal class DocumentReferenceNodeUriFactory : IDocumentReferenceNodeUriFactory
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
