﻿using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ZeroDocSigner.Common.Algorithm;

[Serializable]
public sealed class SignatureInfo
{
    [JsonIgnore]
    public static byte[] StartSequence => Encoding.Default.GetBytes("*{SignStart}*");

    public Signature[] Signatures { get; set; } = Array.Empty<Signature>();

    public string Serialize()
    {
        var options = new JsonSerializerOptions()
        {
            WriteIndented = true
        };

        return JsonSerializer.Serialize(this, options);
    }

    public static SignatureInfo GetNewSignatureInfo(
        byte[] data,
        X509Certificate2 certificate)
    {
        return new()
        {
            Signatures = new[]
            {
                Signature.Create(data, certificate)
            }
        };
    }

    public override string ToString()
    {
        return Encoding.Default.GetString(StartSequence)
            + Environment.NewLine
            + Serialize();
    }
}
