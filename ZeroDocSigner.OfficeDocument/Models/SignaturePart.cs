namespace ZeroDocSigner.OfficeDocument.Models;

internal record SignaturePart(
    string EntryName,
    byte[] Data);
