---
name: dotnet-observability
version: 1.0.0
description: "Use for production logging, tracing, metrics, health checks, and diagnostics in C#/.NET services."
author: mag1art
license: Apache-2.0
tags: [dotnet, observability, logging, serilog, opentelemetry, metrics, tracing, health-checks]
triggers:
  - Serilog
  - OpenTelemetry
  - Prometheus
  - ELK
  - Seq
  - metrics
  - tracing
  - health check
  - correlation ID
metadata:
  hermes:
    tags: [dotnet, observability, serilog, opentelemetry, metrics, tracing, health]
---

# .NET Observability

## When to Use

Use for logs, metrics, distributed traces, health endpoints, correlation, diagnostics, and production incident analysis.

## When Not to Use

Do not add telemetry blindly or log sensitive data merely to increase volume.

## Logging

- Use structured templates with stable property names, not interpolated message strings.
- Log business milestones, durations, outcomes, retry count, and stable entity IDs.
- Never log passwords, tokens, cookies, connection strings, full request bodies, or unnecessary PII.
- Separate operational errors from expected business failures.
- Configure sinks and retention for the actual environment.
- Avoid duplicate exception logging at multiple boundaries.

## Tracing

- Use OpenTelemetry for cross-service HTTP, database, messaging, and background-work traces.
- Propagate W3C trace context across HTTP and message headers.
- Add low-cardinality useful tags such as service, operation, outcome, and dependency.
- Do not put unbounded values or sensitive data into metric labels.

## Metrics and Health

Use counters for totals, histograms for duration/size/latency, and gauges only for current state. Define units and names consistently. Separate liveness from readiness; protect detailed health output.

## Quality Gate

Verify correlation across logs/traces, useful dashboards and alerts, redaction, cardinality, sampling, health semantics, and graceful telemetry shutdown.
## Example: OpenTelemetry Setup

```csharp
builder.Logging.AddJsonConsole();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(builder.Environment.ApplicationName))
    .WithMetrics(m => m
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation())
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddOtlpExporter());
```

Use stable metric names and low-cardinality labels. Add request duration and dependency failure metrics, but never use user IDs, URLs with unbounded values, tokens, or full exception text as metric labels.

