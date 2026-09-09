# PartnerIntegration

.NET 8 Web API for the Partner Integration BFF coding exercise.

This version implements **Requirement 1: Partner Transaction Endpoint**.

## Features

* .NET 8 Web API
* `POST /api/v1/partner/transactions`
* Request validation
* Swagger UI
* Unit tests with xUnit

## Project Structure

```text
PartnerIntegration/
├── src/
│   └── PartnerIntegration.Api/
│       ├── Controllers/
│       ├── Contracts/
│       ├── Validation/
│       └── Program.cs
│
├── tests/
│   └── PartnerIntegration.Tests/
│
└── PartnerIntegration.sln
```

## Requirements

* .NET 8 SDK

Check your installed version:

```bash
dotnet --version
```

## Build

```bash
dotnet restore
dotnet build
```

## Run

```bash
dotnet run --project src/PartnerIntegration.Api
```

The API runs at:

```text
http://localhost:8080
```

## Swagger UI

After starting the application, open:

```text
http://localhost:8080/swagger
```

## API

### Create Partner Transaction

```http
POST /api/v1/partner/transactions
```

Request:

```json
{
  "partnerId": "P-1001",
  "transactionReference": "TXN-99823",
  "amount": 250.00,
  "currency": "USD",
  "timestamp": "2024-05-10T14:30:00Z"
}
```

Successful response:

```json
{
  "message": "Transaction received",
  "transactionReference": "TXN-99823"
}
```

## Validation

The request validates the following rules:

* `partnerId` is required.
* `transactionReference` is required.
* `amount` must be greater than `0`.
* `currency` is required and must be valid.
* `timestamp` is required.

For this implementation, supported currencies are:

```text
USD
EUR
VND
```

Invalid requests return:

```http
400 Bad Request
```

with validation details.

## Unit Tests

Run tests:

```bash
dotnet test
```

Current tests cover:

* Valid transaction request
* Amount equal to zero
* Negative amount
* Invalid currency
* Missing required fields

## Example

You can test the endpoint using Swagger UI or curl:

```bash
curl -i \
  -X POST http://localhost:8080/api/v1/partner/transactions \
  -H "Content-Type: application/json" \
  -d '{
    "partnerId": "P-1001",
    "transactionReference": "TXN-99823",
    "amount": 250.00,
    "currency": "USD",
    "timestamp": "2024-05-10T14:30:00Z"
  }'
```
