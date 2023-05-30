using ZeroDocSigner.Api.Authentication.Models;

namespace ZeroDocSigner.Api.Authentication.Services;

public interface IUserCacheService
{
    Task SaveAsync(UserModel user, CancellationToken cancellationToken = default);

    Task<UserModel?> LoadAsync(string userName, CancellationToken cancellationToken = default);
}
