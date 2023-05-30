using System.Runtime.Serialization;

namespace ZeroDocSigner.Api.Authentication.Exceptions;

[Serializable]
public class MissingCertificateException : Exception
{
    public MissingCertificateException()
    {
    }

    public MissingCertificateException(string? message)
        : base(message)
    {
    }

    public MissingCertificateException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected MissingCertificateException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
