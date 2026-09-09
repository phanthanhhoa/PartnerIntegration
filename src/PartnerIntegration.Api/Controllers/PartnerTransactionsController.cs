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
        [FromBody]
        CreatePartnerTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var errors = _validator.Validate(request);

        if (errors.Count > 0)
        {
            var problemDetails =
                new ValidationProblemDetails(errors)
                {
                    Status =
                        StatusCodes.Status400BadRequest,

                    Title =
                        "Validation failed"
                };

            return BadRequest(problemDetails);
        }

        try
        {
            var result =
                await _transactionService.ProcessAsync(
                    request,
                    cancellationToken);

            return Accepted(result);
        }
        catch (PartnerNotVerifiedException ex)
        {
            return BadRequest(
                new ProblemDetails
                {
                    Status =
                        StatusCodes.Status400BadRequest,

                    Title =
                        "Partner verification failed",

                    Detail = ex.Message
                });
        }
        catch (PartnerVerificationUnavailableException ex)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new ProblemDetails
                {
                    Status =
                        StatusCodes.Status503ServiceUnavailable,

                    Title =
                        "Partner verification unavailable",

                    Detail = ex.Message
                });
        }
        catch (MessagePublishingException ex)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new ProblemDetails
                {
                    Status =
                        StatusCodes.Status503ServiceUnavailable,

                    Title =
                        "Message broker unavailable",

                    Detail = ex.Message
                });
        }
    }
}