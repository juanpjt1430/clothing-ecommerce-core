# Guía Maestra de Arquitectura, Estándares de Código y DevOps Autónomo
**Proyecto:** Clothing E-Commerce Core  
**Stack Tecnológico:** .NET 8, C# 12, Entity Framework Core 8, SQL Server 2022 / PostgreSQL 16, Docker, Jira Cloud, GitHub.

---

## 1. Integraciones & Conectividad (Jira & GitHub)

### Variables de Entorno (Archivo `.env` en la raíz - Excluido de Git)
```env
JIRA_BASE_URL=https://juanpjt317.atlassian.net
JIRA_EMAIL=tu_correo@dominio.com
JIRA_API_TOKEN=tu_api_token
JIRA_PROJECT_KEY=PROYEC
JIRA_BOARD_ID=2
STORY_POINTS_FIELD=customfield_10016
```

### Transiciones de Estado en Jira
OpenCode interactúa con Jira usando la REST API v3 (`/rest/api/3/issue/{issueKey}/transitions`):

- **Inicio de tarea:** Mover a estado **En curso** (In Progress).
- **Finalización de tarea:** Mover a estado **Listo** (Done).

Las credenciales se leen estrictamente de variables de entorno o `.env`, nunca quemadas en código fuente rastreado.

## 2. Protocolo de Flujo de Trabajo Autónomo (Loop por Ticket)
Cada vez que el usuario solicite implementar un ticket (ej. PROYEC-X), el agente ejecutará estrictamente los siguientes 6 pasos:

```
[1. Jira: In Progress] ➡️ [2. Git: Sync & New Branch] ➡️ [3. Code & Patterns] 
         ⬇️
[6. Jira: Done]        ⬅️ [5. Merge to Main & Push]   ⬅️ [4. Build & Unit Tests]
```

1. **Estado Inicial:** Consultar la API de Jira y pasar PROYEC-X a "En curso".
2. **Preparación Git:**
   - Cambiar a main y sincronizar: `git checkout main && git pull origin main`.
   - Crear rama de feature: `git checkout -b feature/PROYEC-X-descripcion-kebab-case`.
3. **Desarrollo y Patrones:** Implementar la lógica cumpliendo las directrices arquitectónicas de la Sección 3.
4. **Verificación Estricta:**
   - Ejecutar `dotnet build` -> 0 Errores, 0 Advertencias.
   - Ejecutar `dotnet test` (si existen proyectos de prueba) -> 100% Tests pasados.
5. **Versionamiento & Merge:**
   - Smart Commit: `git commit -m "PROYEC-X feat: Descripción precisa del cambio"`.
   - Push de rama: `git push -u origin feature/PROYEC-X-descripcion-kebab-case`.
   - Fusionar a main: Integrar cambios a main y sincronizar con origin/main.
6. **Cierre Automático:** Consultar la API de Jira y pasar PROYEC-X a "Listo" (Done).

## 3. Arquitectura del Sistema (.NET 8 Clean Architecture)
El proyecto sigue las reglas estrictas de Inversión de Dependencias (las capas internas jamás dependen de las externas):

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
**Dependencias:** Cero dependencias externas o paquetes de terceros.

**Componentes:**
- **Entity<TId>:** Clase base con Id fuertemente tipado (Guid o int).
- **AuditableEntity:** Campos CreatedAtUtc, LastModifiedAtUtc, CreatedBy, LastModifiedBy.
- **ValueObject:** Clases inmutables con igualdad por valor (ej. Money, Address).
- **Exceptions:** Excepciones puras de dominio (ej. InsufficientStockException).
- **Interfaces:** Contratos de persistencia como IProductRepository, IUnitOfWork.

### Capa 2: ClothingEcommerce.Application
**Dependencias:** ClothingEcommerce.Domain, MediatR, FluentValidation.

**Patrón:** CQRS (Command Query Responsibility Segregation).

**Estructura por Feature:**
- `Features/Products/Commands/CreateProduct/` (CreateProductCommand, CreateProductCommandHandler, CreateProductValidator).
- `Features/Products/Queries/GetProductsPaged/` (GetProductsPagedQuery, GetProductsPagedQueryHandler, ProductDto).

**Reglas:**
- Los Handlers orquestan el caso de uso y devuelven tipos resultado o DTOs.
- Validación automática mediante Pipeline Behaviors de MediatR con FluentValidation.

### Capa 3: ClothingEcommerce.Infrastructure
**Dependencias:** ClothingEcommerce.Application, ClothingEcommerce.Domain, EF Core 8, Microsoft.EntityFrameworkCore.SqlServer.

**Persistencia:**
- ApplicationDbContext que implementa IUnitOfWork.
- Sobreescritura de SaveChangesAsync para inyectar fechas de auditoría automáticas en UTC.
- Mapeos de base de datos usando IEntityTypeConfiguration<T> con Fluent API (nunca Data Annotations en Domain).
- Inyección de Dependencias: Archivo DependencyInjection.cs con el método de extensión AddInfrastructureServices.

### Capa 4: ClothingEcommerce.API
**Dependencias:** ClothingEcommerce.Application, ClothingEcommerce.Infrastructure.

**Diseño RESTful:**
- Controladores delgados o Minimal APIs que únicamente delegan a ISender (MediatR).
- Versionamiento de API (/api/v1/...).
- Respuestas estándar según RFC 7807 (ProblemDetails) para errores 400, 401, 403, 404 y 500.
- Middleware Global: Captura centralizada de excepciones para evitar fugas de trazas internas en producción.

## 4. Entorno de Contenedores & Docker Compose
`docker-compose.yml` en la raíz expone la base de datos SQL Server 2022 (puerto 1433) con volúmenes persistentes.

Los archivos de configuración (`appsettings.Development.json`) deben apuntar al servidor local de Docker:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=ClothingEcommerceDb;User Id=sa;Password=Your_Strong_Password123!;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

## 5. Convenciones de Commits y Buenas Prácticas Git
Formato Smart Commit: `<ISSUE_KEY> <tipo>: <descripción en presente>`

- Ejemplo: `PROYEC-12 feat: Implementa ApplicationDbContext y configuracion de Docker Compose`
- Ejemplo: `PROYEC-13 ci: Agrega pipeline de GitHub Actions para validacion de PRs`

**Limpieza de ramas:** No dejar ramas locales huérfanas tras completar un merge a main.
