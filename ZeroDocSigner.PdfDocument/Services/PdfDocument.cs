using iText.Bouncycastle.Crypto;
using iText.Bouncycastle.X509;
using iText.Kernel.Pdf;
using iText.Signatures;
using Org.BouncyCastle.Pkcs;
using ZeroDocSigner.PdfDocument.Models;
using ZeroDocSigner.Shared.Interfaces;

namespace ZeroDocSigner.PdfDocument.Services;

public class PdfDocument : IDocument<PdfSignatureInfo>
{
    private bool disposed;
    private readonly PdfReader reader;
    private readonly MemoryStream output;

    public PdfDocument(byte[] pdfDocument)
    {
        reader = new PdfReader(new MemoryStream(pdfDocument));
        output = new();

        ContainsSignatures = CheckForSignatures(pdfDocument);
    }

    public bool ContainsSignatures { get; private set; }

    public byte[] GetData()
    {
        return output.ToArray();
    }

    public void Sign(Stream certificate, string? certificatePassword, PdfSignatureInfo signatureInfo)
    {
        ArgumentException.ThrowIfNullOrEmpty(certificatePassword);

        // If signature is already exists we need to append new signature but not overwrite it
        var stampingProperties = ContainsSignatures
            ? new StampingProperties().UseAppendMode()
            : new StampingProperties();

        // Create a PDF signer object to apply the signature
        var signer = new PdfSigner(reader, output, stampingProperties);
        var (certificateInstance, privateKey) = ParseCertificateStream(certificate, certificatePassword);

        var chain = new[] { certificateInstance };
        var signature = new PrivateKeySignature(privateKey, "SHA-256");

        // create invisible rectangle on 1st page
        var appearance = signer.GetSignatureAppearance();
        FillAppearance(appearance, signatureInfo);

        // fill other params and sign document
        FillSigner(signer, signatureInfo.Approve);
        signer.SignDetached(signature, chain, null, null, null, 0, PdfSigner.CryptoStandard.CADES);

        ContainsSignatures = true;
    }

    private static (X509CertificateBC Certificate, PrivateKeyBC PrivateKey) ParseCertificateStream(Stream certificate, string password)
    {
        var pkcs12 = new Pkcs12StoreBuilder().Build();
        pkcs12.Load(certificate, password.ToCharArray());

        var keyAlias = pkcs12.Aliases.Cast<string>().FirstOrDefault(pkcs12.IsKeyEntry);

        var bouncyCertificate = pkcs12.GetCertificate(keyAlias).Certificate;
        var bouncyPrivateKey = pkcs12.GetKey(keyAlias).Key;

        return (new(bouncyCertificate), new(bouncyPrivateKey));
    }

    private static void FillAppearance(PdfSignatureAppearance appearance, PdfSignatureInfo signatureInfo)
    {
        appearance.SetPageNumber(1);
        appearance.SetReasonCaption("Причина подписания: ");
        appearance.SetReason(signatureInfo.Reason);
        appearance.SetLocationCaption("Адрес: ");
        appearance.SetLocation(signatureInfo.Location);
        appearance.SetContact(signatureInfo.Contact);
    }

    private static void FillSigner(PdfSigner signer, bool approve)
    {
        signer.SetSignDate(DateTime.Now);
        signer.SetCertificationLevel(approve ? PdfSigner.NOT_CERTIFIED : PdfSigner.CERTIFIED_NO_CHANGES_ALLOWED);
    }

    private static bool CheckForSignatures(byte[] pdfDocument)
    {
        using var pdfReader = new PdfReader(new MemoryStream(pdfDocument));
        using var pdf = new iText.Kernel.Pdf.PdfDocument(pdfReader);

        var signatureUtil = new SignatureUtil(pdf);
        var signatureNames = signatureUtil.GetSignatureNames();

        return signatureNames.Count > 0;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                reader.Close();
                (reader as IDisposable)?.Dispose();

                output.Close();
                output?.Dispose();
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
