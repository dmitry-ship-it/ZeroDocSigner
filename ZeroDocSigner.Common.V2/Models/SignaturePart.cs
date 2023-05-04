namespace ZeroDocSigner.Common.V2.Models;

public record SignaturePart(
    string EntryName,
    byte[] Data);
