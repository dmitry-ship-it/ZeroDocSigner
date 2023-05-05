using System.IO.Compression;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using ZeroDocSigner.Common.V2.Constants;
using ZeroDocSigner.Common.V2.Extensions;
using ZeroDocSigner.Common.V2.Models;

namespace ZeroDocSigner.Common.V2.Services;

public class OfficeDocument : IDisposable
{
    private const string SignatureEntryBegin = "_xmlsignatures/sig";

    private bool disposed;
    private ZipArchive zip;
    private MemoryStream innerStream;
    private Dictionary<string, Stream> entries;

    public OfficeDocument(byte[] wordDocument)
    {
        var memoryStream = new MemoryStream();
        memoryStream.Write(wordDocument, 0, wordDocument.Length);
        memoryStream.Seek(0, SeekOrigin.Begin);

        try
        {
            zip = new ZipArchive(memoryStream, ZipArchiveMode.Update);
        }
        catch
        {
            zip?.Dispose();
            memoryStream?.Dispose();

            throw;
        }

        entries = FillEntries();
        innerStream = memoryStream;

        ValidateAndThrow();

        ContainsSignatures = CheckForSignatures();
    }

    public bool ContainsSignatures { get; private set; }

    public void Sign(X509Certificate2 certificate, OfficeSignatureInfo signatureInfo)
    {
        var signatureParts = new List<SignaturePart>();
        const string rootRelationshipPath = XmlSignatureConstants.EntryFullNames.RootRelationship;

        var firstEntry = entries[rootRelationshipPath];

        var data = new byte[firstEntry.Length];
        firstEntry.Read(data, 0, data.Length);

        signatureParts.Add(new(entries.Keys.First(key => key == rootRelationshipPath), data));

        foreach (var entry in entries
            .Where(e => SupportedOfficeDocumentTypes.MetaValues
                .Any(value => e.Key.StartsWith(value.InnerEntryRoot)))
            .OrderBy(e => e.Key[e.Key.IndexOf('/')..]))
        {
            var entryStream = entry.Value;

            data = new byte[entryStream.Length];
            entryStream.Read(data, 0, data.Length);

            signatureParts.Add(new(entry.Key, data));
        }

        var (Override, Default) = GetContentTypeDictionaries();
        var signature = new OfficeDocumentSignature(
            certificate,
            signatureInfo,
            signatureParts,
            Override,
            Default);

        var signatureDocument = signature.CreateSignatureDocument();

        // modify existing files
        AddSignatureReferenceToRoot();
        var currentSignatureNumber = AddSignatureToContentTypes();
        AddSignatureToOriginReferences(currentSignatureNumber);
        CreateSignaturesOrigin();
        AppendSignature(currentSignatureNumber, signatureDocument);

        FlushAllStreams();
        ContainsSignatures = true;
    }

    public byte[] GetData()
    {
        using (zip)
        {
            // close archive to save changes
        }

        var data = innerStream.ToArray();

        innerStream = new MemoryStream();
        innerStream.Write(data);
        zip = new ZipArchive(innerStream, ZipArchiveMode.Update);
        entries = FillEntries();

        return data;
    }

    private Dictionary<string, Stream> FillEntries()
    {
        var result = new Dictionary<string, Stream>();

        foreach (var entry in zip.Entries)
        {
            result[entry.FullName] = entry.Open();
        }

        return result;
    }

    private void ValidateAndThrow()
    {
        if (entries.Any(e => !SupportedOfficeDocumentTypes.MetaValues
            .Any(value => !e.Key.StartsWith(value.InnerEntryRoot))))
        {
            throw new ArgumentException(ExceptionMessages.InvalidDocumentType);
        }
    }

    private bool CheckForSignatures()
    {
        return entries.Any(entry => entry.Key.Contains(SignatureEntryBegin));
    }

    private (Dictionary<string, string> Override, Dictionary<string, string> Default) GetContentTypeDictionaries()
    {
        var contentTypesDocument = entries[XmlSignatureConstants.EntryFullNames.ContentTypes]
            .ReadAsXmlDocument();

        var root = (XmlElement)contentTypesDocument
            .GetElementsByTagName(XmlContentTypesConstants.Nodes.Types)[0]!;

        return (GetOverrideContentTypeDictionary(root), GetDefaultContentTypeDictionary(root));
    }

    private static Dictionary<string, string> GetOverrideContentTypeDictionary(XmlElement root)
    {
        var result = new Dictionary<string, string>();

        foreach (XmlElement node in root.GetElementsByTagName(XmlContentTypesConstants.Nodes.Override))
        {
            var partNameAttributeValue = node.GetAttribute(XmlContentTypesConstants.Attributes.PartName);

            // remove '/' at the beginning of the string
            result[partNameAttributeValue.Remove(0, 1)]
                = node.GetAttribute(XmlContentTypesConstants.Attributes.ContentType);
        }

        return result;
    }

    private static Dictionary<string, string> GetDefaultContentTypeDictionary(XmlElement root)
    {
        var result = new Dictionary<string, string>();

        foreach (XmlElement node in root.GetElementsByTagName(XmlContentTypesConstants.Nodes.Default))
        {
            var extensionAttributeValue = node.GetAttribute(XmlContentTypesConstants.Attributes.Extension);

            result[extensionAttributeValue] = node.GetAttribute(XmlContentTypesConstants.Attributes.ContentType);
        }

        return result;
    }

    private void AddSignatureReferenceToRoot()
    {
        var entry = entries[XmlSignatureConstants.EntryFullNames.RootRelationship];
        var xmlDocument = entry.ReadAsXmlDocument();

        var referenceNodes = xmlDocument.GetElementsByTagName(XmlRelationshipConstants.Nodes.Relationship);

        if (referenceNodes.AsEnumerable().Any(node => node.Attributes?[
            XmlRelationshipConstants.Attributes.Target]?.Value == XmlSignatureConstants.EntryFullNames.AllSignaturesOrigin))
        {
            return;
        }

        var referenceDocument = new XmlReferenceDocument(xmlDocument);

        referenceDocument.AppendReference(
            XmlSignatureConstants.Uris.DigitalSignatureOrigin,
            XmlSignatureConstants.EntryFullNames.AllSignaturesOrigin);

        var newContent = Encoding.UTF8.GetBytes(referenceDocument.GetOuterXml());

        entry.Seek(0, SeekOrigin.Begin);
        entry.Write(newContent);
    }

    private int AddSignatureToContentTypes()
    {
        const string AttributeSignaturePathBegin = $"/{SignatureEntryBegin}";
        const string DigitalSignatureOriginFileExtension = "sigs";

        var entry = entries[XmlSignatureConstants.EntryFullNames.ContentTypes];
        var xmlDocument = entry.ReadAsXmlDocument();

        var overrideNodes = xmlDocument.GetElementsByTagName(XmlContentTypesConstants.Nodes.Override);
        var signatures = overrideNodes
            .AsEnumerable()
            .Select(node => node.Attributes?[XmlContentTypesConstants.Attributes.PartName]?.Value)
            .Where(attribute => attribute?.StartsWith(AttributeSignaturePathBegin) == true);

        var lastSignature = signatures.Any()
            ? signatures.Max(value => int.Parse(value?
                .Replace(AttributeSignaturePathBegin, string.Empty)
                .Replace(".xml", string.Empty)
                    ?? "0"))
            : 0;

        var contentTypesDocument = new XmlContentTypesDocument(xmlDocument);
        contentTypesDocument.AppendOverride(
            $"{AttributeSignaturePathBegin}{lastSignature + 1}.xml",
            XmlSignatureConstants.ContentTypes.PackageDigitalSignature);
        contentTypesDocument.AppendDefault(
            DigitalSignatureOriginFileExtension,
            XmlSignatureConstants.ContentTypes.PackageDigitalSignatureOrigin);

        var newContent = Encoding.UTF8.GetBytes(contentTypesDocument.GetOuterXml());

        entry.Seek(0, SeekOrigin.Begin);
        entry.Write(newContent);

        return lastSignature + 1;
    }

    private void AddSignatureToOriginReferences(int currentSignatureNumber)
    {
        const string originEntryName = XmlSignatureConstants.EntryFullNames.SingleSignatureOrigin;

        XmlReferenceDocument referenceDocument;
        Stream entryStream;

        if (!entries.ContainsKey(originEntryName))
        {
            entryStream = zip.CreateEntry(originEntryName).Open();
            entries[originEntryName] = entryStream;
            referenceDocument = XmlReferenceDocument.CreateEmpty();
        }
        else
        {
            entryStream = entries[originEntryName];
            referenceDocument = new XmlReferenceDocument(entryStream.ReadAsXmlDocument());
        }

        referenceDocument.AppendReference(
            XmlSignatureConstants.Uris.PackageDigitalSignatureRelationship,
            $"sig{currentSignatureNumber}.xml");

        var newContent = Encoding.UTF8.GetBytes(referenceDocument.GetOuterXml());

        entryStream.Seek(0, SeekOrigin.Begin);
        entryStream.Write(newContent);
    }

    private void CreateSignaturesOrigin()
    {
        const string entryName = XmlSignatureConstants.EntryFullNames.AllSignaturesOrigin;

        if (entries.ContainsKey(entryName))
        {
            return;
        }

        var entryStream = zip.CreateEntry(entryName).Open();
        entryStream.Write(Array.Empty<byte>());

        entries[entryName] = entryStream;
    }

    private void AppendSignature(int currentSignatureNumber, XmlDocument signatureDocument)
    {
        var entryName = $"_xmlsignatures/sig{currentSignatureNumber}.xml";

        var entry = zip.CreateEntry(entryName);
        var entryStream = entry.Open();

        var data = Encoding.UTF8.GetBytes(signatureDocument.OuterXml);
        entryStream.Write(data);

        entries[entryName] = entryStream;
    }

    private void FlushAllStreams()
    {
        foreach (var stream in entries.Values)
        {
            stream.Flush();
        }

        innerStream.Flush();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                zip?.Dispose();
                innerStream?.Dispose();
            }

            disposed = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
