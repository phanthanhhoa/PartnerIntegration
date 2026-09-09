using PartnerIntegration.Api.Clients;
using PartnerIntegration.Api.Services;
using PartnerIntegration.Api.Validation;
using Polly;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<TransactionRequestValidator>();

builder.Services.AddScoped<
    IPartnerTransactionService,
    PartnerTransactionService>();

builder.Services
    .AddHttpClient<
        IPartnerVerificationClient,
        PartnerVerificationClient>(client =>
    {
        var baseUrl =
            builder.Configuration[
                "PartnerVerification:BaseUrl"]
            ?? throw new InvalidOperationException(
                "PartnerVerification BaseUrl is missing.");

        client.BaseAddress = new Uri(baseUrl);
    })
    .AddStandardResilienceHandler(options =>
    {
        options.Retry.MaxRetryAttempts = 3;

        options.Retry.Delay =
            TimeSpan.FromMilliseconds(200);

        options.Retry.BackoffType =
            DelayBackoffType.Exponential;

        options.Retry.UseJitter = true;

        options.AttemptTimeout.Timeout =
            TimeSpan.FromSeconds(2);

        options.TotalRequestTimeout.Timeout =
            TimeSpan.FromSeconds(10);
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();

public partial class Program;