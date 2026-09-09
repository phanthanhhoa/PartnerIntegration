# PartnerIntegration

`PartnerIntegration` is a .NET 8 Backend-for-Frontend (BFF) Web API that receives transaction data from external partners, validates the request, verifies the partner through an HTTP service, and publishes accepted transactions to RabbitMQ for asynchronous processing.

The solution focuses on clean architecture, separation of concerns, resilience, testability, and keeping the implementation simple enough for the scope of the coding exercise.

---

## Architecture

```text
Partner
   |
   | POST /api/v1/partner/transactions
   | X-API-Key
   v
ApiKeyMiddleware
   |
   v
PartnerTransactionsController
   |
   v
TransactionRequestValidator
   |
   v
PartnerTransactionService
   |
   +------------------------------+
   |                              |
   v                              v
IPartnerVerificationClient    IMessagePublisher
   |                              |
   v                              v
PartnerVerificationClient    RabbitMqMessagePublisher
   |                              |
   | HttpClient + Resilience      |
   v                              v
Mock Partner API              RabbitMQ
                                   |
                                   v
                              Legacy System
                              (not implemented)
```

---

## Features

* .NET 8 Web API
* Partner transaction endpoint
* Request validation
* Mock Partner Verification API
* HTTP retry and resilience strategy
* RabbitMQ message publishing
* Global exception handling
* API Key security
* Swagger UI
* xUnit tests
* Code coverage support
* Docker
* Docker Compose

---

## Project Structure

```text
PartnerIntegration/
├── src/
│   └── PartnerIntegration.Api/
│       ├── Clients/
│       ├── Configuration/
│       ├── Contracts/
│       ├── Controllers/
│       ├── Exceptions/
│       ├── Messaging/
│       ├── Security/
│       ├── Services/
│       ├── Validation/
│       ├── Dockerfile
│       ├── Program.cs
│       └── PartnerIntegration.Api.csproj
│
├── tests/
│   └── PartnerIntegration.Tests/
│       ├── Clients/
│       ├── Services/
│       └── Validation/
│
├── .dockerignore
├── .env.example
├── .gitignore
├── docker-compose.yml
├── PartnerIntegration.sln
└── README.md
```

---

# API

## Create Partner Transaction

```http
POST /api/v1/partner/transactions
```

Required header:

```http
X-API-Key: <api-key>
```

Example request:

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

```http
202 Accepted
```

Example response:

```json
{
  "message": "Transaction accepted",
  "transactionReference": "TXN-99823"
}
```

`202 Accepted` is used because the transaction is accepted and queued for asynchronous downstream processing.

---

# Validation

The API validates the following rules:

* `partnerId` is required.
* `transactionReference` is required.
* `amount` must be greater than `0`.
* `currency` is required and must be supported.
* `timestamp` is required.

Supported currencies for this exercise:

```text
USD
EUR
VND
```

Invalid requests return:

```http
400 Bad Request
```

---

# Partner Verification

Before publishing a transaction to RabbitMQ, the service verifies the partner through a mock Partner Verification API.

Endpoint:

```http
GET /api/v1/mock/partners/{partnerId}/verify
```

The mock endpoint behaves approximately as follows:

```text
70% -> valid response
30% -> TimeoutException
```

The mock API is implemented in the same application for the purpose of the coding exercise.

---

# Resilience Strategy

Partner verification uses `HttpClientFactory` with a resilience pipeline.

The strategy includes:

* bounded retries
* exponential backoff
* jitter
* request timeout

Transient failures are retried automatically.

If partner verification is still unavailable after the retry attempts, the API returns:

```http
503 Service Unavailable
```

This prevents failures in the external service from crashing the incoming request.

---

# Asynchronous Messaging

After the request is validated and the partner is successfully verified, the transaction is published to RabbitMQ.

The business service depends on:

```text
IMessagePublisher
```

The RabbitMQ implementation is:

```text
RabbitMqMessagePublisher
```

This keeps RabbitMQ-specific code separated from the business logic.

Messages are published to:

```text
partner-transactions
```

A separate message contract is used instead of publishing the HTTP request DTO directly.

```text
CreatePartnerTransactionRequest
            ↓
PartnerTransactionMessage
            ↓
RabbitMQ
```

If publishing fails, the API returns:

```http
503 Service Unavailable
```

instead of returning a successful response.

---

# Global Exception Handling

The application uses a global exception handler to provide consistent error responses.

Example mappings:

```text
PartnerNotVerifiedException
→ 400 Bad Request

PartnerVerificationUnavailableException
→ 503 Service Unavailable

MessagePublishingException
→ 503 Service Unavailable

Unhandled Exception
→ 500 Internal Server Error
```

Errors are returned using ASP.NET Core `ProblemDetails`.

---

# Security

The partner transaction endpoint is protected using an API key.

Clients must include:

```http
X-API-Key: <api-key>
```

The API key is not hard-coded into the application and should not be committed to source control.

For local development, the key can be configured through:

* environment variables
* .NET User Secrets

For a production partner integration, stronger authentication mechanisms such as OAuth 2.0 Client Credentials or mTLS should be considered.

---

# Swagger UI

Swagger UI is available in Development mode.

After starting the API, open:

```text
http://localhost:8080/swagger
```

Click:

```text
Authorize
```

and enter the configured API key.

Example:

```text
local-development-key
```

Swagger will send:

```http
X-API-Key: local-development-key
```

with protected requests.

---

# Run Locally

## Prerequisites

* .NET 8 SDK
* RabbitMQ

Check .NET:

```bash
dotnet --version
```

---

## 1. Configure API Key

Recommended option using .NET User Secrets:

```bash
dotnet user-secrets init \
  --project src/PartnerIntegration.Api
```

Set the API key:

```bash
dotnet user-secrets set \
  "Security:ApiKey" \
  "local-development-key" \
  --project src/PartnerIntegration.Api
```

Alternatively:

```bash
export Security__ApiKey="local-development-key"
```

.NET maps:

```text
Security__ApiKey
```

to:

```text
Security:ApiKey
```

---

## 2. Start RabbitMQ

RabbitMQ can be started with Docker:

```bash
docker run -d \
  --name partner-rabbitmq \
  -p 5672:5672 \
  -p 15672:15672 \
  -e RABBITMQ_DEFAULT_USER=partner \
  -e RABBITMQ_DEFAULT_PASS=partner-local-password \
  rabbitmq:management
```

Configure the application:

```bash
export RabbitMq__HostName="localhost"
export RabbitMq__Port="5672"
export RabbitMq__UserName="partner"
export RabbitMq__Password="partner-local-password"
export RabbitMq__QueueName="partner-transactions"
```

---

## 3. Restore

```bash
dotnet restore
```

---

## 4. Build

```bash
dotnet build
```

---

## 5. Run Tests

```bash
dotnet test
```

Run with code coverage:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

---

## 6. Run API

```bash
dotnet run --project src/PartnerIntegration.Api
```

API:

```text
http://localhost:8080
```

Swagger:

```text
http://localhost:8080/swagger
```

---

# Test with curl

```bash
curl -i \
  -X POST http://localhost:8080/api/v1/partner/transactions \
  -H "Content-Type: application/json" \
  -H "X-API-Key: local-development-key" \
  -d '{
    "partnerId": "P-1001",
    "transactionReference": "TXN-99823",
    "amount": 250.00,
    "currency": "USD",
    "timestamp": "2024-05-10T14:30:00Z"
  }'
```

Expected successful result:

```http
202 Accepted
```

---

# Run with Docker Compose

Docker Compose starts both:

```text
PartnerIntegration.Api
RabbitMQ
```

## 1. Create `.env`

Copy:

```bash
cp .env.example .env
```

Example `.env`:

```env
SECURITY_API_KEY=local-development-key
RABBITMQ_USER=partner
RABBITMQ_PASSWORD=partner-local-password
```

Do not commit `.env`.

Make sure `.gitignore` contains:

```text
.env
```

---

## 2. Build and Start

```bash
docker compose up --build
```

Run in background:

```bash
docker compose up -d --build
```

---

## 3. Check Containers

```bash
docker compose ps
```

Expected services:

```text
api
rabbitmq
```

---

## 4. View Logs

API logs:

```bash
docker compose logs -f api
```

RabbitMQ logs:

```bash
docker compose logs -f rabbitmq
```

---

## 5. Access Services

API:

```text
http://localhost:8080
```

Swagger:

```text
http://localhost:8080/swagger
```

RabbitMQ Management:

```text
http://localhost:15672
```

Use the RabbitMQ credentials configured in `.env`.

---

## 6. Verify Published Messages

Open RabbitMQ Management:

```text
Queues and Streams
```

Open:

```text
partner-transactions
```

After a successful API request, the queue should contain the published transaction.

Because the coding exercise only requires publishing messages and does not implement the legacy consumer, messages may remain in:

```text
Ready
```

state.

---

## 7. Stop Containers

```bash
docker compose down
```

Remove containers and volumes:

```bash
docker compose down -v
```

---

# Testing Strategy

The test project focuses on business behavior rather than infrastructure implementation details.

## Validation Tests

Tests cover:

* valid request
* missing required fields
* zero amount
* negative amount
* invalid currency

## Partner Verification Tests

A fake `HttpMessageHandler` is used to simulate HTTP responses.

Example:

```text
Request 1 → 500
Request 2 → 500
Request 3 → 200
```

Expected result:

```text
Retry succeeds
CallCount = 3
```

Additional tests cover:

* retry exhaustion
* non-transient HTTP failures

## Messaging Tests

`PartnerTransactionService` is tested with fake implementations of:

```text
IPartnerVerificationClient
IMessagePublisher
```

Tests verify:

```text
Valid partner
→ message published

Invalid partner
→ message not published

Request data
→ correctly mapped to message

Publisher failure
→ request does not report success
```

This keeps unit tests fast and independent from RabbitMQ.

---

# Configuration

Main configuration keys:

```text
PartnerVerification:BaseUrl

RabbitMq:HostName
RabbitMq:Port
RabbitMq:UserName
RabbitMq:Password
RabbitMq:QueueName

Security:ApiKey
```

Environment variables use double underscores:

```text
PartnerVerification__BaseUrl

RabbitMq__HostName
RabbitMq__Port
RabbitMq__UserName
RabbitMq__Password
RabbitMq__QueueName

Security__ApiKey
```

Secrets such as API keys and RabbitMQ passwords must not be committed to source control.

---

# Design Decisions

## Thin Controller

The controller handles:

```text
HTTP request
Validation
Service call
HTTP response
```

Business orchestration is handled by `PartnerTransactionService`.

## Dependency Inversion

The service depends on:

```text
IPartnerVerificationClient
IMessagePublisher
```

rather than concrete infrastructure implementations.

This improves testability and allows infrastructure implementations to change without modifying business logic.

## Separate Message Contract

The HTTP request DTO is not published directly to RabbitMQ.

This avoids coupling the external API contract with the internal messaging contract.

## No Database

The exercise does not require local persistence.

Adding a database, repository, or Unit of Work would introduce unnecessary complexity.

## No Consumer

The requirement is to queue transactions for legacy systems to process.

The legacy consumer is outside the scope of this exercise.

---

# Production Considerations

For a production implementation, additional considerations would include:

* OAuth 2.0 Client Credentials or mTLS
* API Gateway
* secret management / vault
* rate limiting per partner
* idempotency
* structured logging
* distributed tracing
* OpenTelemetry
* health checks
* RabbitMQ dead-letter queues
* poison message handling
* consumer acknowledgements
* retry policies
* transactional outbox when database persistence and message publishing must be atomic

---

# Summary

The solution focuses on:

```text
Clean architecture
Simple responsibilities
Testability
Resilient HTTP integration
Reliable asynchronous messaging
Consistent error handling
Lightweight endpoint security
Minimal unnecessary complexity
```
