# WebAPIWithEFCF - Workforce Management Web API (EF Core 9.0 Code First)

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4.svg?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-13.0-239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Web%20API-5C2D91.svg?style=for-the-badge&logo=dotnet&logoColor=white)
![EF Core](https://img.shields.io/badge/EF%20Core-9.0.2%20Code%20First-68217A.svg?style=for-the-badge&logo=dotnet&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2019%20%2F%202022%20%2F%20Express-CC292B.svg?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)
![Swagger](https://img.shields.io/badge/OpenAPI%20%2F%20Swagger-v1-85EA2D.svg?style=for-the-badge&logo=swagger&logoColor=black)
![Tests](https://img.shields.io/badge/Tests-27%20Passed%20(100%25)-brightgreen.svg?style=for-the-badge&logo=xunit&logoColor=white)
![Architecture](https://img.shields.io/badge/Architecture-Clean%20%2F%20Layered-blue.svg?style=for-the-badge)

A production-grade, enterprise **ASP.NET Core 9.0 Web API** demonstrating the full power of **Entity Framework Core 9.0 Code First (EF CF)** with Microsoft SQL Server. Built following **Clean Architecture** principles, featuring a complete repository and unit-of-work layer, FluentValidation, Global Exception Handling (RFC 7807 Problem Details), Swagger/OpenAPI documentation, soft deletes via EF Core Global Query Filters, optimistic concurrency tokens, database seeding, and an automated 27-test xUnit test suite.

---

## Table of Contents

- [Overview](#overview)
- [Architecture & Project Structure](#architecture--project-structure)
- [Entity Framework Core Code First Highlights](#entity-framework-core-code-first-highlights)
- [Database Schema & Relationships](#database-schema--relationships)
- [API Endpoints Reference](#api-endpoints-reference)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Database Configuration](#database-configuration)
  - [Running Database Migrations](#running-database-migrations)
  - [Running the Application](#running-the-application)
- [Running the Test Suite](#running-the-test-suite)
- [Security, Validation & Error Handling](#security-validation--error-handling)

---

## Overview

**WebAPIWithEFCF** showcases enterprise backend patterns utilizing **Code First** database design:
- Rather than manually authoring SQL scripts, domain classes define database tables, column types, precision, and constraints.
- EF Core Migrations automatically track schema changes, ensuring repeatable and zero-downtime database updates.
- Decoupled Clean Architecture ensures business rules in `Application` and entities in `Domain` remain completely independent of persistence infrastructure and external presentation layers.

---

## Architecture & Project Structure

The solution adheres to Clean / Layered Architecture:

```
WebAPIWithEFCF/
├── WebAPIWithEFCF.sln
├── src/
│   ├── WebAPIWithEFCF.Domain/              # Core domain entities, enums, exceptions
│   │   ├── Common/BaseEntity.cs            # Id, CreatedAtUtc, UpdatedAtUtc, IsDeleted, RowVersion
│   │   ├── Entities/                       # Employee, Department, Project, EmployeeProfile, EmployeeProject
│   │   ├── Enums/                          # EmploymentStatus, DepartmentType
│   │   └── Exceptions/                     # Domain & business rule exceptions
│   │
│   ├── WebAPIWithEFCF.Application/         # Business logic, DTOs, interfaces, validators
│   │   ├── Common/                         # PagedResult<T>, QueryParameters
│   │   ├── DTOs/                           # Create, Update, Response, Summary DTOs
│   │   ├── Interfaces/                     # IAsyncRepository, IUnitOfWork, IEmployeeService, etc.
│   │   ├── Services/                       # EmployeeService, DepartmentService, ProjectService, DashboardService
│   │   └── Validators/                     # FluentValidation rules
│   │
│   ├── WebAPIWithEFCF.Infrastructure/      # EF Core DbContext, Repositories, Migrations
│   │   ├── Data/                           # ApplicationDbContext, DbInitializer (Seed Data)
│   │   ├── Data/Configurations/            # Fluent API EntityTypeConfiguration mappings
│   │   ├── Data/Migrations/                # EF Core Code First generated migrations
│   │   └── Repositories/                   # AsyncRepository, EmployeeRepository, DepartmentRepository
│   │
│   └── WebAPIWithEFCF.API/                 # ASP.NET Core 9.0 Web API host
│       ├── Controllers/                    # EmployeesController, DepartmentsController, ProjectsController, DashboardController, HealthController
│       ├── Middleware/                     # ExceptionHandlingMiddleware (RFC 7807 ProblemDetails)
│       ├── appsettings.json                # Connection strings & configuration
│       └── Program.cs                      # Dependency injection, pipeline, Swagger setup
│
└── tests/
    ├── WebAPIWithEFCF.UnitTests/           # 15 Unit tests (Moq, FluentAssertions, Services, Validators)
    └── WebAPIWithEFCF.IntegrationTests/    # 12 Integration tests (InMemory DbContext, WebApplicationFactory API tests)
```

---

## Entity Framework Core Code First Highlights

1. **One-to-Many Relationship**:
   - `Department` has many `Employee` records (`FK_Employees_Departments_DepartmentId`).
   - Configured with `OnDelete(DeleteBehavior.Restrict)` to enforce referential integrity.
2. **One-to-One Relationship**:
   - `Employee` has an optional `EmployeeProfile` (`FK_EmployeeProfiles_Employees_EmployeeId`).
   - Configured with cascade delete.
3. **Many-to-Many Relationship**:
   - `Employee` and `Project` linked via `EmployeeProject` join entity.
   - Composite Primary Key `(EmployeeId, ProjectId)` with tracking metadata (`RoleOnProject`, `AllocationPercentage`, `AssignedDate`).
4. **Soft Delete & Global Query Filters**:
   - `IsDeleted` flag on `BaseEntity`.
   - Global query filters automatically applied in `ApplicationDbContext.OnModelCreating`:
     ```csharp
     modelBuilder.Entity<Employee>().HasQueryFilter(e => !e.IsDeleted);
     ```
   - Soft-deleted entities are transparently filtered from all standard queries.
5. **Optimistic Concurrency**:
   - `RowVersion` byte array configured with `.IsRowVersion()` (SQL Server `rowversion/timestamp`).
   - Prevents lost updates when multiple users edit concurrently.
6. **Automatic Audit Timestamps**:
   - `ApplicationDbContext.SaveChangesAsync` automatically stamps `CreatedAtUtc` on insert and `UpdatedAtUtc` on modification.
7. **Database Seeding**:
   - `DbInitializer.SeedAsync` populates 5 departments, 8 diverse employees with profiles, and 3 enterprise projects on first launch.

---

## API Endpoints Reference

| HTTP Method | Route | Description | Status Codes |
|---|---|---|---|
| `GET` | `/api/v1/health` | System health and runtime status | 200 |
| `GET` | `/api/v1/employees` | Paginated, filtered, sorted employees list | 200 |
| `GET` | `/api/v1/employees/{id}` | Full employee profile and project allocations | 200, 404 |
| `POST` | `/api/v1/employees` | Create a new employee with profile | 201, 400, 409 |
| `PUT` | `/api/v1/employees/{id}` | Update an existing employee | 200, 400, 404, 409 |
| `PATCH` | `/api/v1/employees/{id}/status` | Update employee employment status | 204, 404 |
| `DELETE` | `/api/v1/employees/{id}` | Soft delete an employee | 204, 404 |
| `POST` | `/api/v1/employees/{id}/projects` | Assign employee to a project | 204, 404 |
| `DELETE` | `/api/v1/employees/{id}/projects/{projectId}` | Remove employee from project | 204, 404 |
| `GET` | `/api/v1/departments` | All departments with headcount counts | 200 |
| `GET` | `/api/v1/departments/{id}` | Department details with employee roster | 200, 404 |
| `POST` | `/api/v1/departments` | Create a new department | 201, 400, 409 |
| `PUT` | `/api/v1/departments/{id}` | Update an existing department | 200, 400, 404 |
| `DELETE` | `/api/v1/departments/{id}` | Delete department (must have 0 employees) | 204, 404, 422 |
| `GET` | `/api/v1/projects` | All company projects with assigned counts | 200 |
| `POST` | `/api/v1/projects` | Create a new project | 201, 400 |
| `GET` | `/api/v1/dashboard/metrics` | Executive workforce KPIs, aggregates & salary spend | 200 |

---

## Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Microsoft SQL Server](https://www.microsoft.com/sql-server) (LocalDB, SQL Express, or standard SQL Server)
- EF Core CLI Tools: `dotnet tool install --global dotnet-ef`

### Database Configuration

In `src/WebAPIWithEFCF.API/appsettings.json`, set your SQL Server connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=WebAPIWithEFCF_DB;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True"
  },
  "UseInMemoryDatabase": false
}
```

### Running Database Migrations

Apply code-first migrations directly to your database:

```bash
dotnet ef database update --project src/WebAPIWithEFCF.Infrastructure --startup-project src/WebAPIWithEFCF.API
```

To create a new migration after modifying entities:

```bash
dotnet ef migrations add <MigrationName> --project src/WebAPIWithEFCF.Infrastructure --startup-project src/WebAPIWithEFCF.API -o Data/Migrations
```

### Running the Application

```bash
dotnet run --project src/WebAPIWithEFCF.API/WebAPIWithEFCF.API.csproj
```

Open your browser to explore interactive Swagger UI:
👉 **`http://localhost:5200/`** (or configured port)

---

## Running the Test Suite

Execute all 27 unit and integration tests:

```bash
dotnet test WebAPIWithEFCF.sln
```

### Test Coverage Summary:
- **Unit Tests (15 Tests)**:
  - `EmployeeServiceTests`: Pagination, detail retrieval, unique email checks, department validation, soft delete, project assignment.
  - `DepartmentServiceTests`: Roster retrieval, duplicate code checks, business rule protection against deleting non-empty departments.
  - `ValidatorTests`: Email formatting, salary bounds, department code regex, project budget constraints.
- **Integration Tests (12 Tests)**:
  - `EmployeeRepositoryTests`: Search term filtering, salary range filtering, soft-delete query filter verification, aggregate metric calculation.
  - `ApiEndpointsIntegrationTests`: Live HTTP endpoint requests (`/health`, `/employees`, `/departments`, `/dashboard/metrics`), 400 Bad Request on invalid models, 404 on missing entities, 201 on creation.

---

## Security, Validation & Error Handling

- **Global Exception Middleware**: Translates uncaught exceptions into standard **RFC 7807 `ProblemDetails`** responses.
- **FluentValidation**: Automatically intercepts invalid payloads before reaching controller actions, returning structured 400 validation error maps.
- **Concurrency Protection**: Detects conflicting updates via `RowVersion` and returns 409 Conflict.
- **Data Integrity**: Enforces unique indexes on `Email` and `Code` at both the database level and application service level.
