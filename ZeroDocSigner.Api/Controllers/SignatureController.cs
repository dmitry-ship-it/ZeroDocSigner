using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.X509Certificates;
using ZeroDocSigner.Api.Models;
using ZeroDocSigner.Common;

namespace ZeroDocSigner.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SignatureController : ControllerBase
    {
        private readonly X509Certificate2 _certificate;
        private readonly ILogger<SignatureController> _logger;

        public SignatureController(
            X509Certificate2 certificate,
            ILogger<SignatureController> logger)
        {
            _certificate = certificate;
            _logger = logger;
        }

        [HttpPost(nameof(Sign))]
        public IActionResult Sign([FromBody] SignModel model)
        {
            var manager = new SignatureManager(model.Data, _certificate);
            return Ok(manager.CreateSignature(model.Parameters, model.Force));
        }

        [HttpPost(nameof(Verify))]
        public IActionResult Verify([FromBody] BaseModel model)
        {
            _logger.LogInformation("Got data for verification, length={Length}", model.Data.Length);
            var manager = new SignatureManager(model.Data, _certificate);
            return Ok(manager.Verify());
        }
    }
}