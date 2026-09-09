using PartnerIntegration.Api.Clients;
using PartnerIntegration.Api.Services;
using PartnerIntegration.Api.Validation;
using PartnerIntegration.Api.Configuration;
using PartnerIntegration.Api.Extensions;
using PartnerIntegration.Api.Exceptions;
using PartnerIntegration.Api.Messaging;
using Polly;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<TransactionRequestValidator>();

builder.Services.AddScoped<
    IPartnerTransactionService,
    PartnerTransactionService>();

builder.Services.Configure<RabbitMqOptions>(
builder.Configuration.GetSection(
    RabbitMqOptions.SectionName));

builder.Services.AddSingleton<
    IMessagePublisher,
    RabbitMqMessagePublisher>();

builder.Services.AddPartnerVerification(
    builder.Configuration);

builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<
    GlobalExceptionHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();

public partial class Program;