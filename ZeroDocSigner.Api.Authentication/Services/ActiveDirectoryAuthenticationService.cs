using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.DirectoryServices.AccountManagement;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.Versioning;
using System.Security.Claims;
using System.Text;
using ZeroDocSigner.Api.Authentication.Exceptions;
using ZeroDocSigner.Api.Authentication.Models;

namespace ZeroDocSigner.Api.Authentication.Services;

[SupportedOSPlatform("windows")]
public class ActiveDirectoryAuthenticationService : IAuthenticationService
{
    private readonly PrincipalContext domainContext;
    private readonly IUserCacheService userCacheService;
    private readonly IConfiguration configuration;

    public ActiveDirectoryAuthenticationService(IConfiguration configuration, IUserCacheService userCacheService)
    {
        try
        {
            domainContext = new(ContextType.Domain, configuration.GetSection("ActiveDirectory")["Domain"]);
        }
        catch (Exception ex)
        {
            throw new AuthenticationException("Failed to login into AD", ex);
        }

        this.userCacheService = userCacheService;
        this.configuration = configuration;
    }

    public async Task<JWToken> LoginAsync(UserModel user, CancellationToken cancellationToken = default)
    {
        if (!domainContext.ValidateCredentials(user.UserName, user.Password))
        {
            throw new AuthenticationException($"User '{user.UserName}' is not present in AD");
        }

        await userCacheService.SaveAsync(user, cancellationToken);

        return CreateToken(user.UserName);
    }

    private JWToken CreateToken(string userName)
    {
        var authSection = configuration.GetSection("JwtBearer");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authSection["Key"]!));

        var now = DateTime.UtcNow;
        var jwt = new JwtSecurityToken(
            issuer: authSection["Issuer"],
            audience: authSection["Audience"],
            claims: new[] { new Claim(JwtRegisteredClaimNames.Name, userName) },
            notBefore: now,
            expires: now.Add(TimeSpan.FromHours(1)),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.Aes256CbcHmacSha512));

        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

        return new(encodedJwt);
    }
}
