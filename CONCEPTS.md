# Event Sourcing Concepts - Complete Guide

## 📚 Table of Contents

1. [What is Event Sourcing?](#what-is-event-sourcing)
2. [Core Concepts](#core-concepts)
3. [CQRS Pattern](#cqrs-pattern)
4. [Clean Architecture](#clean-architecture)
5. [Advanced Topics](#advanced-topics)
6. [Best Practices](#best-practices)

---

## What is Event Sourcing?

### Traditional Approach (CRUD)

In traditional applications, we store the **current state** of entities:

```sql
-- Accounts Table
| AccountId | Balance | LastModified |
|-----------|---------|--------------|
| 123       | $1500   | 2024-01-05   |
```

**Problems:**
- ❌ No history of changes
- ❌ Can't see how we got to current state
- ❌ Difficult to debug issues
- ❌ No audit trail
- ❌ Can't answer "What was the balance on Jan 1st?"

### Event Sourcing Approach

Instead of storing current state, we store **all the events** that led to the current state:

```sql
-- Events Table
| EventId | EventType      | Data                    | Timestamp  |
|---------|----------------|-------------------------|------------|
| 1       | AccountOpened  | {Balance: 1000}         | 2024-01-01 |
| 2       | MoneyDeposited | {Amount: 500}           | 2024-01-02 |
| 3       | MoneyWithdrawn | {Amount: 300}           | 2024-01-03 |
| 4       | MoneyDeposited | {Amount: 300}           | 2024-01-05 |
```

**Current Balance = Sum of all events = $1500**

**Benefits:**
- ✅ Complete audit trail
- ✅ Time travel (state at any point)
- ✅ Easy debugging
- ✅ Business insights from events
- ✅ Multiple views from same data

---

## Core Concepts

### 1. Events

**What are Events?**

Events are **immutable records** of things that happened in the past.

**Characteristics:**
- Named in **past tense** (AccountOpened, not OpenAccount)
- **Immutable** (never change once created)
- Contain all data about what happened
- Have timestamps
- Have version numbers

**Example:**

```csharp
public record MoneyDeposited(
    Guid AccountId,
    decimal Amount,
    string Description,
    DateTime OccurredAt
) : DomainEvent;
```

**Why Immutable?**

Once something happened, you can't change history! Events are facts.

---

### 2. Aggregates

**What are Aggregates?**

Aggregates are domain entities that:
- Make business decisions
- Emit events when things happen
- Protect business rules

**Example: BankAccount Aggregate**

```csharp
public class BankAccount : AggregateRoot<Guid>
{
    public decimal Balance { get; private set; }
    
    public void Deposit(decimal amount)
    {
        // 1. Validate business rule
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive");
        
        // 2. Create event
        var @event = new MoneyDeposited(Id, amount, DateTime.UtcNow);
        
        // 3. Apply event (update state)
        Apply(@event);
        
        // 4. Track for persistence
        _uncommittedEvents.Add(@event);
    }
    
    private void Apply(MoneyDeposited @event)
    {
        Balance += @event.Amount;
    }
}
```

**Key Pattern:**
1. Validate business rules
2. Create event
3. Apply event to update state
4. Store event for persistence

---

### 3. Event Store

**What is an Event Store?**

A specialized database that stores events in chronological order.

**Characteristics:**
- **Append-only** (no updates or deletes)
- Events organized by **stream** (one per aggregate)
- Supports **optimistic concurrency**
- Provides **audit trail**

**Operations:**

```csharp
// Save events
await eventStore.AppendEventsAsync(
    streamId: "account-123",
    events: [event1, event2],
    expectedVersion: 5  // For concurrency
);

// Load events
var events = await eventStore.GetEventsAsync("account-123");
```

**Why Append-Only?**

- Fast writes (no indexes to update)
- Perfect audit trail
- Can't accidentally lose history
- Easy to replicate

---

### 4. Event Replay

**What is Event Replay?**

The process of rebuilding an aggregate's current state by replaying all its events.

**How it Works:**

```csharp
// 1. Load all events for an aggregate
var events = await eventStore.GetEventsAsync(accountId);

// 2. Create empty aggregate
var account = new BankAccount();

// 3. Replay each event
foreach (var @event in events)
{
    account.Apply(@event);  // Updates internal state
}

// 4. Account now has current state!
Console.WriteLine(account.Balance);  // $1500
```

**When Do We Replay?**

Every time we need to load an aggregate:
- To execute a command
- To answer a query
- To validate a business rule

**Performance Optimization:**

For aggregates with many events, use **snapshots** (covered later).

---

### 5. Optimistic Concurrency

**The Problem:**

Two users try to modify the same account simultaneously:

```
User A loads account (version 5)
User B loads account (version 5)

User A deposits $100 → tries to save with expected version 5
User B withdraws $50 → tries to save with expected version 5
```

**The Solution:**

Event Store checks the version:

```csharp
await eventStore.AppendEventsAsync(
    accountId,
    events,
    expectedVersion: 5  // Must match current version
);
```

**Flow:**
1. User A saves successfully (version becomes 6)
2. User B's save fails (expected 5, but current is 6)
3. User B must reload and retry

---

## CQRS Pattern

**What is CQRS?**

CQRS = **Command Query Responsibility Segregation**

Separate paths for:
- **Commands** - Change state
- **Queries** - Read data

### Commands (Write Side)

**Purpose:** Perform actions that change state

**Examples:**
- OpenAccountCommand
- DepositMoneyCommand
- WithdrawMoneyCommand

**Flow:**

```csharp
// 1. Command
public record DepositMoneyCommand(
    Guid AccountId,
    decimal Amount,
    string Description
) : IRequest<Unit>;

// 2. Handler
public class DepositMoneyHandler : IRequestHandler<DepositMoneyCommand, Unit>
{
    public async Task<Unit> Handle(DepositMoneyCommand request, ...)
    {
        // Load aggregate
        var account = await LoadAccount(request.AccountId);
        
        // Execute business logic
        account.Deposit(request.Amount, request.Description);
        
        // Persist events
        await SaveAccount(account);
        
        return Unit.Value;
    }
}
```

### Queries (Read Side)

**Purpose:** Retrieve data for display

**Examples:**
- GetAccountQuery
- GetAccountHistoryQuery
- GetAccountAtDateQuery

**Flow:**

```csharp
// 1. Query
public record GetAccountQuery(Guid AccountId) : IRequest<AccountDto>;

// 2. Handler
public class GetAccountHandler : IRequestHandler<GetAccountQuery, AccountDto>
{
    public async Task<AccountDto> Handle(GetAccountQuery request, ...)
    {
        // Load events
        var events = await _eventStore.GetEventsAsync(request.AccountId);
        
        // Rebuild state
        var account = new BankAccount();
        account.LoadFromHistory(events);
        
        // Return DTO
        return new AccountDto { Balance = account.Balance, ... };
    }
}
```

### Benefits of CQRS

- ✅ Clear separation of concerns
- ✅ Can optimize reads and writes separately
- ✅ Can use different models for read/write
- ✅ Scalable (scale reads and writes independently)

---

## Clean Architecture

### Layer Organization

```
┌─────────────────────────────────────┐
│         API Layer                   │  ← HTTP endpoints
├─────────────────────────────────────┤
│    Application Layer                │  ← Use cases, CQRS handlers
├─────────────────────────────────────┤
│     Domain Layer                    │  ← Business logic, aggregates
├─────────────────────────────────────┤
│  Infrastructure Layer               │  ← Database, external services
└─────────────────────────────────────┘
```

### 1. Domain Layer

**Purpose:** Core business logic

**Contains:**
- Aggregates (BankAccount)
- Events (MoneyDeposited)
- Value Objects
- Domain Exceptions
- Business Rules

**No Dependencies:** Pure business logic, no frameworks!

### 2. Application Layer

**Purpose:** Use cases and orchestration

**Contains:**
- Commands and Queries
- Command/Query Handlers
- DTOs (Data Transfer Objects)
- Validators
- Interfaces (IEventStore)

**Dependencies:** Only Domain layer

### 3. Infrastructure Layer

**Purpose:** Technical implementation

**Contains:**
- Event Store implementation
- Database access
- External service integrations
- File system access

**Dependencies:** Domain + Application layers

### 4. API Layer

**Purpose:** HTTP interface

**Contains:**
- Controllers
- Middleware
- Configuration
- Startup logic

**Dependencies:** All other layers

---

## Advanced Topics

### 1. Projections (Read Models)

**Problem:** Replaying 10,000 events is slow!

**Solution:** Build read models (projections)

```csharp
// Listen to events and build optimized view
public class AccountSummaryProjection
{
    private readonly Dictionary<Guid, Summary> _summaries = new();
    
    public void Handle(AccountOpened @event)
    {
        _summaries[@event.AccountId] = new Summary
        {
            AccountId = @event.AccountId,
            Balance = @event.InitialDeposit
        };
    }
    
    public void Handle(MoneyDeposited @event)
    {
        _summaries[@event.AccountId].Balance += @event.Amount;
    }
}
```

**Benefits:**
- ✅ Fast queries (no event replay)
- ✅ Can have multiple projections
- ✅ Can rebuild anytime from events

### 2. Snapshots

**Problem:** 100,000 events to replay!

**Solution:** Periodic snapshots of state

```csharp
// Save snapshot every 1000 events
if (account.Version % 1000 == 0)
{
    await SaveSnapshot(new AccountSnapshot
    {
        AccountId = account.Id,
        Balance = account.Balance,
        Version = account.Version
    });
}

// Load from snapshot
var snapshot = await LoadSnapshot(accountId);
var recentEvents = await LoadEventsSince(accountId, snapshot.Version);

// Only replay recent events
account.LoadFromSnapshot(snapshot);
account.LoadFromHistory(recentEvents);
```

### 3. Event Versioning

**Problem:** Events change over time

**Example:**

```csharp
// Version 1
public record MoneyDeposited(Guid AccountId, decimal Amount);

// Version 2 - Added currency
public record MoneyDepositedV2(Guid AccountId, decimal Amount, string Currency);
```

**Solutions:**

**a) Upcasting**
Convert old events to new format when loading:

```csharp
private object Upcast(object @event)
{
    if (@event is MoneyDeposited old)
        return new MoneyDepositedV2(old.AccountId, old.Amount, "USD");
    
    return @event;
}
```

**b) Multi-version handling**
Support both versions:

```csharp
protected override void Apply(object @event)
{
    switch (@event)
    {
        case MoneyDeposited e: Balance += e.Amount; break;
        case MoneyDepositedV2 e: Balance += e.Amount; Currency = e.Currency; break;
    }
}
```

### 4. Time Travel Queries

**Show state at any point in history:**

```csharp
// Get balance on January 1st
var historicalEvents = events
    .Where(e => e.OccurredAt <= new DateTime(2024, 1, 1))
    .ToList();

var account = new BankAccount();
account.LoadFromHistory(historicalEvents);

Console.WriteLine($"Balance on Jan 1: {account.Balance}");
```

**Use Cases:**
- Auditing
- Debugging
- Compliance
- Analysis

---

## Best Practices

### 1. Event Design

✅ **DO:**
- Use past tense names (OrderPlaced, not PlaceOrder)
- Make events immutable
- Include all relevant data
- Keep events small and focused

❌ **DON'T:**
- Change published events
- Include computed values
- Make events too large
- Use present/future tense

### 2. Aggregate Design

✅ **DO:**
- Keep aggregates small
- Validate in aggregate methods
- Emit events for all changes
- Make state private

❌ **DON'T:**
- Call external services in aggregates
- Access other aggregates directly
- Put infrastructure code in domain
- Expose mutable state

### 3. Performance

✅ **Optimize:**
- Use snapshots for large event streams
- Build read models for queries
- Cache projections
- Batch event writes

❌ **Avoid:**
- Replaying millions of events
- Storing huge events
- Synchronous projection updates
- Loading entire event store

### 4. Testing

✅ **Test:**
- Business rules in aggregates
- Event emission
- Event replay
- Command handlers
- Query handlers

**Example:**

```csharp
[Fact]
public void Deposit_ShouldEmitMoneyDepositedEvent()
{
    // Arrange
    var account = CreateAccount();
    
    // Act
    account.Deposit(100m, "Test");
    
    // Assert
    var events = account.GetUncommittedEvents();
    events.Should().ContainSingle();
    events[0].Should().BeOfType<MoneyDeposited>();
}
```

---

## When to Use Event Sourcing

### ✅ Good Use Cases

- Financial systems (banking, trading)
- Audit-required systems
- Complex business rules
- Need for temporal queries
- Machine learning (events as training data)
- Debugging production issues

### ❌ Poor Use Cases

- Simple CRUD applications
- No audit requirements
- Read-heavy with simple writes
- Team unfamiliar with pattern
- Tight deadlines

---

## Common Pitfalls

### 1. Over-Engineering

❌ **Problem:** Using Event Sourcing everywhere

✅ **Solution:** Use selectively for core domains

### 2. Large Events

❌ **Problem:** Storing entire documents in events

✅ **Solution:** Store only changes, reference external data

### 3. No Versioning Strategy

❌ **Problem:** Breaking changes to events

✅ **Solution:** Plan for versioning from day one

### 4. Ignoring Projections

❌ **Problem:** Always replaying events for queries

✅ **Solution:** Build read models for fast queries

---

## Summary

**Event Sourcing is:**
- Storing events instead of state
- Complete audit trail
- Time travel capability
- Foundation for CQRS

**Key Benefits:**
- Perfect audit trail
- Debugging capability
- Business insights
- Temporal queries

**Challenges:**
- Complexity
- Event versioning
- Query performance
- Learning curve

**Best For:**
- Financial systems
- Regulated industries
- Complex domains
- Audit requirements

---

**Ready to implement Event Sourcing? Start with the code examples in this solution! 🚀**
