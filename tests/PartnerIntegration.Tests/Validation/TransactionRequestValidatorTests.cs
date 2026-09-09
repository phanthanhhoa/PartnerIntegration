using PartnerIntegration.Api.Contracts;
using PartnerIntegration.Api.Validation;
using Xunit;

namespace PartnerIntegration.Tests.Validation;

public sealed class TransactionRequestValidatorTests
{
    private readonly TransactionRequestValidator _validator = new();

    [Fact]
    public void Validate_Should_ReturnNoErrors_When_RequestIsValid()
    {
        var request = CreateValidRequest();

        var errors = _validator.Validate(request);

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_Should_ReturnError_When_AmountIsNotPositive(
        decimal amount)
    {
        var request =
            CreateValidRequest() with
            {
                Amount = amount
            };

        var errors = _validator.Validate(request);

        Assert.Contains("amount", errors.Keys);
    }

    [Fact]
    public void Validate_Should_ReturnError_When_CurrencyIsInvalid()
    {
        var request =
            CreateValidRequest() with
            {
                Currency = "ABC"
            };

        var errors = _validator.Validate(request);

        Assert.Contains("currency", errors.Keys);
    }

    [Fact]
    public void Validate_Should_ReturnErrors_When_RequiredFieldsAreMissing()
    {
        var request =
            new CreatePartnerTransactionRequest(
                null,
                null,
                100,
                null,
                null);

        var errors = _validator.Validate(request);

        Assert.Contains("partnerId", errors.Keys);
        Assert.Contains("transactionReference", errors.Keys);
        Assert.Contains("currency", errors.Keys);
        Assert.Contains("timestamp", errors.Keys);
    }

    private static CreatePartnerTransactionRequest
        CreateValidRequest()
    {
        return new CreatePartnerTransactionRequest(
            "P-1001",
            "TXN-99823",
            250m,
            "USD",
            DateTimeOffset.Parse(
                "2024-05-10T14:30:00Z"));
    }
}