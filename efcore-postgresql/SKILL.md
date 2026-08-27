---
name: efcore-postgresql
version: 1.0.0
description: "Use for EF Core and PostgreSQL data access, query tuning, migrations, concurrency, and persistence reviews."
author: mag1art
license: Apache-2.0
tags: [efcore, entity-framework, postgresql, sql, migrations, database, query-performance]
triggers:
  - EF Core
  - PostgreSQL
  - Npgsql
  - DbContext
  - migration
  - LINQ query
  - SQL query performance
metadata:
  hermes:
    tags: [efcore, postgresql, npgsql, migrations, sql, performance]
---

# EF Core and PostgreSQL

## When to Use

Use for entities, configurations, DbContext, repositories, LINQ queries, migrations, transactions, concurrency, indexes, and PostgreSQL-specific behavior.

## When Not to Use

Do not use for non-.NET data access, database administration unrelated to application code, or API-only changes with no persistence impact.

## Workflow

1. Inspect provider, EF Core version, naming conventions, migrations, nullability, and DbContext lifetime.
2. Identify whether the operation is a read, tracked update, bulk operation, migration, or transaction.
3. Write the smallest intention-revealing query and inspect generated SQL for important paths.
4. Add constraints and indexes in the database model, not only in application checks.
5. Test provider-specific behavior against PostgreSQL, not an in-memory fake.
6. Run migrations, build, and integration tests.

## Query Rules

- Use AsNoTracking for read-only queries.
- Project to DTOs instead of loading complete graphs.
- Avoid lazy loading and accidental N+1 queries.
- Apply deterministic ordering and bounded pagination.
- Prefer keyset pagination for large tables.
- Do not expose IQueryable outside the persistence boundary.
- Use ExecuteUpdate and ExecuteDelete deliberately; they bypass change tracking.
- Pass CancellationToken to every async database call.
- Treat SqlNullValueException as a nullability/model mismatch.

## PostgreSQL Rules

- Use PostgreSQL types intentionally: timestamptz, jsonb, arrays, UUID, and numeric precision.
- Use UTC instants and map them consistently.
- Use unique and partial indexes when business rules require them.
- Use optimistic concurrency explicitly. With Npgsql, xmin can be a concurrency token; SQL Server rowversion is not portable.
- Parameterize values and never concatenate user input into SQL.
- Use EXPLAIN (ANALYZE, BUFFERS) for measured query investigations.
- Keep migrations reviewable and generate idempotent production scripts or bundles.

## Transactions and Reliability

Keep transaction boundaries at the use-case level when multiple writes must be atomic. Consider an outbox for database-plus-message consistency. Handle unique-constraint and serialization failures as expected conflicts where appropriate.

## Quality Gate

Check SQL shape, indexes, nullability, tracking mode, transaction scope, cancellation, migration safety, and real-PostgreSQL integration coverage.
## Example: Bounded Read and Concurrency

```csharp
var orders = await db.Orders
    .AsNoTracking()
    .Where(x => x.CustomerId == customerId && x.Id > afterId)
    .OrderBy(x => x.Id)
    .Select(x => new OrderListItem(x.Id, x.Status, x.CreatedAt))
    .Take(Math.Clamp(pageSize, 1, 100))
    .ToListAsync(ct);

var changed = await db.Orders
    .Where(x => x.Id == id && x.Version == expectedVersion)
    .ExecuteUpdateAsync(setters => setters
        .SetProperty(x => x.Status, OrderStatus.Paid)
        .SetProperty(x => x.UpdatedAt, DateTimeOffset.UtcNow), ct);

if (changed == 0)
    throw new ConcurrencyException("The order was changed or deleted.");
```

The first query uses keyset pagination and projection. ExecuteUpdate bypasses change tracking, so do not expect an already tracked entity to be updated in memory. Use a transaction when several writes must be atomic.

