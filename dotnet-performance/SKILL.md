---
name: dotnet-performance
version: 1.0.0
description: "Use for performance-sensitive C#/.NET code involving async pipelines, concurrency, streaming, memory, allocations, or throughput."
author: mag1art
license: Apache-2.0
tags: [dotnet, csharp, performance, async, concurrency, channels, streaming, memory, allocation]
triggers:
  - performance
  - throughput
  - latency
  - async pipeline
  - Channel
  - Parallel.ForEachAsync
  - ArrayPool
  - streaming
  - allocation
metadata:
  hermes:
    tags: [dotnet, csharp, performance, async, concurrency, channels, streaming, memory]
---

# .NET Performance

## When to Use

Use for measured performance work, high-throughput services, bounded worker pipelines, large payloads, and allocation-sensitive paths.

## When Not to Use

Do not optimize without a workload, baseline, or acceptance metric. Do not trade correctness and operability for speculative micro-optimizations.

## Workflow

1. Define throughput, latency, memory, concurrency, and correctness targets.
2. Profile or benchmark the current implementation.
3. Identify the bottleneck: I/O, CPU, allocations, locks, database, serialization, or dependency.
4. Make the smallest change and benchmark again.
5. Test cancellation, errors, backpressure, ordering, duplicates, and shutdown.

## Rules

- Keep async all the way; never use Result, Wait, or unbounded fire-and-forget.
- Bound concurrency with Channel, SemaphoreSlim, or Parallel.ForEachAsync.
- Use bounded channels and explicit full-mode behavior for backpressure.
- Stream large files and responses; avoid unbounded ToList, ReadToEnd, or buffering.
- Pass CancellationToken through every I/O path.
- Use ArrayPool only with strict ownership and finally-return; clear sensitive buffers.
- Prefer database-side filtering, projection, pagination, and batch operations.
- Measure serialization and logging overhead on hot paths.
- Preserve ordering only when the contract requires it.
- Make retries bounded and cancellation-aware.

## Quality Gate

Provide before/after measurements, resource assumptions, failure behavior, and a rollback-safe change.