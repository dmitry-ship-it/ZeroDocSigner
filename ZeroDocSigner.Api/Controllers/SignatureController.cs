using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography.X509Certificates;
using ZeroDocSigner.Models;
using ZeroDocSigner.Common.Manager;

namespace ZeroDocSigner.Api.Controllers
{
    [ApiController]
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
                model.Data, _certificate, model.SignerInfo);

            manager.CreateSignature(model.Force);

            return Ok(manager.BuildFile());
        }

        [HttpPost(nameof(Verify))]
        public IActionResult Verify([FromBody] DataModel model)
        {
            _logger.LogInformation("Got data for verification, size={Length} bytes",
                model.Data.Length);

            var manager = new SignatureManager(
                model.Data, _certificate);

            return Ok(manager.Verify());
        }
    }
}