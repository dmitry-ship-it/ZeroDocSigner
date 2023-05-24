namespace ZeroDocSigner.OfficeDocument.Services.Abstractions;

internal interface IDocumentReferenceNodeUriFactory
{
    string Create(string entryName,
        IDictionary<string, string> overrideContentTypes,
        IDictionary<string, string> defaultContentTypes);
}
