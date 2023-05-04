using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ZeroDocSigner.Common.Algorithm;

public class HashAlgorithmNameConverter : JsonConverter<HashAlgorithmName>
{
    public override HashAlgorithmName Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new HashAlgorithmName(reader.GetString());
    }

    public override void Write(Utf8JsonWriter writer, HashAlgorithmName value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Name);
    }
}