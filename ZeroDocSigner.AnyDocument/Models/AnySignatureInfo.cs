using ZeroDocSigner.Shared.Interfaces;

namespace ZeroDocSigner.AnyDocument.Models;

public record AnySignatureInfo(
    string SignatureComments,
    string AddressPrimary,
    string AddressSecondary,
    string City,
    string StateOrProvince,
    string PostalCode,
    string CountryName,
    string SignerRole,
    string CommitmentType, // Description
    bool Force,
    byte[] Document) : ISignatureInfo;
