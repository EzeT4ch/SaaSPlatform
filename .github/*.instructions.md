# 🧭 YouEcommerce Architecture Guidelines

## 🎯 Purpose

These guidelines exist to ensure that **all generated or manually written code** respects the architecture and design principles of this solution.

Copilot and developers must follow these rules strictly to maintain consistency, quality, and scalability.

---

## 🧱 Architecture Overview

This project follows a **modular monolith architecture** using:

- **Domain-Driven Design (DDD)**  
- **Clean Architecture**  
- **CQRS (Command Query Responsibility Segregation)**  
- **Feature-based modularity** (Auth, Billing, WMS, Notifications, Integrations)

Each module is self-contained and can later be extracted into a microservice.

---

## 🧩 Core Design Rules

### 1. Layered Boundaries

Each module follows this structure:

/Application
/Commands
/Queries
/Validators
/Abstractions
/Domain
/Entities
/Events
/Enums
/Specifications
/Infrastructure
/Database
/Repositories
/Transactions
/Security
/Presentation
/Controllers


### 2. Allowed Dependencies

| Layer | Can depend on | Must NOT depend on |
|--------|----------------|--------------------|
| Domain | Nothing (pure) | Application, Infrastructure |
| Application | Domain | Infrastructure, Presentation |
| Infrastructure | Application, Domain | Presentation |
| Presentation | Application | Domain, Infrastructure (directly) |

Copilot should **never** suggest code that violates these dependency rules.

---

## 🧩 Commands & Queries

- **Commands** (`ICommandHandler`) modify state.  
  - Examples: `RegisterUserCommand`, `RegisterTenantAdminCommand`, `LoginCommand`.

- **Queries** (`IQueryHandler`) read state only.  
  - Examples: `GetUserByEmailQuery`, `GetTenantListQuery`.

Rules:
- Commands must be **idempotent** and return `Result<T>`.
- Queries must be **read-only**, no domain events or `SaveChangesAsync()` calls.
- Handlers should be **thin**, orchestrating domain entities and services.

---

## 🧱 Domain Model Rules

- Domain entities live in `/Domain/Entities`.
- Domain events live in `/Domain/Events`.
- Domain entities **should not** reference infrastructure or persistence code.
- Each entity must:
  - Encapsulate its invariants (use private setters).
  - Raise domain events for side effects.
  - Be persistence-agnostic (no EF annotations, no base DbContext classes).

---

## 🧩 Infrastructure Rules

- Use **EF Core** only within Infrastructure.
- `DbContext` is abstracted via `IApplicationDbContext`.
- Use **repositories** to mediate between domain and database.
- Use **UnitOfWork** to manage transactions and domain event dispatching.
- Use **Polly** for retries and resilience in transactional code.

---

## 🧱 API Layer (Presentation)

- Expose endpoints via Controllers in `/Presentation/Controllers`.
- Each endpoint:
  - Should call a single Command or Query via `ISender` (MediatR or custom bus).
  - Should **not** contain business logic or DB access.
  - Should validate input with FluentValidation.
- Responses use `Result<T>` (success or failure) with proper HTTP codes.

---

## 🧩 Eventing

- Domain events (e.g., `UserRegisteredEvent`, `TenantCreatedEvent`) are raised in domain entities.
- Handlers for domain events live in `/Application/DomainEventHandlers`.
- Event handlers:
  - Should be side-effecting (e.g., send email, enqueue message).
  - Must not modify core domain state directly (avoid recursion).

---

## 🧱 Testing Standards

- **Unit Tests:** for domain and handlers.
- **Integration Tests:** for repositories, persistence, and transaction flow.
- **Functional Tests:** for API endpoints (Controllers).

Use:
- `xUnit` + `Moq` + `FluentAssertions` for unit tests.
- `WebApplicationFactory<T>` for endpoint tests.

---

## 🧩 Coding Style

- Follow **Clean Code principles**:
  - Methods under 30 lines.
  - Single responsibility per class.
  - Avoid static helpers unless pure.
  - Prefer composition over inheritance.
- Use `Result<T>` for all command outcomes.
- Prefer **constructors over property injection**.

---

## 🚫 Prohibited Patterns

❌ Do not:
- Inject `DbContext` directly into Handlers or Controllers.  
- Perform queries inside Commands (CQRS separation).  
- Raise domain events from outside Entities.  
- Access Infrastructure code from Application or Domain.  
- Write business logic in Controllers.  
- Use `async void` methods.  
- Depend on static state (except constants).

---

## 🧩 Example Flow (User Registration)

Controller
↓
RegisterUserCommand
↓
Validator
↓
RegisterUserCommandHandler
↓
User (Domain Entity)
↓
Repository + UnitOfWork
↓
DbContext (Infrastructure)

---

## 🧠 Copilot Behavior Instructions

When suggesting code:
- Always respect **layered architecture boundaries**.
- Prefer **Commands** for writes and **Queries** for reads.
- Use **dependency injection** for services.
- Never suggest direct EF queries in Application layer.
- Use **domain events** to trigger side effects, not direct calls.
- Avoid large methods or God classes — prefer splitting by behavior.
- Include `CancellationToken` in async methods.
- Use `Result<T>` instead of exceptions for predictable flows.

---

## 📚 References

- **Domain-Driven Design (Evans, 2004)**
- **Clean Architecture (Robert C. Martin, 2018)**
- **Enterprise Application Patterns (Fowler, 2002)**
- **CQRS and Event Sourcing (Greg Young)**

---

## ✅ Summary

This system is built around **clarity, separation of concerns, and testability**.

Copilot and developers should ensure:
- Handlers are small and use explicit dependencies.
- Domain is pure and free of infrastructure details.
- Every change respects DDD boundaries and CQRS principles.
- No architectural leakage between layers.
