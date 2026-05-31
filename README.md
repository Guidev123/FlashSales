# FlashSales

A production-grade **flash sales platform** built as a **modular monolith** with ASP.NET Core 10. Designed to handle the core challenge of flash sales: competing buyers racing for limited-stock products under high concurrency — while keeping the codebase maintainable, testable, and ready to scale.

---

## What It Does

Sellers register, get activated, and list products. They then create time-limited, limited-stock offers (**Launches**). When a launch goes live, buyers compete to place orders. The system guarantees no overselling even under extreme concurrent load, processes payments, and notifies users throughout the flow.

---

## Architecture

This project is a **modular monolith** — a single deployable unit where each business domain lives in a fully isolated module with its own database schema, domain model, and application layer. Modules communicate exclusively through well-defined contracts: asynchronously via integration events over Azure Service Bus (Outbox/Inbox pattern), and synchronously via in-process public API interfaces for cases that require immediate feedback (e.g., stock reservation during order placement).

The goal is to demonstrate the discipline and rigor of microservices architecture — bounded contexts, event-driven communication, independent data ownership — without the operational overhead of distributed deployments.

```
┌────────────────────────────────────────────────────────────────────┐
│                          ASP.NET Core API                          │
├──────────┬──────────┬──────────┬──────────┬──────────┬────────────┤
│  Users   │ Catalog  │ Launches │  Orders  │ Payments │   Notif.   │
├──────────┴──────────┴──────────┴──────────┴──────────┴────────────┤
│           Shared Building Blocks (Domain, Application,             │
│                   Infrastructure, Endpoints)                       │
├────────────────────────────────────────────────────────────────────┤
│  PostgreSQL (per-module schema)  │  Azure Service Bus              │
│  Azure Blob Storage              │  Keycloak (Identity)            │
│  Redis (Permissions Cache)       │  Serilog                        │
└────────────────────────────────────────────────────────────────────┘
```

Each module follows **Clean Architecture**:

```
Module.Domain          → Entities, Value Objects, Domain Events, Errors
Module.Application     → Commands, Queries, Handlers, Domain Event Handlers
Module.Infrastructure  → EF Core, Repositories, Outbox/Inbox, Background Jobs
Module.Endpoints       → Minimal API endpoints, Permissions
Module.Contracts       → Integration Events, Public API interface (IModulePublicApi)
```

---

## Key Technical Decisions

### Outbox / Inbox Pattern
Domain operations and event publishing are atomic. When a command commits its transaction, the integration event is written to an `OutboxMessages` table in the same transaction. A background processor then picks it up and publishes to Azure Service Bus. On the consumer side, an `InboxMessages` table ensures idempotent processing — each event is processed exactly once even if delivered multiple times.

### Permission-Based Authorization
Rather than relying solely on roles, the system uses fine-grained permissions (e.g., `catalog:create-product`, `launches:schedule`). Permissions are resolved per-request from the user's assigned roles, cached in Redis, and enforced via a custom `IAuthorizationHandler`. This gives full control over what each role can do without coupling business logic to auth primitives.

### Optimistic Concurrency for Stock Reservation
When a Launch goes live and hundreds of buyers hit the order endpoint simultaneously, all competing for the same stock counter, the system uses **PostgreSQL's `xmin` system column** as a row-level concurrency token via EF Core. If two transactions attempt to reserve stock from the same snapshot, only one commits — the other retries transparently. This avoids pessimistic locking and its associated contention under burst traffic.

### Module Contracts
Each module exposes a `Contracts` project containing its integration events and a public API interface (`IUsersPublicApi`, `ILaunchStockService`, etc.). Other modules depend only on this contract — never on the module's `Infrastructure` or `Application` layers. This enforces true isolation: swapping a module's internals or extracting it to a microservice requires no changes to its consumers.

### Account Activation Middleware
A custom middleware enforces that authenticated users must have activated their account before accessing any endpoint. Unactivated users receive a structured `403` with a redirect hint — the kind of cross-cutting concern that's clean to implement in a monolith and painful to coordinate across microservices.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10, ASP.NET Core Minimal APIs |
| ORM | Entity Framework Core 10 |
| Raw Queries | Dapper |
| Database | PostgreSQL 16 |
| Messaging | Azure Service Bus |
| Identity | Keycloak (JWT Bearer + OIDC) |
| Storage | Azure Blob Storage |
| Cache | Redis |
| Logging | Serilog |
| Testing | xUnit, Testcontainers, FluentAssertions |
