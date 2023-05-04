using Microsoft.AspNetCore.Mvc;
using ZeroDocSigner.Common.V2.Models;
using ZeroDocSigner.Common.V2.Services.Abstractions;

namespace ZeroDocSigner.Api.Controllers;

[ApiController]
[Route("api/v2")]
public class SignatureControllerV2 : ControllerBase
{
    private readonly IOfficeDocumentService documentService;

    public SignatureControllerV2(IOfficeDocumentService documentService)
    {
        this.documentService = documentService;
    }

    [HttpPost(nameof(Sign))]
    public IActionResult Sign([FromBody] OfficeSignatureInfo signatureInfo)
    {
        return Ok(documentService.Sign(signatureInfo));
    }
}
