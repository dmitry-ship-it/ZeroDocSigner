using System.IO.Compression;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using ZeroDocSigner.Common.Algorithm;
using ZeroDocSigner.Common.Extensions;
using ZeroDocSigner.Models;

namespace ZeroDocSigner.Common.Manager;

public class SignatureManager : ISigner, IVerifier, IUnsigner
{
    private readonly DocumentParser documentParser;
    private readonly DocumentBuilder documentBuilder;
    private readonly X509Certificate2 certificate;

    public SignatureManager(
        byte[] data,
        X509Certificate2 certificate,
        SignerModel? signerInfo = null)
    {
        var documentType = GetDocumentType(data);
        data = SetProperties(data, documentType, signerInfo);

        documentParser = new DocumentParser(data, documentType);
        documentBuilder = new DocumentBuilder(data, documentType);
        this.certificate = certificate;

        documentBuilder.SetContent(documentParser.FileContent);
    }

    public void AddSignature()
    {
        var signatures = documentParser.SignatureInfo;
        if (signatures is null)
        {
            documentBuilder.SetSignatureInfo(SignatureInfo.GetNewSignatureInfo(
                documentParser.FileContent, certificate));
            return;
        }

        var newSignature = Signature.Create(
            documentParser.FileContent, certificate);

        if (signatures.Signatures.Any(s => s.Sequence.SequenceEqual(newSignature.Sequence)))
        {
            throw new InvalidOperationException("File is already signed");
        }

        signatures.Signatures = signatures.Signatures.Add(newSignature);

        documentBuilder.SetSignatureInfo(signatures);
    }

    public void CreateSignature(bool force = false)
    {
        if (!force && documentParser.SignatureInfo is not null)
        {
            throw new InvalidOperationException(
                "This data already contains signature(s).");
        }

        var fileData = documentParser.FileContent;

        documentBuilder.SetSignatureInfo(SignatureInfo.GetNewSignatureInfo(
            fileData, certificate));
    }

    public bool Verify(Signature signature)
    {
        var verifier = SignatureAlgorithm.Create(certificate);

        var hash = Hashing.Compute(
            documentParser.FileContent,
            verifier.HashAlgorithm);

        return verifier.VerifySignature(hash, signature);
    }

    public bool Verify()
    {
        if (documentParser.SignatureInfo is null)
        {
            return false;
        }

        var signatures = documentParser.SignatureInfo!;
        return signatures.Signatures.Any(Verify);
    }

    public void RemoveSignature(Signature signature)
    {
        var signInfo = documentParser.SignatureInfo
            ?? throw new InvalidOperationException("No signatures to remove.");

        signInfo.Signatures = signInfo.Signatures.RemoveOne(
            el => el.Sequence.SequenceEqual(signature.Sequence));

        documentBuilder.SetSignatureInfo(signInfo);
    }

    public byte[] BuildFile()
    {
        return documentBuilder.Build();
    }

    private static DocumentType GetDocumentType(byte[] data)
    {
        try
        {
            using var memory = new MemoryStream(data);
            using var archive = new ZipArchive(memory);
        }
        catch (InvalidDataException)
        {
            return DocumentType.Binary;
        }

        return DocumentType.Archive;
    }

    public byte[] SetProperties(byte[] data, DocumentType documentType, SignerModel? signer)
    {
        if (documentType != DocumentType.Archive || signer is null)
        {
            return data;
        }

        using var memory = new MemoryStream();
        memory.Write(data);
        using (var archive = new ZipArchive(memory, ZipArchiveMode.Update))
        {
            var entry = archive.GetEntry("docProps/core.xml");

            foreach (var item in archive.Entries)
            {
                Console.WriteLine(item.Name);
            }

            if (entry is null)
            {
                return data;
            }

            var content = new StreamReader(entry!.Open());
            var text = content.ReadToEnd();

            var statusIndex = text.IndexOf("</cp:contentStatus>");
            if (statusIndex == -1)
            {
                text = text.Replace("</cp:coreProperties>",
                    $"<cp:contentStatus>Подписано: {signer.Signer}</cp:contentStatus></cp:coreProperties>");
            }
            else
            {
                statusIndex--;
                var sb = new StringBuilder(text);
                while (statusIndex != -1 && sb[statusIndex] != '>')
                {
                    sb.Remove(statusIndex, 1);
                    statusIndex--;
                }

                sb.Replace("<cp:contentStatus>", $"<cp:contentStatus>Подписано: {signer.Signer}");
                text = sb.ToString();
            }

            using (content)
            {
                // close stream reader
            }

            using var writer = new StreamWriter(entry.Open());
            writer.Write(text);
        }

        return memory.ToArray();
    }
}
