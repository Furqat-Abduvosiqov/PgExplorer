# PgExplorer Architecture Documentation

## Overview

PgExplorer uses **Clean Architecture** with **Vertical Slice Architecture** principles, creating a maintainable and scalable .NET application with Angular frontend.

> **Inspiration**: Based on [The Best Way to Structure Your .NET Projects](https://antondevtips.com/blog/the-best-way-to-structure-your-dotnet-projects-with-clean-architecture-and-vertical-slices) by Anton DevTips.

## Project Structure

```mermaid
graph TD
    ROOT[PgExplorer Solution]
    ROOT --> SRC[src/]
    ROOT --> TESTS[tests/]
    ROOT --> DOCS[docs/]

    SRC --> HOST[Pg.Explorer.Host]
    SRC --> FEATURES[Pg.Explorer.Features]
    SRC --> DOMAIN[Pg.Explorer.Domain]
    SRC --> INFRA[Pg.Explorer.Infrastructure]
    SRC --> SHARED[Pg.Explorer.Shared]
    SRC --> FRONT[pg-explorer-client]

    TESTS --> UNIT[Unit Tests]
    TESTS --> INTEG[Integration Tests]
```

## Architecture Layers

### 1. Host Layer (Entry Point)
**Project**: `Pg.Explorer.Host`

```mermaid
graph LR
    A[API Request] --> B[Host Layer]
    B --> C[Features]
    B --> D[Infrastructure]
    B --> E[Middleware Pipeline]
```

**Responsibilities:**
- Application startup & configuration
- Dependency injection setup
- API documentation (Swagger)
- Request/response handling

### 2. Features Layer (Business Logic)
**Project**: `Pg.Explorer.Features`

```mermaid
graph TD
    A[Request] --> B[Validator]
    B --> C[Command/Query]
    C --> D[Handler]
    D --> E[Repository Interface]
    D --> F[Business Service]
    D --> G[Response]
```

**Each feature contains:**
- Commands/Queries with MediatR
- Validators with FluentValidation
- API Endpoints
- Repository interfaces (in Shared folder)
- Mappings and services

### 3. Domain Layer (Pure Business)
**Project**: `Pg.Explorer.Domain`

```mermaid
graph TD
    A[Domain Layer] --> B[Entities]
    A --> C[Value Objects]
    A --> D[Business Rules]
    
    B --> E[Connection]
    B --> F[QueryEntity]
    C --> G[QueryType]
    C --> H[QueryStatus]
```

**Zero external dependencies** - pure business logic only.

### 4. Infrastructure Layer (Data & External Services)
**Project**: `Pg.Explorer.Infrastructure`

```mermaid
graph TD
    A[Infrastructure] --> B[Database Context]
    A --> C[Repository Implementations]
    A --> D[External Services]
    A --> E[Migrations]
    
    B --> F[EF Core + PostgreSQL]
    C --> G[Implements Feature Interfaces]
```

### 5. Shared Layer (Utilities)
**Project**: `Pg.Explorer.Shared`

```mermaid
graph LR
    A[Shared Layer] --> B[Encryption]
    A --> C[Pagination]
    A --> D[Middleware]
    A --> E[Endpoint Abstractions]
```

### 6. Frontend Layer (Angular)
**Project**: `pg-explorer-client`

```mermaid
graph TD
    A[Angular App] --> B[Components]
    A --> C[Services]
    A --> D[HTTP Client]
    A --> E[SSR Support]
```

## Key Innovation: Feature-Driven Repository Pattern

Traditional approach puts repository interfaces in Domain. We put them **inside each feature**:

```mermaid
graph LR
    subgraph "Feature: Connections"
        A[Create Connection Handler]
        B[Shared/IConnectionRepository]
    end
    
    subgraph "Infrastructure"
        C[ConnectionRepository Implementation]
    end
    
    A --> B
    B -.-> C
    C --> D[Database]
```

**Benefits:**
- Complete feature encapsulation
- Repository interfaces co-located with usage
- Better cohesion, easier refactoring

## Feature Structure Example

```mermaid
graph TD
    subgraph "Features/Connections"
        A[CreateConnection/]
        B[UpdateConnection/]
        C[DeleteConnection/]
        D[GetConnectionById/]
        E[GetConnectionList/]
        F[Shared/IConnectionRepository]
    end
    
    A --> F
    B --> F
    C --> F
    D --> F
    E --> F
```

## Dependency Flow

```mermaid
graph TD
    A[Host] --> B[Features]
    A --> C[Infrastructure]
    B --> D[Domain]
    B --> E[Shared]
    C --> B
    C --> D
    C --> E
    
    style A fill:#00bcd4
    style B fill:#9c27b0
    style C fill:#4caf50
    style D fill:#ff9800
    style E fill:#e91e63
```

**Key Rules:**
- Dependencies point inward
- Domain has ZERO external dependencies
- Features define their own data contracts
- Infrastructure implements feature interfaces

## Technology Stack

### Backend
- **.NET 8** - Framework
- **ASP.NET Core** - Web API
- **Entity Framework Core** - ORM
- **PostgreSQL** - Database
- **MediatR** - CQRS pattern
- **FluentValidation** - Input validation

### Frontend
- **Angular 17+** - SPA framework
- **TypeScript** - Type safety
- **Angular Universal** - SSR
- **Docker** - Containerization

## Adding a New Feature

1. **Create feature folder** in `Pg.Explorer.Features`
2. **Define repository interface** in `FeatureName/Shared/`
3. **Create request/response models**
4. **Implement handler** with business logic
5. **Add validation** rules
6. **Create API endpoint**
7. **Implement repository** in Infrastructure

### Example Code Structure

```csharp
// Features/NewFeature/Shared/INewFeatureRepository.cs
public interface INewFeatureRepository : IRepository<Entity, Guid>
{
    Task<Entity> GetByNameAsync(string name);
}

// Features/NewFeature/Create/CreateFeatureCommand.cs
public record CreateFeatureCommand(string Name) : IRequest<ErrorOr<FeatureResponse>>;

// Features/NewFeature/Create/CreateFeatureHandler.cs
public class CreateFeatureHandler : IRequestHandler<CreateFeatureCommand, ErrorOr<FeatureResponse>>
{
    private readonly INewFeatureRepository _repository;
    // Implementation
}

// Infrastructure/Repositories/NewFeatureRepository.cs
public class NewFeatureRepository : BaseRepository<Entity, Guid>, INewFeatureRepository
{
    // Implementation
}
```

## Architecture Benefits

### ✅ Developer Benefits
- **Clear structure** - easy to find code
- **Fast development** - everything co-located
- **Easy testing** - isolated components
- **Independent features** - parallel development

### ✅ Business Benefits
- **Maintainable** - localized changes
- **Flexible** - easy to extend
- **Reliable** - separation of concerns
- **Cost-effective** - faster development

## Key Architectural Principles

1. **Feature Ownership** - Each feature owns its complete contract
2. **Pure Domain** - Zero external dependencies
3. **Vertical Slices** - High cohesion within features
4. **Clean Dependencies** - Dependencies point inward
5. **Testability** - Easy to test in isolation

This architecture provides the perfect balance between structure and flexibility, making PgExplorer maintainable and scalable.
