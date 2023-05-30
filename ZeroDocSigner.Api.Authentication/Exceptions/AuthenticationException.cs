using System.Runtime.Serialization;

namespace ZeroDocSigner.Api.Authentication.Exceptions;

[Serializable]
public class AuthenticationException : Exception
{
    public AuthenticationException()
    {
    }

    public AuthenticationException(string? message)
        : base(message)
    {
    }

    public AuthenticationException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    protected AuthenticationException(SerializationInfo serializationInfo, StreamingContext streamingContext)
        : base(serializationInfo, streamingContext)
    {
    }
}
