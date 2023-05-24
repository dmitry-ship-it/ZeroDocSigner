using System.Numerics;
using System.Security.Cryptography.X509Certificates;

namespace ZeroDocSigner.Shared.Extensions;

public static class X509Certificate2Extension
{
    public static string GetFormattedSerialNumber(this X509Certificate2 certificate)
    {
        return new BigInteger(certificate.GetSerialNumber()).ToString();
    }
}
