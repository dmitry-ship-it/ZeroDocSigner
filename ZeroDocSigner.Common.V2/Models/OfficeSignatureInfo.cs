namespace ZeroDocSigner.Common.V2.Models;

public record OfficeSignatureInfo(
    string SignatureComments,
    string AddressPrimary,
    string AddressSecondary,
    string City,
    string StateOrProvince,
    string PostalCode,
    string CountryName,
    string SignerRole,
    string CommitmentType, // Description
    byte[] Document);
