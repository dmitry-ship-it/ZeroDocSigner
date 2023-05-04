namespace ZeroDocSigner.Common.V2.Extensions;

public static class StringExtension
{
    public static string Format(this string s, params object[] arguments)
    {
        ArgumentException.ThrowIfNullOrEmpty(s);

        var result = s;

        for (var i = 0; i < arguments.Length; i++)
        {
            result = result.Replace("{" + i + "}", arguments[i].ToString());
        }

        return result;
    }
}
