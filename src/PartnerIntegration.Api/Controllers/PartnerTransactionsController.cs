using Microsoft.AspNetCore.Mvc;
using PartnerIntegration.Api.Contracts;
using PartnerIntegration.Api.Validation;

namespace PartnerIntegration.Api.Controllers;

[ApiController]
[Route("api/v1/partner/transactions")]
public sealed class PartnerTransactionsController : ControllerBase
{
    private readonly TransactionRequestValidator _validator;

    public PartnerTransactionsController(TransactionRequestValidator validator)
    {
        _validator = validator;
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreatePartnerTransactionRequest request)
    {
        var errors = _validator.Validate(request);

        if (errors.Count > 0)
        {
            var problemDetails = new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed"
            };

            return BadRequest(problemDetails);
        }

        return Ok(new TransactionReceivedResponse(
            "Transaction received",
            request.TransactionReference!));
    }
}
