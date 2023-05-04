﻿namespace ZeroDocSigner.Common.V2.Constants;

public static class SupportedOfficeDocumentTypes
{
    public static readonly (string extension, string InnerEntryRoot)[] MetaValues = new[]
    {
        ("docx", "word"),
        ("xlsx", "xl"),
        ("pptx", "ppt")
    };
}
