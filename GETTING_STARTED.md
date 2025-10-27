# Getting Started with Banking Event Sourcing

## 🎯 Prerequisites

- .NET 9 SDK installed
- Basic C# knowledge
- Understanding of REST APIs
- (Optional) PostgreSQL for persistent event store

## 📦 Installation

### 1. Restore NuGet Packages

```bash
cd BankingEventSourcing
dotnet restore
```

### 2. Build the Solution

```bash
dotnet build
```

### 3. Run Tests

```bash
dotnet test
```

## 🚀 Running the Application

### Option 1: In-Memory Event Store (Quickest)

```bash
cd src/BankingEventSourcing.API
dotnet run
```

The API will start on `https://localhost:5001`

### Option 2: With PostgreSQL (Production-like)

1. Start PostgreSQL:
```bash
docker run -d \
  --name postgres-eventstore \
  -e POSTGRES_PASSWORD=postgres \
  -p 5432:5432 \
  postgres:15
```

2. Update Program.cs:
```csharp
services.AddInfrastructureServices(builder.Configuration, useInMemory: false);
```

3. Run the API:
```bash
cd src/BankingEventSourcing.API
dotnet run
```

## 📝 Your First API Calls

### 1. Open Swagger UI

Navigate to: `https://localhost:5001/swagger`

### 2. Create an Account

```bash
curl -X POST https://localhost:5001/api/accounts \
  -H "Content-Type: application/json" \
  -d '{
    "accountHolderName": "John Doe",
    "email": "john@example.com",
    "initialDeposit": 1000
  }'
```

Response:
```json
{
  "accountId": "guid-here"
}
```

### 3. Deposit Money

```bash
curl -X POST https://localhost:5001/api/accounts/{accountId}/deposit \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 500,
    "description": "Monthly salary"
  }'
```

### 4. Get Account Details

```bash
curl https://localhost:5001/api/accounts/{accountId}
```

Response:
```json
{
  "accountId": "guid",
  "accountHolderName": "John Doe",
  "email": "john@example.com",
  "balance": 1500,
  "isClosed": false,
  "createdAt": "2024-01-01T00:00:00Z",
  "version": 2
}
```

### 5. View Event History

```bash
curl https://localhost:5001/api/accounts/{accountId}/history
```

Response:
```json
[
  {
    "eventType": "AccountOpened",
    "eventData": "{\"AccountId\":\"...\",\"InitialDeposit\":1000}",
    "occurredAt": "2024-01-01T00:00:00Z",
    "version": 1
  },
  {
    "eventType": "MoneyDeposited",
    "eventData": "{\"Amount\":500,\"Description\":\"Monthly salary\"}",
    "occurredAt": "2024-01-01T00:05:00Z",
    "version": 2
  }
]
```

## 🎓 Understanding the Flow

### Command Flow (Write Operations)

```
1. User → API Controller
   POST /api/accounts/{id}/deposit
   
2. Controller → MediatR
   Send(DepositMoneyCommand)
   
3. MediatR → Command Handler
   DepositMoneyHandler.Handle()
   
4. Handler → Event Store
   Load events → Rebuild aggregate
   
5. Aggregate → Business Logic
   account.Deposit(amount) → Validates rules
   
6. Aggregate → Event Creation
   Creates MoneyDeposited event
   
7. Handler → Event Store
   Persist new events
   
8. API → Response
   200 OK
```

### Query Flow (Read Operations)

```
1. User → API Controller
   GET /api/accounts/{id}
   
2. Controller → MediatR
   Send(GetAccountQuery)
   
3. MediatR → Query Handler
   GetAccountHandler.Handle()
   
4. Handler → Event Store
   Load all events for account
   
5. Handler → Aggregate Reconstruction
   Replay events to rebuild state
   
6. Handler → DTO Mapping
   Convert to AccountDto
   
7. API → Response
   Return current state
```

## 🔍 Exploring the Code

### Domain Layer

Start here to understand the business logic:

1. **Aggregates/BankAccount.cs**
   - Core business entity
   - Methods: Deposit(), Withdraw(), Close()
   - Event emission

2. **Events/**
   - AccountOpened, MoneyDeposited, etc.
   - Immutable event records

### Application Layer

CQRS implementation:

1. **Commands/** - Write operations
2. **CommandHandlers/** - Execute commands
3. **Queries/** - Read operations
4. **QueryHandlers/** - Execute queries

### Infrastructure Layer

Technical implementation:

1. **EventStore/** - Event persistence
2. **InMemoryEventStore.cs** - For development
3. **MartenEventStore.cs** - For production

### API Layer

REST endpoints:

1. **Controllers/AccountsController.cs** - HTTP endpoints
2. **Middleware/** - Error handling
3. **Program.cs** - Application setup

## 🧪 Running Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity detailed

# Run specific test
dotnet test --filter "FullyQualifiedName~BankAccountTests"
```

## 📊 Monitoring

### Logging

The application logs to console. Look for:

```
[Information] Handling OpenAccountCommand
[Information] Account abc-123 opened successfully
[Information] Handled OpenAccountCommand in 45ms
```

### Performance Monitoring

Slow requests (>500ms) are logged:

```
[Warning] Slow request detected: DepositMoneyCommand took 612ms
```

## 🎯 Next Steps

1. **Experiment with API**
   - Create multiple accounts
   - Try different operations
   - View event histories

2. **Study the Code**
   - Read through each layer
   - Understand event flow
   - Check validation rules

3. **Add Features**
   - Try adding a transfer operation
   - Implement account limits
   - Add more event types

4. **Explore Advanced Concepts**
   - Try time-travel queries
   - Look at optimistic concurrency
   - Understand aggregate versioning

## ❓ Troubleshooting

### Port Already in Use

```bash
# Change port in launchSettings.json or:
dotnet run --urls "https://localhost:5002"
```

### PostgreSQL Connection Issues

```bash
# Check if PostgreSQL is running:
docker ps

# Check connection string in appsettings.json
```

### Build Errors

```bash
# Clean and rebuild:
dotnet clean
dotnet build
```

## 📚 Additional Resources

- See README.md for comprehensive overview
- Check CONCEPTS.md for Event Sourcing theory
- Review code comments for implementation details

**You're ready to start exploring Event Sourcing! 🎉**
