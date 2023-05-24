using ZeroDocSigner.Shared.Interfaces;

namespace ZeroDocSigner.OpenDocument.Models;

public record OpenSignatureInfo(
    string CommitmentType,
    string SignerRole,
    string SignatureComments,
    string City,
    string StateOrProvince,
    string PostalCode,
    string CountryName, // Description
    byte[] Document) : ISignatureInfo;
