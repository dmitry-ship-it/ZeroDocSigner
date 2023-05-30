using ZeroDocSigner.Api.Authentication.Models;

namespace ZeroDocSigner.Api.Authentication.Services;

public interface IAuthenticationService
{
    Task<JWToken> LoginAsync(UserModel user, CancellationToken cancellationToken = default);
}
