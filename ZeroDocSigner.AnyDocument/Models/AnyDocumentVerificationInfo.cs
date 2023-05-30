namespace ZeroDocSigner.AnyDocument.Models;

public record AnyDocumentVerificationInfo(
    bool IsValid,
    string SignatureComments,
    string AddressPrimary,
    string AddressSecondary,
    string City,
    string StateOrProvince,
    string PostalCode,
    string CountryName,
    string SignerRole,
    string CommitmentType); // Description
