using System.Xml;

namespace ZeroDocSigner.Common.V2.Models;

public record OfficeSignatureObjectElements(
    XmlElement Package,
    XmlElement Office,
    XmlElement SignedProperties);
