using Microsoft.Extensions.Configuration;
using System.DirectoryServices.AccountManagement;
using System.Runtime.Versioning;
using System.Security.Cryptography.X509Certificates;
using ZeroDocSigner.Api.Authentication.Exceptions;

namespace ZeroDocSigner.Api.Authentication.Services;

[SupportedOSPlatform("windows")]
public class ActiveDirectoryUserCertificateService : IUserCertificateService
{
    private readonly IUserCacheService userCacheService;
    private readonly IConfiguration configuration;

    public ActiveDirectoryUserCertificateService(IUserCacheService userCacheService, IConfiguration configuration)
    {
        this.userCacheService = userCacheService;
        this.configuration = configuration;
    }

    public async Task<X509Certificate2> GetCertificateAsync(string userName, CancellationToken cancellationToken = default)
    {
        var user = await userCacheService.LoadAsync(userName, cancellationToken)
            ?? throw new AuthenticationException($"User '{userName}' is not authenticated");

        using var domainContext = new PrincipalContext(
            ContextType.Domain,
            configuration.GetSection("ActiveDirectory")["Domain"]);

        if (!domainContext.ValidateCredentials(user.UserName, user.Password))
        {
            throw new AuthenticationException($"User '{userName}' is not authenticated");
        }

        using var userContext = UserPrincipal.FindByIdentity(domainContext, user.UserName);

        return userContext.Certificates.FirstOrDefault(certificate => certificate.HasPrivateKey)
            ?? throw new MissingCertificateException($"User '{userName}' does not have any certificates with private key");
    }
}
