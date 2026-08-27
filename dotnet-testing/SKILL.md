---
name: dotnet-testing
version: 1.0.0
description: "Use for designing, implementing, or reviewing automated tests for C#/.NET services and microservices."
author: mag1art
license: Apache-2.0
tags: [dotnet, csharp, testing, xunit, integration-tests, testcontainers, contract-tests]
triggers:
  - unit test
  - integration test
  - xUnit
  - WebApplicationFactory
  - Testcontainers
  - contract test
  - test coverage
metadata:
  hermes:
    tags: [dotnet, csharp, testing, xunit, integration, testcontainers, contracts]
---

# .NET Testing

## When to Use

Use for unit, integration, API, persistence, messaging, contract, regression, and end-to-end test design.

## When Not to Use

Do not add tests that only duplicate implementation details or use fake providers where real provider behavior matters.

## Strategy

- Unit-test pure domain rules and deterministic application services.
- Integration-test HTTP, authentication, serialization, persistence, migrations, and external boundaries.
- Use WebApplicationFactory for the API composition root.
- Use Testcontainers for PostgreSQL, RabbitMQ, or other dependencies when provider behavior matters.
- Use contract tests for independently deployed service boundaries.
- Keep tests isolated, deterministic, and safe to run in parallel.
- Assert observable behavior, status codes, error contracts, state transitions, and emitted messages.
- Control time, randomness, IDs, and retries through injectable abstractions.

## Cases

Cover success, validation, not-found, conflict, authorization, cancellation, timeout, duplicate delivery, retry, and infrastructure failure where relevant. Avoid sleeps; await readiness or poll with a bounded timeout.

## Quality Gate

Run formatting, build, the smallest relevant test scope, then the full suite. Report commands and failures accurately.

~~~bash
dotnet format --verify-no-changes
dotnet build --no-restore
dotnet test --no-build
~~~