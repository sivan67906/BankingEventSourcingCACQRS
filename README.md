# Banking Event Sourcing - Complete .NET 9 Solution

## Overview

This is a comprehensive, production-ready .NET 9 Web API implementing **Event Sourcing** with **CQRS** pattern using **Clean Architecture**. The solution demonstrates all advanced Event Sourcing concepts through a banking domain example.

## 🎯 Key Features

- ✅ **Clean Architecture** with proper layer separation
- ✅ **Event Sourcing** pattern implementation
- ✅ **CQRS** (Command Query Responsibility Segregation)
- ✅ **MediatR** for command/query handling
- ✅ **FluentValidation** for input validation
- ✅ **In-Memory & Marten** event store options
- ✅ **Audit Trail** and complete event history
- ✅ **Time Travel** queries (state at any point in time)
- ✅ **Optimistic Concurrency** control
- ✅ **Pipeline Behaviors** (Logging, Validation, Performance)
- ✅ **Unit Tests** with xUnit and FluentAssertions
- ✅ **Swagger/OpenAPI** documentation

## 📁 Solution Structure

```
BankingEventSourcing/
├── src/
│   ├── BankingEventSourcing.Domain/          # Business logic & entities
│   ├── BankingEventSourcing.Application/     # Use cases & CQRS handlers
│   ├── BankingEventSourcing.Infrastructure/  # Data access & external services
│   └── BankingEventSourcing.API/             # REST API endpoints
└── tests/
    ├── BankingEventSourcing.Domain.Tests/
    ├── BankingEventSourcing.Application.Tests/
    └── BankingEventSourcing.API.Tests/
```

## 🚀 Quick Start

### Prerequisites

- .NET 9 SDK
- (Optional) PostgreSQL for Marten event store
- (Optional) Docker for containerized PostgreSQL

### Running the Application

1. **Clone the repository**
   ```bash
   cd BankingEventSourcing
   ```

2. **Build the solution**
   ```bash
   dotnet build
   ```

3. **Run the API**
   ```bash
   cd src/BankingEventSourcing.API
   dotnet run
   ```

4. **Access Swagger UI**
   Open browser: `https://localhost:5001/swagger`

### Using In-Memory Event Store (Default)

The solution is configured to use in-memory event store by default. No database setup required!

### Using Marten Event Store (PostgreSQL)

1. Start PostgreSQL:
   ```bash
   docker run -d -p 5432:5432 -e POSTGRES_PASSWORD=postgres postgres
   ```

2. Update `Program.cs`:
   ```csharp
   services.AddInfrastructureServices(builder.Configuration, useInMemory: false);
   ```

## 📖 Understanding Event Sourcing

### What is Event Sourcing?

Instead of storing current state, Event Sourcing stores **all changes as a sequence of events**.

**Traditional Approach:**
```
Database: { AccountId: 1, Balance: 1500 }
// Previous balances are lost
```

**Event Sourcing Approach:**
```
Events:
1. AccountOpened (Balance: 1000)
2. MoneyDeposited (+500)
3. MoneyWithdrawn (-200)
4. MoneyDeposited (+200)

Current Balance = Replay events = 1500
```

### Core Concepts

#### 1. **Domain Events**
Immutable facts about what happened:
- `AccountOpened`
- `MoneyDeposited`
- `MoneyWithdrawn`
- `AccountClosed`

#### 2. **Aggregates**
Domain entities that emit events:
- `BankAccount` - manages account state through events

#### 3. **Event Store**
Database storing all events:
- Append-only
- Complete audit trail
- Source of truth

#### 4. **CQRS**
Separate paths for commands and queries:
- **Commands**: Change state (Open, Deposit, Withdraw)
- **Queries**: Read data (GetAccount, GetHistory)

#### 5. **Event Replay**
Rebuild state by replaying events:
```csharp
var events = await eventStore.GetEventsAsync(accountId);
var account = new BankAccount();
account.LoadFromHistory(events);
// Account state is now reconstructed
```

## 🎨 API Endpoints

### Commands (Write Operations)

```http
POST /api/accounts
{
  "accountHolderName": "John Doe",
  "email": "john@example.com",
  "initialDeposit": 1000
}
```

```http
POST /api/accounts/{id}/deposit
{
  "amount": 500,
  "description": "Salary"
}
```

```http
POST /api/accounts/{id}/withdraw
{
  "amount": 200,
  "description": "Rent"
}
```

```http
POST /api/accounts/{id}/close
{
  "reason": "Customer request"
}
```

### Queries (Read Operations)

```http
GET /api/accounts/{id}
// Get current account state

GET /api/accounts
// Get all accounts

GET /api/accounts/{id}/history
// Get complete event history

GET /api/accounts/{id}/at-date?date=2024-01-01
// Time travel query
```

## 🔬 Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/BankingEventSourcing.Domain.Tests
```

## 🏆 Advanced Features

### 1. **Optimistic Concurrency**
Prevents conflicts when multiple operations happen simultaneously:
```csharp
await eventStore.AppendEventsAsync(
    streamId,
    events,
    expectedVersion: currentVersion  // Fails if version doesn't match
);
```

### 2. **Time Travel Queries**
View account state at any historical point:
```csharp
var accountYesterday = await mediator.Send(
    new GetAccountAtDateQuery(accountId, DateTime.UtcNow.AddDays(-1))
);
```

### 3. **Complete Audit Trail**
Every change is recorded:
```csharp
var history = await mediator.Send(new GetAccountHistoryQuery(accountId));
// Returns all events with timestamps
```

### 4. **Pipeline Behaviors**
Cross-cutting concerns:
- **Validation**: FluentValidation before command execution
- **Logging**: Request/response logging
- **Performance**: Slow query detection

## 📚 Learning Resources

### Key Concepts to Understand

1. **Event Sourcing Basics**
   - Events vs State
   - Event replay
   - Audit trail benefits

2. **CQRS Pattern**
   - Command handlers
   - Query handlers
   - Separation of concerns

3. **Clean Architecture**
   - Domain layer (business logic)
   - Application layer (use cases)
   - Infrastructure layer (technical details)
   - API layer (presentation)

4. **Advanced Topics**
   - Event versioning
   - Snapshots (not implemented, but easy to add)
   - Projections
   - Saga patterns

### Recommended Reading

- Martin Fowler: Event Sourcing
- Greg Young: CQRS/Event Sourcing
- Vaughn Vernon: Implementing Domain-Driven Design

## 🎯 Common Patterns

### Opening an Account
```csharp
// 1. User sends command
var command = new OpenAccountCommand("John Doe", "john@example.com", 1000);

// 2. MediatR routes to handler
var accountId = await mediator.Send(command);

// 3. Handler creates aggregate
var account = BankAccount.OpenAccount(accountId, name, email, deposit);

// 4. Aggregate emits event
// AccountOpened event is created

// 5. Event is persisted
await eventStore.AppendEventsAsync(accountId, account.GetUncommittedEvents());
```

### Querying Account State
```csharp
// 1. User sends query
var query = new GetAccountQuery(accountId);

// 2. Handler loads events
var events = await eventStore.GetEventsAsync(accountId);

// 3. Aggregate rebuilds state
var account = new BankAccount();
account.LoadFromHistory(events);

// 4. Return current state
return new AccountDto { Balance = account.Balance, ... };
```

## 🛠️ Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "EventStore": "Host=localhost;Database=BankingES;Username=postgres;Password=postgres"
  }
}
```

### Switching Event Stores

In `Program.cs`:
```csharp
// In-Memory (for development/testing)
services.AddInfrastructureServices(configuration, useInMemory: true);

// Marten/PostgreSQL (for production)
services.AddInfrastructureServices(configuration, useInMemory: false);
```

## 🚧 Future Enhancements

- [ ] Snapshots for performance optimization
- [ ] Event versioning/upcasting
- [ ] Projections (read models)
- [ ] Background event processors
- [ ] Integration events (for microservices)
- [ ] Event replay admin tools
- [ ] Docker Compose setup
- [ ] Authentication/Authorization
- [ ] Rate limiting
- [ ] Distributed tracing

## 📝 License

MIT License - feel free to use for learning and production!

## 🤝 Contributing

Contributions welcome! This is an educational project demonstrating Event Sourcing best practices.

## 📞 Support

For questions about Event Sourcing concepts or implementation, please open an issue!

---

**Happy Event Sourcing! 🚀**
