using System.IO.Compression;
using System.Text;
using System.Xml;
using ZeroDocSigner.OpenDocument.Models;
using ZeroDocSigner.Shared.Constants;
using ZeroDocSigner.Shared.Extensions;
using ZeroDocSigner.Shared.Interfaces;

namespace ZeroDocSigner.OpenDocument.Services;

public class OpenDocument : IDocument<OpenSignatureInfo>
{
    private bool disposed;

    private ZipArchive zip;
    private MemoryStream innerStream;
    private Dictionary<string, Stream> entries;

    public OpenDocument(byte[] openDocument)
    {
        var memoryStream = new MemoryStream();
        memoryStream.Write(openDocument, 0, openDocument.Length);
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

        entries = zip.GetOpenedEntriesDictionary();
        innerStream = memoryStream;

        ContainsSignatures = CheckForSignatures();
    }

    public bool ContainsSignatures { get; private set; }

    public void Sign(Stream certificate, string? certificatePassword, OpenSignatureInfo signatureInfo)
    {
        using var certificateInstance = certificate.ReadAsX509Certificate2(certificatePassword);
        var signature = new OpenDocumentSignature(certificateInstance, signatureInfo, entries);

        if (ContainsSignatures)
        {
            AppendSignature(signature);
        }
        else
        {
            CreateSignature(signature);
        }

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

        // reopen archive to continue work with document
        innerStream = new MemoryStream();
        innerStream.Write(data);
        zip = new ZipArchive(innerStream, ZipArchiveMode.Update);
        entries = zip.GetOpenedEntriesDictionary();

        return data;
    }

    private bool CheckForSignatures()
    {
        return entries.ContainsKey(XmlSignatureConstants.EntryFullNames.OpenDocumentSignaturesEntry);
    }

    private void CreateSignature(OpenDocumentSignature signature)
    {
        var document = new XmlDocument()
        {
            PreserveWhitespace = true
        };

        var root = document.CreateElement(
            XmlSignatureConstants.Nodes.DocumentSignatures,
            XmlSignatureConstants.Namespaces.OpenDocumentSignature);

        var signatureElement = document.ImportNode(signature.CreateSignatureDocument().DocumentElement!, true);

        root.AppendChild(signatureElement);
        document.AppendChild(root);
        document.AddXmlDeclaration("1.0", "UTF-8");

        WriteToSignaturesEntry(document);
    }

    private void AppendSignature(OpenDocumentSignature signature)
    {
        var document = entries[XmlSignatureConstants.EntryFullNames.OpenDocumentSignaturesEntry].ReadAsXmlDocument();
        var signatureElement = document.ImportNode(signature.CreateSignatureDocument().DocumentElement!, true);
        document.GetElementsByTagName(XmlSignatureConstants.Nodes.DocumentSignatures)[0]!.AppendChild(signatureElement);

        WriteToSignaturesEntry(document);
    }

    private void WriteToSignaturesEntry(XmlDocument document)
    {
        var data = Encoding.UTF8.GetBytes(document.OuterXml);
        var entry = entries.GetValueOrDefault(XmlSignatureConstants.EntryFullNames.OpenDocumentSignaturesEntry)
            ?? zip.CreateEntry(XmlSignatureConstants.EntryFullNames.OpenDocumentSignaturesEntry).Open();

        entry.Seek(0, SeekOrigin.Begin);
        entry.Write(data);
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
