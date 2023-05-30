using System.Security.Cryptography.X509Certificates;

namespace ZeroDocSigner.Api.Authentication.Services;

public interface IUserCertificateService
{
    Task<X509Certificate2> GetCertificateAsync(string userName, CancellationToken cancellationToken = default);
}
