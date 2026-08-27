---
name: dotnet-version-guide
version: 1.0.0
description: "Use when choosing, upgrading, or maintaining a specific .NET Core/.NET and C# version, or when explaining version-specific APIs and migration risks."
author: mag1art
license: Apache-2.0
tags: [dotnet, dotnet-core, csharp, versioning, migration, compatibility, sdk, target-framework]
triggers:
  - .NET version
  - .NET Core version
  - upgrade .NET
  - migrate .NET
  - target framework
  - netcoreapp
  - netstandard
  - LangVersion
  - breaking changes
metadata:
  hermes:
    tags: [dotnet, dotnet-core, csharp, versioning, migration, compatibility]
---

# .NET Version Guide

## When to Use

Use when explaining differences between .NET Core 3.1, .NET 5, .NET 6, .NET 7, .NET 8, .NET 9, and .NET 10; selecting a target framework; applying version-specific features; or planning an upgrade.

## When Not to Use

Do not recommend an upgrade from version numbers alone. Inspect the repository, deployment runtime, package compatibility, operating systems, database providers, and support requirements first.

## Version Map

| Runtime | C# | Release profile | Practical focus |
|---|---:|---|---|
| .NET Core 3.1 | 8 | Legacy LTS | Maintenance only; plan migration |
| .NET 5 | 9 | Legacy STS | Unified .NET line; do not start new work |
| .NET 6 | 10 | Legacy LTS | Minimal APIs, hot reload, source generation, mature microservices baseline |
| .NET 7 | 11 | Legacy STS | Performance, Native AOT foundations, rate limiting, generic math, container publishing |
| .NET 8 | 12 | LTS | Blazor Web App, improved Native AOT, time abstraction, modern production baseline |
| .NET 9 | 13 | STS | Cloud-native tooling, Aspire, AI abstractions, OpenAPI, performance |
| .NET 10 | 14 | LTS | Current LTS baseline, runtime/library improvements, C# 14, ASP.NET Core and EF Core updates |

Support status changes. Check the official support policy before setting a production baseline:
https://dotnet.microsoft.com/platform/support/policy/dotnet-core

## Key Changes

### .NET Core 3.1

- Maintain existing applications; do not create new services on it.
- Expect ASP.NET Core 3.1 hosting, Startup-based composition, endpoint routing, and System.Text.Json conventions.
- Check dependencies before migration because later hosting and configuration patterns differ.

### .NET 5

- The separate Core naming ended for the unified runtime line: target net5.0 rather than netcoreapp5.0.
- C# 9 introduced records, init-only setters, top-level statements, and improved pattern matching.
- Single-file publishing and trimming were available but required compatibility testing.

### .NET 6

- LTS release with minimal APIs, hot reload, and C# 10 global usings and file-scoped namespaces.
- System.Text.Json source generation and IAsyncEnumerable serialization became practical production tools.
- Use the new SDK templates as a reference, but do not assume upgrading rewrites existing code.

### .NET 7

- Added generic math, Regex source generators, non-backtracking regular expressions, rate limiting APIs, and stronger Native AOT foundations.
- The SDK added built-in container publishing and central package management.
- Treat Native AOT, trimming, and reflection-heavy libraries as an explicit compatibility project.

### .NET 8

- LTS release and the preferred baseline for applications that cannot yet move to .NET 10.
- Blazor Web Apps introduced the unified web-app model with static SSR and interactive render modes.
- C# 12 added primary constructors and collection expressions.
- Native AOT and ASP.NET Core support matured; verify reflection, serializers, DI registrations, and deployment assumptions.

### .NET 9

- Focused on cloud-native development and performance.
- Added Aspire improvements, built-in OpenAPI generation, static asset fingerprinting, AI/vector-data abstractions, JSON schema support, CountBy, and AggregateBy.
- C# 13 added params collections, the new Lock type, partial properties, and ref-struct improvements.

### .NET 10

- Current LTS line with C# 14.
- Runtime and libraries improve JIT, Native AOT, JSON, cryptography, networking, diagnostics, and PipeReader scenarios.
- C# 14 adds field-backed properties, extension blocks, null-conditional assignment, and additional operator/lambda improvements.
- ASP.NET Core adds Blazor, OpenAPI, form-validation, diagnostics, memory-pool, and passkey improvements.
- EF Core 10 adds named query filters and other LINQ/performance improvements.

Use the official version pages for detailed APIs and breaking changes:
- https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10/overview
- https://learn.microsoft.com/dotnet/core/whats-new/dotnet-9/overview
- https://learn.microsoft.com/dotnet/core/whats-new/dotnet-8/overview
- https://learn.microsoft.com/dotnet/core/whats-new/dotnet-7
- https://learn.microsoft.com/dotnet/core/whats-new/dotnet-6
- https://learn.microsoft.com/dotnet/core/whats-new/dotnet-5
- https://learn.microsoft.com/dotnet/core/whats-new/dotnet-core-3-1

## Applying a Feature Safely

1. Read TargetFramework, global.json, LangVersion, package versions, container image, and deployment runtime.
2. Confirm the API belongs to the target runtime and not only to a newer SDK or package.
3. Check breaking changes for every skipped runtime version.
4. Prefer the existing project style when backporting a feature.
5. For multi-targeting, use TargetFrameworks and small conditional sections such as NET8_0_OR_GREATER.
6. Keep Microsoft.Extensions, ASP.NET Core, EF Core, and provider major versions aligned with the target runtime.
7. Build and test every target framework in CI.
8. Test publishing, trimming, Native AOT, containers, and OS-specific behavior separately when applicable.

## Upgrade Checklist

- Record current runtime, SDK, packages, hosting model, and deployment image.
- Upgrade one major version at a time when the application is old or heavily coupled.
- Read official breaking-change pages and provider migration notes.
- Update global.json, project files, Docker images, CI images, and deployment manifests together.
- Run restore, format/analyzers, build, unit tests, integration tests, contract tests, and smoke tests.
- Compare startup, memory, throughput, database behavior, serialization, and logs.
- Document intentional behavior changes and rollback steps.

## Response Contract

When answering a version question, state the detected target and SDK, requested target, immediately available features, required target/package changes, breaking risks, exact project/CI/container changes, and validation commands.
## Example: Target Framework and SDK Pinning

A project chooses its runtime and compiler independently but should keep them compatible:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <LangVersion>12.0</LangVersion>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
```

```json
{
  "sdk": {
    "version": "8.0.4xx",
    "rollForward": "latestFeature"
  }
}
```

During an upgrade, change the target framework, SDK image, CI runtime, package versions, deployment runtime, and tests together. Verify analyzers, serializers, EF providers, native dependencies, and hosting behavior before enabling new language features.

