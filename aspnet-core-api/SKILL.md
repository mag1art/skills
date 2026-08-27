---
name: aspnet-core-api
version: 1.0.0
description: "Use for designing, implementing, or reviewing production ASP.NET Core HTTP APIs."
author: mag1art
license: Apache-2.0
tags: [aspnetcore, web-api, http, openapi, authentication, authorization, validation]
triggers:
  - ASP.NET Core API
  - controller
  - Minimal API
  - endpoint
  - middleware
  - ProblemDetails
  - OpenAPI
metadata:
  hermes:
    tags: [aspnetcore, web-api, http, openapi, auth, validation]
---

# ASP.NET Core API

## When to Use

Use for HTTP API contracts, controllers, Minimal APIs, middleware, authentication, authorization, validation, error handling, and OpenAPI.

## When Not to Use

Do not use for frontend-only work, generic C# code, or persistence-only changes without an API boundary.

## Workflow

1. Inspect the target framework, API style, routing, serialization, auth, and error contract.
2. Preserve established conventions unless the user requests a migration.
3. Define DTOs and explicit status codes before changing handlers.
4. Keep business behavior in application services or use cases, not controllers.
5. Add validation, authorization, cancellation, logging, and OpenAPI metadata.
6. Add endpoint tests for success, validation, auth, not-found, conflict, and failure paths.
7. Run formatting, build, tests, and API contract checks.

## API Rules

- Pass CancellationToken from endpoint to every I/O operation.
- Use DTOs at the HTTP boundary; never expose EF entities directly.
- Use route constraints and stable operation IDs.
- Return ProblemDetails or ValidationProblemDetails consistently.
- Map domain/application errors centrally to HTTP status codes.
- Use CreatedAtAction or a Location header for successful resource creation.
- Paginate collection endpoints and cap page size.
- Avoid unbounded collections and internal exception messages.
- Apply authorization per operation and resource.
- Keep middleware order explicit: exception handling, security, authentication, authorization, endpoint mapping.
- Configure JSON enum, date, null, and naming policies intentionally.

## Security Gate

Check authentication scheme, authorization policy, object-level access, CORS, request-size limits, rate limiting, secret handling, and sensitive logging.

## Quality Gate

Verify build/tests, actual OpenAPI schemas, cancellation, authorization, bounded reads, and safe structured logging.