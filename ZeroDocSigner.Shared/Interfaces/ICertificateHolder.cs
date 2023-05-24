namespace ZeroDocSigner.Shared.Interfaces;

/// <summary>
/// This interface could be used for DI with <see cref="Services.CertificateHolder"/>.
/// </summary>
public interface ICertificateHolder : IDisposable
{
    Stream CertificateStream { get; }

    string Password { get; }
}
