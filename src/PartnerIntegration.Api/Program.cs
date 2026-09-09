using PartnerIntegration.Api.Validation;
using PartnerIntegration.Api.Clients;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<TransactionRequestValidator>();
builder.Services.AddHttpClient<
    IPartnerVerificationClient,
    PartnerVerificationClient>(client =>
{
    client.BaseAddress =
        new Uri("http://localhost:8080");

    client.Timeout =
        TimeSpan.FromSeconds(3);
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();

public partial class Program;