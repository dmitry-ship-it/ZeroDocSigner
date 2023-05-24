using System.Xml;

namespace ZeroDocSigner.OfficeDocument.Models;

internal record OfficeSignatureObjectElements(
    XmlElement Package,
    XmlElement Office,
    XmlElement SignedProperties);
