# Guía Maestra de Arquitectura, Estándares de Código y DevOps Autónomo

**Proyecto:** Clothing E-Commerce Core  
**Stack Tecnológico:** .NET 8 (C# 12), Clean Architecture, EF Core 8, SQL Server 2022 / Docker, MediatR, FluentValidation, xUnit, FluentAssertions, Jira Cloud & GitHub CLI.

---

## 1. Integraciones & Conectividad (Jira & GitHub)

### Variables de Entorno (Archivo `.env` en la raíz - Protegido por `.gitignore`)
```env
JIRA_BASE_URL=https://juanpjt317.atlassian.net
JIRA_EMAIL=tu_correo@dominio.com
JIRA_API_TOKEN=tu_api_token
JIRA_PROJECT_KEY=PROYEC
JIRA_BOARD_ID=2
STORY_POINTS_FIELD=customfield_10016
```

### Transiciones de Estado en Jira
El agente interactúa con Jira vía REST API v3 (`/rest/api/3/issue/{issueKey}/transitions`):

- **Inicio de tarea:** Mover a estado **En curso** (In Progress).
- **Finalización de tarea:** Mover a estado **Listo** (Done).

Las credenciales se leen estrictamente de variables de entorno o `.env`.

## 2. Protocolo de Flujo de Trabajo Autónomo (Loop por Ticket)
Cada vez que el usuario ordene ejecutar un ticket (ej. PROYEC-X), se ejecutará obligatoriamente la siguiente secuencia de 6 pasos:

```
[1. Jira: In Progress] ➡️ [2. Git: Sync & New Branch] ➡️ [3. Code & Patterns] 
         ⬇️
[6. Jira: Done]        ⬅️ [5. GitHub PR & Merge]     ⬅️ [4. Build & Unit Tests]
```

1. **Jira (In Progress):** Consultar la API y pasar PROYEC-X a "En curso".
2. **Git (Branch):**
   - Cambiar a main y sincronizar: `git checkout main && git pull origin main`.
   - Crear rama de feature: `git checkout -b feature/PROYEC-X-descripcion-kebab-case`.
3. **Desarrollo y Patrones:** Implementar la lógica cumpliendo las directrices de la Sección 3 y añadir pruebas unitarias.
4. **Verificación Estricta (Candado de Calidad):**
   - Ejecutar `dotnet build` → 0 Errores, 0 Advertencias.
   - Ejecutar `dotnet test` → 100% Tests pasados.
5. **GitHub (Commit, Push, Pull Request & Merge):**
   - Smart Commit: `git commit -m "PROYEC-X feat: Descripción precisa del cambio"`.
   - Push de rama: `git push -u origin feature/PROYEC-X-descripcion-kebab-case`.
   - Crear Pull Request formal: `gh pr create --title "PROYEC-X feat: Descripción precisa" --body "Closes PROYEC-X" --base main`.
   - Fusionar PR en GitHub: `gh pr merge --merge --delete-branch`.
   - Sincronizar local: `git checkout main && git pull origin main`.
6. **Jira (Done):** Pasar PROYEC-X a estado "Listo" (Done) vía API de Jira.

## 3. Arquitectura del Sistema (.NET 8 Clean Architecture)

```
┌─────────────────────────────────────────────────────────────┐
│                       Presentation (API)                    │
│   Controllers / Minimal APIs, Middlewares, Dependency Injection│
└──────────────┬───────────────────────────────┬──────────────┘
               │                               │
               ▼                               ▼
┌──────────────────────────────┐ ┌────────────────────────────┐
│      Application (Core)      │ │   Infrastructure (External)│
│  Commands, Queries (MediatR),│ │   EF Core DbContext,       │
│  DTOs, Mappings, Validators  │ │   Repositories, Auth/JWT,  │
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

### Capa 1: ClothingEcommerce.Domain
**Dependencias:** Cero dependencias externas.

**Componentes:**
- **Entity<TId> / BaseEntity:** Clase base con Id fuertemente tipado (Guid o int).
- **AuditableEntity:** Campos CreatedAtUtc, LastModifiedAtUtc, CreatedBy, LastModifiedBy.
- **ValueObject:** Clases inmutables con igualdad por valor (ej. Money, Address).
- **Exceptions:** Excepciones puras de dominio.
- **Interfaces:** Contratos abstractos (IProductRepository, IUnitOfWork).

### Capa 2: ClothingEcommerce.Application
**Dependencias:** Domain, MediatR, FluentValidation.

**Patrón:** CQRS por carpetas de características (Features/FeatureName/Commands/ y Features/FeatureName/Queries/).

**Reglas:**
- Los Handlers orquestan el caso de uso y retornan Result/DTOs.
- Pipeline Behavior de MediatR para validación automática antes de ejecutar Handlers.

### Capa 3: ClothingEcommerce.Infrastructure
**Dependencias:** Application, Domain, EF Core 8, Microsoft.EntityFrameworkCore.SqlServer.

**Persistencia:**
- ApplicationDbContext implementa IUnitOfWork y sobreescribe SaveChangesAsync para inyectar fechas de auditoría en UTC.
- Mapeos exclusivamente con Fluent API (IEntityTypeConfiguration<T>).
- Inyección de dependencias centralizada en DependencyInjection.cs (AddInfrastructureServices).

### Capa 4: ClothingEcommerce.API
**Dependencias:** Application, Infrastructure.

**Diseño:**
- Endpoints RESTful delgados que delegan a ISender (MediatR).
- Versionamiento de API (/api/v1/...).
- Middleware global de excepciones con estándar RFC 7807 (ProblemDetails).

### Capa 5: tests/ClothingEcommerce.UnitTests
**Frameworks:** xUnit, FluentAssertions, Moq.

Pruebas de reglas de negocio, validaciones y comportamiento de entidades.

## 4. Entorno de Contenedores & Docker Compose
`docker-compose.yml` en la raíz con SQL Server 2022 (mcr.microsoft.com/mssql/server:2022-latest), puerto 1433 y volúmenes persistentes.

`appsettings.Development.json` configurado hacia el contenedor:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=ClothingEcommerceDb;User Id=sa;Password=Your_Strong_Password123!;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

## 5. Convenciones de Commits y Buenas Prácticas Git
Formato Smart Commit: `<ISSUE_KEY> <tipo>: <descripción en presente>`

- Ejemplo: `PROYEC-12 feat: Implementa ApplicationDbContext y configuracion de Docker Compose`
- Ejemplo: `PROYEC-13 ci: Agrega pipeline de GitHub Actions y tests unitarios`

**Limpieza:** Ramas eliminadas automáticamente tras el merge a main.
