using System.IO.Compression;

namespace ZeroDocSigner.Shared.Extensions;

public static class ZipArchiveExtension
{
    public static Dictionary<string, Stream> GetOpenedEntriesDictionary(this ZipArchive zip)
    {
        var result = new Dictionary<string, Stream>(zip.Entries.Count);

        foreach (var entry in zip.Entries)
        {
            result[entry.FullName] = entry.Open();
        }

        return result;
    }
}
