---
name: dotnet-expert
version: 2.1.0
author: mag1art
license: Apache License 2.0
description: "Use when building or reviewing modern .NET 10/C# 14 applications: ASP.NET Core Controller APIs, Minimal APIs, EF Core 10, service/repository layers, OpenAPI, validation, auth, performance, or production-quality C# backend code."
triggers:
  - .NET
  - .NET 10
  - dotnet
  - C#
  - C# 14
  - ASP.NET Core
  - Controller
  - Web API
  - Minimal API
  - OpenAPI
  - Swagger
  - Entity Framework
  - EF Core
role: specialist
scope: implementation
output-format: code
metadata:
  hermes:
    tags: [dotnet, csharp, aspnetcore, efcore, backend, openapi, performance, quality]
---

# .NET Expert

## Overview

Act as a senior .NET engineer. Prefer **.NET 10 LTS**, **C# 14**, ASP.NET Core 10, EF Core 10, nullable-safe code, clear architecture, production observability, and testable business logic. Produce real implementation code, not placeholders.

Default assumptions unless the repository says otherwise:

- Target framework: `net10.0`.
- SDK pinned with `global.json` when reproducibility matters.
- Nullable reference types and implicit usings are enabled.
- Analyzers, formatting, and warnings-as-errors are expected for application code.
- **Controller-Service-Repository** is the default enterprise API style unless the project already uses Minimal APIs or vertical slices.
- Minimal APIs are valid for small services, infra endpoints, fast prototypes, or highly focused modules, but do not assume they are the dominant style.

## When to Use

- Implementing or reviewing C#/.NET backend code.
- Designing ASP.NET Core APIs, controllers, filters, middleware, DI, options, auth, and background services.
- Writing EF Core models, migrations, repositories, queries, transactions, and performance fixes.
- Defining OpenAPI contracts, response schemas, errors, auth schemes, and client-generation compatibility.
- Improving code quality, tests, security, observability, memory usage, or deployment readiness.

Do **not** force CQRS/MediatR/DDD into tiny CRUD apps. Keep architecture proportional to domain complexity.

## .NET 10 Baseline

```xml
<!-- Directory.Build.props -->
<Project>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <AnalysisMode>Recommended</AnalysisMode>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  </PropertyGroup>
</Project>
```

```json
// global.json
{
  "sdk": { "version": "10.0.202", "rollForward": "latestFeature" }
}
```

Use C# 14 features when they make code clearer, not to be clever:

- extension members/properties for cohesive helper APIs;
- field-backed properties for validation around auto-property storage;
- null-conditional assignment for simple optional updates;
- collection expressions, records, pattern matching, primary constructors where they reduce noise.

## Quality Rules

1. **Business failures return results, not exceptions.** Exceptions are for infrastructure, programmer errors, and unexpected failures.
2. **Async all the way.** Never use `.Result`, `.Wait()`, sync-over-async, or unsupervised fire-and-forget.
3. **CancellationToken everywhere** on I/O paths: controller/endpoint → service → repository → EF/HTTP/file calls.
4. **Nullable-safe domain.** No `!` unless unavoidable and justified; validate at boundaries.
5. **Explicit contracts.** DTOs for API boundaries; never return EF entities directly.
6. **Thin controllers.** Controllers handle HTTP mapping, auth attributes, model state, status codes; business logic lives in services/domain.
7. **Useful repositories.** Repositories hide persistence complexity when useful; avoid generic wrappers that remove EF Core value.
8. **Observability built in.** Structured logs, metrics where useful, health checks, trace correlation.
9. **Secure by default.** No secrets in source/logs; validate authorization per operation.
10. **Performance by design.** Avoid N+1, over-fetching, needless allocations, and unbounded queries.
11. **Tests prove behavior.** Unit-test domain/services; integration-test API + database boundaries.

## Project Shapes

### Default: Controller-Service-Repository

Use this for conventional ASP.NET Core Web APIs, enterprise CRUD/business apps, and teams expecting familiar separation.

```text
src/
  Api/
    Controllers/
    Contracts/
    Filters/
    Middleware/
    Program.cs
  Application/
    Services/
    Validation/
    Mapping/
  Domain/
    Entities/
    ValueObjects/
    Errors/
  Infrastructure/
    Persistence/
      AppDbContext.cs
      Configurations/
      Repositories/
    Auth/
    ExternalServices/
tests/
  UnitTests/
  IntegrationTests/
```

Layer rules:

- `Api` depends on `Application` contracts/services.
- `Application` contains use-case orchestration and interfaces for persistence/external dependencies.
- `Domain` contains entities, value objects, invariants, domain errors.
- `Infrastructure` implements persistence, HTTP clients, auth providers, file/storage adapters.
- Avoid circular dependencies. If in doubt: dependencies point inward.

### Modular Monolith / Feature Modules

Use modules when bounded contexts matter:

```text
src/
  Api/
  Modules/
    Users/
      Users.Api/             # controllers/endpoints for module, optional
      Users.Application/     # services/use cases/contracts
      Users.Domain/          # entities/value objects
      Users.Infrastructure/  # EF configs/repos/adapters
  Shared/
    SharedKernel/
    Infrastructure/
```

For small services, collapse projects but keep namespaces/folders clean.

## ASP.NET Core Controller API Pattern

Prefer controllers as the default style unless the repo clearly uses Minimal API.

```csharp
[ApiController]
[Route("api/users")]
[Produces("application/json")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _users;

    public UsersController(IUserService users) => _users = users;

    [HttpGet("{id:guid}", Name = "GetUserById")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _users.GetByIdAsync(id, ct);

        return result.Match<ActionResult<UserResponse>>(
            Ok,
            error => error.Type == ErrorType.NotFound
                ? NotFound(ToProblemDetails(error))
                : ProblemFrom(error));
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserResponse>> Create(
        [FromBody] CreateUserRequest request,
        CancellationToken ct)
    {
        var result = await _users.CreateAsync(request, ct);

        return result.Match<ActionResult<UserResponse>>(
            user => CreatedAtAction(nameof(GetById), new { id = user.Id }, user),
            error => ProblemFrom(error));
    }
}
```

Controller guidelines:

- Use `[ApiController]` for automatic binding/validation behavior.
- Use route constraints: `{id:guid}`, `{slug:minlength(3)}`, etc.
- Use `ActionResult<T>` or `IActionResult` consistently.
- Keep controller methods short: parse HTTP inputs, call service, map result to HTTP.
- Prefer `CreatedAtAction`, `NoContent`, `NotFound`, `Conflict`, `Forbid`, `ValidationProblem` over ad-hoc response objects.
- Add `ProducesResponseType`/OpenAPI metadata for every meaningful response.
- Do not inject `DbContext` into controllers for business endpoints; use service/repository/use-case layer.

## Service Layer Pattern

Services orchestrate use cases, transactions, domain rules, repository calls, and external adapters.

```csharp
public interface IUserService
{
    Task<Result<UserResponse>> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Result<UserResponse>> CreateAsync(CreateUserRequest request, CancellationToken ct);
}

public sealed class UserService(IUserRepository users, IPasswordHasher hasher, TimeProvider clock)
    : IUserService
{
    public async Task<Result<UserResponse>> CreateAsync(CreateUserRequest request, CancellationToken ct)
    {
        if (await users.EmailExistsAsync(request.Email, ct))
            return Result<UserResponse>.Fail(UserErrors.DuplicateEmail);

        var user = User.Create(request.Email, request.Name, hasher.Hash(request.Password), clock.GetUtcNow());
        await users.AddAsync(user, ct);
        await users.SaveChangesAsync(ct);
        return Result<UserResponse>.Ok(UserResponse.From(user));
    }

    public async Task<Result<UserResponse>> GetByIdAsync(Guid id, CancellationToken ct) =>
        await users.GetByIdAsync(id, ct) is { } user
            ? Result<UserResponse>.Ok(UserResponse.From(user))
            : Result<UserResponse>.Fail(UserErrors.NotFound(id));
}
```

Service rules:

- No HTTP types in services (`HttpContext`, `ActionResult`, `ControllerBase`) except explicit web abstractions.
- Return domain/application results, not status codes.
- Keep transactions at service/use-case level when multiple repository calls must be atomic.
- Use `TimeProvider` or clock abstraction for testable time.
- Log business milestones with stable IDs, not PII/secrets.

## Repository Pattern with EF Core

Use repositories when they clarify persistence boundaries. Avoid a generic repository over every entity if it just wraps `DbSet<T>` poorly.

```csharp
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct);
    Task<IReadOnlyList<UserResponse>> SearchAsync(UserSearchQuery query, CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct);
}

public sealed class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<bool> EmailExistsAsync(string email, CancellationToken ct) =>
        db.Users.AnyAsync(u => u.Email == email, ct);

    public async Task<IReadOnlyList<UserResponse>> SearchAsync(UserSearchQuery query, CancellationToken ct) =>
        await db.Users.AsNoTracking()
            .Where(u => query.Search == null || u.Name.Contains(query.Search))
            .OrderBy(u => u.Name)
            .Take(Math.Clamp(query.PageSize, 1, 100))
            .Select(u => new UserResponse(u.Id, u.Name, u.Email))
            .ToListAsync(ct);

    public Task AddAsync(User user, CancellationToken ct) => db.Users.AddAsync(user, ct).AsTask();
    public Task<int> SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
```

Repository rules:

- Use `AsNoTracking()` for read-only queries.
- Project to DTOs for list/read models where domain behavior is not needed.
- Keep query methods intention-revealing: `FindActiveByEmailAsync`, not `Get(Expression<Func<T,bool>>)` everywhere.
- Expose `IQueryable` only inside infrastructure; do not leak it across application boundaries.
- Use compiled queries only for proven hot paths.

## Minimal API Pattern

Minimal APIs are a secondary option. Use them for lean services, module-local endpoints, health/internal APIs, or when the repo already uses endpoint groups.

```csharp
public static class UserEndpoints
{
    public static RouteGroupBuilder MapUsers(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .RequireAuthorization();

        group.MapGet("/{id:guid}", GetById)
            .WithName("GetUserById")
            .Produces<UserResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    private static async Task<Results<Ok<UserResponse>, NotFound, ProblemHttpResult>> GetById(
        Guid id,
        IUserService users,
        CancellationToken ct)
    {
        var result = await users.GetByIdAsync(id, ct);
        return result.Match<Results<Ok<UserResponse>, NotFound, ProblemHttpResult>>(
            TypedResults.Ok,
            error => error.Type == ErrorType.NotFound
                ? TypedResults.NotFound()
                : TypedResults.Problem(ToProblemDetails(error)));
    }
}
```

Minimal API rules:

- Group endpoints by feature: `MapUsers`, `MapOrders`.
- Use typed results where practical.
- Keep handler delegates short; call services/use cases for business logic.
- Add `.WithName`, `.WithTags`, `.Produces*`, `.RequireAuthorization` for OpenAPI clarity.
- Do not mix controller and Minimal API styles randomly in the same feature.

## Program.cs Setup

Register only what the app uses, validate config at startup, and keep middleware order explicit.

```csharp
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddOptions<JwtOptions>().BindConfiguration("Jwt").ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("Database")));
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddAuthentication().AddJwtBearer();
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();
app.MapOpenApi();
app.MapControllers();
app.MapHealthChecks("/health/live");
```

Setup checklist:

- Config via `IOptions<T>` with `ValidateOnStart()`.
- One consistent exception handler for unexpected failures.
- Authentication before authorization; policies for operations.
- Health checks for DB/external dependencies; separate liveness/readiness where deployed.
- CORS/rate limiting only when deliberately configured.

## OpenAPI / Swagger Knowledge

OpenAPI is a contract, not just a UI. Keep it accurate enough for generated clients and integration tests.

For .NET 10, prefer built-in OpenAPI services when enough:

```csharp
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Info.Title = "Users API";
        document.Info.Version = "v1";
        return Task.CompletedTask;
    });
});

app.MapOpenApi();
```

Use Swashbuckle/NSwag when the project needs Swagger UI, advanced filters, client generation, or established tooling.

OpenAPI rules:

- Every endpoint should declare stable operation IDs/names.
- Declare all success and error responses: 200/201/204, 400, 401, 403, 404, 409, 422, 500 as applicable.
- Use `ProblemDetails` / `ValidationProblemDetails` for errors consistently.
- Document auth schemes: Bearer JWT, OAuth2/OIDC, API key, cookie auth.
- Version APIs intentionally: URL segment (`/api/v1`) or header/media type; do not improvise per controller.
- Avoid exposing internal DTOs, EF entities, stack traces, or implementation-only fields.
- Prefer stable enum serialization policy and document it.
- For file uploads/downloads, explicitly document content types, size limits, and response codes.
- If generated clients are used, avoid ambiguous schemas and duplicate type names.

Controller OpenAPI hints:

```csharp
[HttpGet("{id:guid}", Name = "GetUserById")]
[ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public async Task<ActionResult<UserResponse>> GetById(Guid id, CancellationToken ct) { ... }
```

Minimal API OpenAPI hints:

```csharp
group.MapPost("/", Create)
    .WithName("CreateUser")
    .WithSummary("Creates a user")
    .Accepts<CreateUserRequest>("application/json")
    .Produces<UserResponse>(StatusCodes.Status201Created)
    .ProducesValidationProblem()
    .ProducesProblem(StatusCodes.Status409Conflict);
```

## Result + Error Pattern

Keep it tiny and predictable:

```csharp
public readonly record struct Result<T>(T? Value, Error? Error)
{
    public bool IsSuccess => Error is null;
    public static Result<T> Ok(T value) => new(value, null);
    public static Result<T> Fail(Error error) => new(default, error);

    public TResult Match<TResult>(Func<T, TResult> ok, Func<Error, TResult> fail) =>
        IsSuccess ? ok(Value!) : fail(Error!);
}

public sealed record Error(string Code, string Message, ErrorType Type);
public enum ErrorType { Validation, NotFound, Conflict, Forbidden, Failure }
```

Guidelines:

- Validation/business errors: `Result`, `ValidationProblem`, `ProblemDetails`.
- Infrastructure errors: log with context, translate at boundary, do not leak internals.
- Error codes should be stable (`User.DuplicateEmail`) and testable.
- Keep error-to-HTTP mapping centralized.

## EF Core 10 Rules

- Use migrations as source of truth; generate idempotent scripts or migration bundles for production.
- `AsNoTracking()` for reads unless updating entities.
- Project to DTOs in SQL; avoid loading full graphs for responses.
- Always paginate list endpoints and cap `PageSize`.
- Use transactions for multi-aggregate writes; use optimistic concurrency where conflicts matter.
- Configure models with `IEntityTypeConfiguration<T>`; avoid giant `OnModelCreating`.
- Use named query filters in EF Core 10 when combining soft-delete/tenant filters.
- Use EF Core 10 JSON/vector features only when the selected database/provider supports them.
- Avoid lazy-loading in APIs unless deliberately chosen and measured.
- Prefer database constraints for uniqueness and integrity; do not rely only on app checks.

```csharp
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Email).HasMaxLength(320).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.Name).HasMaxLength(100).IsRequired();
        builder.Property(u => u.RowVersion).IsRowVersion();
    }
}
```

Migration commands:

```bash
dotnet ef migrations add AddUsers -p src/Infrastructure -s src/Api
dotnet ef database update -p src/Infrastructure -s src/Api
dotnet ef migrations script --idempotent -p src/Infrastructure -s src/Api -o artifacts/migrations.sql
```

## Validation

Validate at boundaries. FluentValidation is fine; built-in validation is fine if the repo uses it. Keep validators deterministic and side-effect free.

```csharp
public sealed class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(12).MaximumLength(256);
    }
}
```

Validation rules:

- Request shape validation belongs at API/application boundary.
- Domain invariants belong inside entities/value objects as well.
- Async validators must receive `CancellationToken` and avoid hidden side effects.
- Do not use regex-heavy validators without limits/timeouts when input is user-controlled.

## Auth, Security, Observability

- Prefer policy-based authorization over scattered role string checks.
- Validate authorization at operation/resource level: “can edit this order”, not only “is admin”.
- Store secrets in environment/secret manager, never in appsettings committed to source.
- Do not log passwords, tokens, cookies, API keys, refresh tokens, or PII.
- Hash passwords with proven framework services; never write custom crypto.
- Protect mass assignment: bind request DTOs, not domain entities.
- Use structured logs: `LogInformation("Processed order {OrderId}", orderId)`.
- Include correlation/request IDs.
- Add health checks for databases, queues, external APIs.
- Use OpenTelemetry when tracing across services matters.

## Performance and Memory

General rules:

- Measure before complex optimization.
- Avoid repeated LINQ allocations in hot loops.
- Use `Span<T>`, `Memory<T>`, `ReadOnlySpan<T>` for parsing/slicing hot paths.
- Prefer streaming for large files/responses instead of loading entire payloads into memory.
- Avoid unbounded `ToListAsync()`, `ReadToEndAsync()`, or buffering user-controlled data.
- Use `HttpClientFactory`; do not create/dispose raw `HttpClient` per request.

### ArrayPool<T> / pooled arrays

Use `ArrayPool<T>` for temporary large buffers or hot-path allocations. Do **not** use it for normal small arrays or long-lived storage.

```csharp
byte[] buffer = ArrayPool<byte>.Shared.Rent(64 * 1024);
try
{
    int read = await source.ReadAsync(buffer.AsMemory(0, buffer.Length), ct);
    await destination.WriteAsync(buffer.AsMemory(0, read), ct);
}
finally
{
    ArrayPool<byte>.Shared.Return(buffer, clearArray: false);
}
```

ArrayPool rules:

- Always return rented arrays in `finally`.
- Only use the slice length you actually wrote/read; rented arrays may be larger than requested.
- Set `clearArray: true` before returning buffers containing secrets, tokens, PII, or sensitive payloads.
- Never store rented arrays in fields, caches, async state beyond a tightly controlled operation, or returned DTOs.
- Do not return the same buffer twice.
- Do not use a buffer after returning it.
- Prefer `MemoryPool<T>`/`IMemoryOwner<T>` when ownership/lifetime must be explicit.

## Testing Standard

Minimum expectations:

- Unit tests for domain behavior and pure services.
- Integration tests for controllers/endpoints, auth policies, persistence, migrations, and serialization.
- Use Testcontainers or real local services when database/provider behavior matters; avoid fake EF providers for SQL-specific logic.
- Tests should assert errors/status codes/contracts, not private implementation details.
- Test OpenAPI generation if clients depend on it.

```bash
dotnet format --verify-no-changes
dotnet build --no-restore
dotnet test --no-build --collect:"XPlat Code Coverage"
```

## Review Checklist

- [ ] Targets .NET 10 / C# 14 intentionally; no stale .NET 6/7/8 assumptions.
- [ ] Nullable, analyzers, formatting, and warnings are clean.
- [ ] Public API contracts are explicit and OpenAPI-visible.
- [ ] Controller-Service-Repository style is followed where expected by the repo.
- [ ] Minimal API is used only where it matches project style or scope.
- [ ] All I/O is async and receives `CancellationToken`.
- [ ] No secrets, PII, stack traces, or tokens leak into logs/responses.
- [ ] EF queries are bounded, projected, and free of obvious N+1 issues.
- [ ] Business rules live in domain/services/use cases, not controllers.
- [ ] AuthN/AuthZ is enforced per operation/resource.
- [ ] Integration tests cover the real database/provider when behavior depends on it.
- [ ] ArrayPool usage, if any, returns buffers safely and clears sensitive data.
- [ ] Build/test/format commands were run or explicitly reported as not run.

## Common Pitfalls

1. Over-architecting CRUD: use simple controllers/services/repositories until complexity justifies CQRS/DDD.
2. Returning EF entities from API responses: leaks schema and tracking concerns.
3. Putting business rules in controllers: controllers should map HTTP, not own domain logic.
4. Treating FluentValidation as the domain model: domain invariants still belong in domain code.
5. Generic repository everywhere: often hides EF Core strengths without adding value.
6. Swallowing exceptions or logging without correlation/context.
7. Using `DateTime.Now`: prefer `TimeProvider`/UTC abstractions.
8. Unbounded `ToListAsync()` on user-controlled queries.
9. Hidden sync I/O, blocking calls, or missing cancellation in libraries.
10. Blind AutoMapper use for simple mappings; explicit mapping is often clearer.
11. Applying migrations automatically on app startup in production without an approved deployment path.
12. Inaccurate OpenAPI: undocumented errors break generated clients and consumers.
13. Misusing `ArrayPool<T>`: using returned buffers, leaking sensitive data, or returning twice.
14. Using new .NET/C# features without checking repo SDK, CI image, and deployment runtime.
