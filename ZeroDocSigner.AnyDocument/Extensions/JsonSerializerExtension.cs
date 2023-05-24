using System.Text.Encodings.Web;
using System.Text.Json;

namespace ZeroDocSigner.AnyDocument.Extensions;

internal static class JsonSerializerExtension
{
    public static string SerializeWithoutEscaping<T>(this T obj)
    {
        return JsonSerializer.Serialize(obj, new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }

    public static T? DeserializeWithoutEscaping<T>(this string json)
    {
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }
}
