using Microsoft.Extensions.Caching.Distributed;
using ZeroDocSigner.Api.Authentication.Models;

namespace ZeroDocSigner.Api.Authentication.Services;

public class UserCacheService : IUserCacheService
{
    private readonly IDistributedCache cache;

    public UserCacheService(IDistributedCache cache)
    {
        this.cache = cache;
    }

    public async Task SaveAsync(UserModel user, CancellationToken cancellationToken = default)
    {
        await cache.SetStringAsync(
            user.UserName,
            user.Password,
            new() { AbsoluteExpiration = DateTimeOffset.Now.AddHours(1)},
            cancellationToken);
    }

    public async Task<UserModel?> LoadAsync(string userName, CancellationToken cancellationToken = default)
    {
        var loaded = await cache.GetStringAsync(userName, cancellationToken);

        if (loaded is null)
        {
            return null;
        }

        return new(userName, loaded);
    }
}
