using Microsoft.AspNetCore.Mvc;
using PartnerIntegration.Api.Contracts;

namespace PartnerIntegration.Api.Controllers;

[ApiController]
[Route("api/v1/mock/partners")]
public sealed class MockPartnerVerificationController : ControllerBase
{
    [HttpGet("{partnerId}/verify")]
    public ActionResult<PartnerVerificationResponse> Verify(
        string partnerId)
    {
        var shouldTimeout =
            Random.Shared.NextDouble() < 0.30;

        if (shouldTimeout)
        {
            throw new TimeoutException(
                "Partner verification timed out.");
        }

        return Ok(
            new PartnerVerificationResponse(
                partnerId,
                true));
    }
}