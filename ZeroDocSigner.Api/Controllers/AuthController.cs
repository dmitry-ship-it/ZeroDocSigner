using Microsoft.AspNetCore.Mvc;
using ZeroDocSigner.Api.Authentication.Models;
using ZeroDocSigner.Api.Authentication.Services;

namespace ZeroDocSigner.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService service;

    public AuthController(IAuthenticationService service)
    {
        this.service = service;
    }

    [HttpPost]
    public async Task<IActionResult> LoginAsync(
        [FromBody] UserModel model,
        CancellationToken cancellationToken = default)
    {
        return Ok(await service.LoginAsync(model, cancellationToken));
    }
}
