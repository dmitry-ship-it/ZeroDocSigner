namespace ZeroDocSigner.AnyDocument.Models;

[Serializable]
public record JsonSignature(
    string SignedProperties, // Base64
    JsonHashHolder DocumentHash,
    JsonHashHolder PropertiesHash,
    string Signature, // Base64

    string Certificate); // Base64
