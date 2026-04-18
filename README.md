# 🌊 Shopwave

A multi-vendor e-commerce marketplace platform built with .NET 8, following
Clean Architecture, SOLID principles, and the Mediator pattern from scratch.

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Git](https://git-scm.com/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) _(Phase 8)_
- IDE: [Rider](https://www.jetbrains.com/rider/) or [VS Code](https://code.visualstudio.com/) with C# Dev Kit

---

## Scaffold the Solution

### Mac / Linux
```bash
chmod +x scaffold.sh
./scaffold.sh
```

### Windows (PowerShell)
```powershell
.\scaffold.ps1
```

## Wire (reference) up the project

### Mac / Linux
```bash
chmod +x wire.sh
./wire.sh
```

## Then verify everything builds:
```bash
dotnet build
```

---

## Solution Structure

```
Shopwave/
│
├── src/
│   ├── API/
│   │   └── Shopwave.API                            → ASP.NET Core entry point
│   │
│   ├── Shared/
│   │   └── Shopwave.Shared                         → Shared Kernel (contracts, base classes)
│   │
│   └── Modules/
│       ├── Identity/                               → Auth, registration, JWT, 2FA
│       │   ├── Shopwave.Modules.Identity.Domain
│       │   ├── Shopwave.Modules.Identity.Application
│       │   └── Shopwave.Modules.Identity.Infrastructure
│       │
│       ├── Catalog/                                → Products, categories, search
│       │   ├── Shopwave.Modules.Catalog.Domain
│       │   ├── Shopwave.Modules.Catalog.Application
│       │   └── Shopwave.Modules.Catalog.Infrastructure
│       │
│       ├── Inventory/                              → Stock levels, availability
│       │   ├── Shopwave.Modules.Inventory.Domain
│       │   ├── Shopwave.Modules.Inventory.Application
│       │   └── Shopwave.Modules.Inventory.Infrastructure
│       │
│       ├── Orders/                                 → Cart, checkout, order lifecycle
│       │   ├── Shopwave.Modules.Orders.Domain
│       │   ├── Shopwave.Modules.Orders.Application
│       │   └── Shopwave.Modules.Orders.Infrastructure
│       │
│       ├── Payments/                               → Payment processing, payouts, refunds
│       │   ├── Shopwave.Modules.Payments.Domain
│       │   ├── Shopwave.Modules.Payments.Application
│       │   └── Shopwave.Modules.Payments.Infrastructure
│       │
│       ├── Stores/                                 → Seller profiles, store setup
│       │   ├── Shopwave.Modules.Stores.Domain
│       │   ├── Shopwave.Modules.Stores.Application
│       │   └── Shopwave.Modules.Stores.Infrastructure
│       │
│       ├── Notifications/
│       │   └── Shopwave.Modules.Notifications      → Emails, alerts (single project)
│       │
│       └── Analytics/
│           └── Shopwave.Modules.Analytics          → Metrics, GMV, reports (single project)
│
└── tests/
    ├── Shopwave.Modules.Identity.Tests
    ├── Shopwave.Modules.Catalog.Tests
    ├── Shopwave.Modules.Inventory.Tests
    ├── Shopwave.Modules.Orders.Tests
    └── Shopwave.Modules.Payments.Tests
```

---

## Architecture

```
┌─────────────────────────────────────────┐
│              Shopwave.API               │  HTTP endpoints (minimal controllers)
├─────────────────────────────────────────┤
│     Shopwave.Modules.*.Application      │  Use cases, commands, queries, handlers
├─────────────────────────────────────────┤
│       Shopwave.Modules.*.Domain         │  Entities, value objects, domain events
├─────────────────────────────────────────┤
│   Shopwave.Modules.*.Infrastructure     │  EF Core, external APIs, email, storage
└─────────────────────────────────────────┘
         ↑ Dependencies point inward only
```

**Golden Rule:** The Domain layer knows nothing about the database.
The Application layer knows nothing about HTTP.
Each layer is independently testable.

---

## Key Design Decisions

| Decision | Rationale |
|---|---|
| Modular Monolith | Self-contained modules. Can extract to microservices later without rewrites. |
---

## Learning Roadmap

| Phase | Status | Focus |
|---|---|---|
| 1 — Requirements | ✅ Done | User stories, acceptance criteria, bounded contexts |
| 2 — Solution Structure | ✅ Done | Clean Architecture, modular scaffold |
| 3 — Git Strategy | ⬜ Next | Branching model, commit conventions |
| 4 — Domain & Application | ⬜ | Entities, value objects, use cases, custom mediator |
| 5 — Infrastructure | ⬜ | EF Core, repositories, migrations |
| 6 — API Layer | ⬜ | Minimal APIs, versioning, auth middleware |
| 7 — Testing | ⬜ | xUnit, Moq, TestContainers |
| 8 — Docker | ⬜ | Dockerfile, docker-compose |
| 9 — CI/CD | ⬜ | GitHub Actions pipelines |
| 10 — Deployment | ⬜ | Cloud hosting, release strategy |