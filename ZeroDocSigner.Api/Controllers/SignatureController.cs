using Microsoft.AspNetCore.Mvc;
using ZeroDocSigner.AnyDocument.Interfaces;
using ZeroDocSigner.AnyDocument.Models;
using ZeroDocSigner.OfficeDocument.Models;
using ZeroDocSigner.OpenDocument.Models;
using ZeroDocSigner.PdfDocument.Models;
using ZeroDocSigner.Shared.Interfaces;
using ZeroDocSigner.Shared.Models;

namespace ZeroDocSigner.Api.Controllers;

[ApiController]
[Route("api/v2")]
public class SignatureController : ControllerBase
{
    [HttpPost($"{nameof(Sign)}/office")]
    public IActionResult Sign(
        [FromBody] OfficeSignatureInfo signatureInfo,
        [FromServices] IDocumentSignatureService<OfficeSignatureInfo> signatureService)
    {
        return Ok(new DocumentModel(signatureService.Sign(signatureInfo)));
    }

    [HttpPost($"{nameof(Sign)}/open")]
    public IActionResult Sign(
        [FromBody] OpenSignatureInfo signatureInfo,
        [FromServices] IDocumentSignatureService<OpenSignatureInfo> signatureService)
    {
        return Ok(new DocumentModel(signatureService.Sign(signatureInfo)));
    }

    [HttpPost($"{nameof(Sign)}/pdf")]
    public IActionResult Sign(
        [FromBody] PdfSignatureInfo signatureInfo,
        [FromServices] IDocumentSignatureService<PdfSignatureInfo> signatureService)
    {
        return Ok(new DocumentModel(signatureService.Sign(signatureInfo)));
    }

    [HttpPost($"{nameof(Sign)}/any")]
    public IActionResult Sign(
        [FromBody] AnySignatureInfo signatureInfo,
        [FromServices] IAnyDocumentSignatureService signatureService)
    {
        return Ok(new DocumentModel(signatureService.Sign(signatureInfo)));
    }

    [HttpPost(nameof(Verify))]
    public IActionResult Verify(
        [FromBody] DocumentModel model,
        [FromServices] IAnyDocumentSignatureService signatureService)
    {
        return Ok(signatureService.Verify(model.Document));
    }
}
