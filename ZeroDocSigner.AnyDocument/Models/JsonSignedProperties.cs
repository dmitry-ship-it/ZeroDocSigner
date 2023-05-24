namespace ZeroDocSigner.AnyDocument.Models;

public record JsonSignedProperties(
    string SignatureComments,
    string AddressPrimary,
    string AddressSecondary,
    string City,
    string StateOrProvince,
    string PostalCode,
    string CountryName,
    string SignerRole,
    string CommitmentType,
    DateTimeOffset Timestamp);
