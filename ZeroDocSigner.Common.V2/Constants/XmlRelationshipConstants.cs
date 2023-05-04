namespace ZeroDocSigner.Common.V2.Constants;

public static class XmlRelationshipConstants
{
    public const string SchemeUri = "http://schemas.openxmlformats.org/package/2006/relationships";

    public readonly struct Nodes
    {
        public const string Relationships = "Relationships";
        public const string Relationship = "Relationship";
    }

    public readonly struct Attributes
    {
        public const string Id = "Id";
        public const string Type = "Type";
        public const string Target = "Target";
    }
}
