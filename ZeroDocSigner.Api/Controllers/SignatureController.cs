using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.X509Certificates;
using ZeroDocSigner.Api.Models;
using ZeroDocSigner.Common;
using ZeroDocSigner.Common.Manager;

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
            var manager = new SignatureManager(
                model.Data, _certificate);

            manager.AddSignature();

            return Ok(manager.BuildFile());
        }

        [HttpPost(nameof(Verify))]
        public IActionResult Verify([FromBody] DataModel model)
        {
            _logger.LogInformation("Got data for verification, length={Length}",
                model.Data.Length);

            var manager = new SignatureManager(
                model.Data, _certificate);

            return Ok(manager.Verify());
        }
    }
}