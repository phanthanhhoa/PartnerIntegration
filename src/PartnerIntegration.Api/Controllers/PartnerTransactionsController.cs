using Microsoft.AspNetCore.Mvc;
using PartnerIntegration.Api.Contracts;
using PartnerIntegration.Api.Exceptions;
using PartnerIntegration.Api.Services;
using PartnerIntegration.Api.Validation;

namespace PartnerIntegration.Api.Controllers;

[ApiController]
[Route("api/v1/partner/transactions")]
public sealed class PartnerTransactionsController
    : ControllerBase
{
    private readonly TransactionRequestValidator _validator;

    private readonly IPartnerTransactionService
        _transactionService;

    public PartnerTransactionsController(
        TransactionRequestValidator validator,
        IPartnerTransactionService transactionService)
    {
        _validator = validator;
        _transactionService = transactionService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreatePartnerTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var errors = _validator.Validate(request);

        if (errors.Count > 0)
        {
            return BadRequest(
                new ValidationProblemDetails(errors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed"
                });
        }

        var result =
            await _transactionService.ProcessAsync(
                request,
                cancellationToken);

        return Accepted(result);
    }
}