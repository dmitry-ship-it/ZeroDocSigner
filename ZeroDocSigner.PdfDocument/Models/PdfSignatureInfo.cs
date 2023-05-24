using ZeroDocSigner.Shared.Interfaces;

namespace ZeroDocSigner.PdfDocument.Models;

public record PdfSignatureInfo(
    string Reason,
    string Location,
    string Contact,
    bool Approve,
    byte[] Document) : ISignatureInfo;
