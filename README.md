# Clothing E-Commerce Core

API REST profesional de e-commerce de ropa construida con **.NET 8**, **Clean Architecture** y principios SOLID.

## Stack Tecnológico

| Tecnología | Versión | Uso |
|-----------|---------|-----|
| .NET | 8.0 (C# 12) | Runtime y compilación |
| Entity Framework Core | 8.0 | ORM y migraciones |
| SQL Server | 2022 | Base de datos (Docker) |
| MediatR | Latest | CQRS (Commands/Queries) |
| FluentValidation | Latest | Validación de dominio |
| xUnit + FluentAssertions | Latest | Pruebas unitarias |
| GitHub Actions | CI/CD | Build y tests automáticos |

## Arquitectura (Clean Architecture)

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation (API)                        │
│    Controllers / Minimal APIs, Middleware, DI                │
└──────────────┬───────────────────────────────┬──────────────┘
               │                               │
               ▼                               ▼
┌──────────────────────────────┐ ┌────────────────────────────┐
│      Application (Core)      │ │   Infrastructure (External)│
│  Commands, Queries (MediatR) │ │   EF Core DbContext,       │
│  DTOs, Mappings, Validators  │ │   Repositories, Auth/JWT   │
│  (FluentValidation)          │ │   External Services        │
└──────────────┬───────────────┘ └─────────────┬──────────────┘
               │                               │
               └───────────────┬───────────────┘
                               ▼
                ┌──────────────────────────────┐
                │         Domain (Core)        │
                │   Entities, Value Objects,   │
                │   Aggregates, Domain Events, │
                │   Repository Interfaces      │
                └──────────────────────────────┘
```

### Estructura de Capas

| Capa | Proyecto | Responsabilidad |
|------|----------|-----------------|
| **Domain** | `ClothingEcommerce.Domain` | Entidades, Value Objects, excepciones e interfaces de repositorio. Cero dependencias externas. |
| **Application** | `ClothingEcommerce.Application` | Casos de uso CQRS (Commands/Queries), DTOs, validadores FluentValidation. |
| **Infrastructure** | `ClothingEcommerce.Infrastructure` | Persistencia EF Core, repositorios, servicios externos, DI. |
| **API** | `ClothingEcommerce.API` | Endpoints RESTful delgados, middleware de excepciones (RFC 7807). |
| **Tests** | `ClothingEcommerce.UnitTests` | Pruebas unitarias con xUnit, FluentAssertions y Moq. |

## Requisitos Previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com)
- [GitHub CLI](https://cli.github.com) (para PRs)

## Comandos de Compilación

```bash
# Restaurar dependencias
dotnet restore

# Compilar (0 errores, 0 warnings requerido)
dotnet build --configuration Release

# Ejecutar pruebas
dotnet test --configuration Release --verbosity normal
```

## Docker (SQL Server 2022)

```bash
# Levantar contenedor
docker-compose up -d

# Detener contenedor
docker-compose down

# Detener y eliminar volúmenes
docker-compose down -v
```

### ConnectionString (appsettings.Development.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=ClothingEcommerceDb;User Id=sa;Password=Your_Strong_Password123!;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

## Migraciones EF Core

```bash
dotnet ef migrations add <NombreMigracion> --project src/ClothingEcommerce.Infrastructure --startup-project src/ClothingEcommerce.API
dotnet ef database update --project src/ClothingEcommerce.Infrastructure --startup-project src/ClothingEcommerce.API
```

## Flujo de DevOps (por Ticket)

Cada ticket (PROYEC-X) sigue un ciclo autónomo de 6 pasos:

```
[1. Jira: In Progress] → [2. Git: Branch] → [3. Code & Patterns]
         ↓
[6. Jira: Done]       ← [5. PR & Merge]   ← [4. Build & Tests]
```

1. Mover ticket a **En curso** en Jira
2. Crear rama `feature/PROYEC-X-descripcion` desde `main`
3. Implementar siguiendo Clean Architecture + pruebas unitarias
4. Validar: `dotnet build` (0 errores/warnings) y `dotnet test` (100%)
5. Commit, push, `gh pr create`, `gh pr merge --delete-branch`
6. Mover ticket a **Listo** en Jira

## Convenciones

- **Commits:** `<ISSUE_KEY> <tipo>: <descripción>` (ej: `PROYEC-12 feat: Implementa DbContext`)
- **Pruebas:** Estructura Arrange/Act/Assert, nomenclatura `Metodo_Resultado_Condicion`
- **Errores de negocio:** Patrón `Result<T>` (no excepciones)
- **Validaciones:** FluentValidation → HTTP 400 con RFC 7807 ProblemDetails

## CI/CD (GitHub Actions)

El workflow `.github/workflows/ci.yml` ejecuta automáticamente en cada push/PR a `main`:

1. `dotnet restore`
2. `dotnet build --configuration Release --no-restore`
3. `dotnet test --configuration Release --no-build`

## Licencia

Este proyecto es privado. Todos los derechos reservados.
