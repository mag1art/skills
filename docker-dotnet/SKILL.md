---
name: docker-dotnet
version: 1.0.0
description: "Use for containerizing, configuring, running, and reviewing C#/.NET services with Docker and Compose."
author: mag1art
license: Apache-2.0
tags: [dotnet, docker, containers, compose, deployment, health-checks, security]
triggers:
  - Dockerfile for .NET
  - docker compose .NET
  - containerize ASP.NET
  - Docker health check
  - .NET container
metadata:
  hermes:
    tags: [dotnet, docker, containers, compose, deployment, security]
---

# Docker for .NET

## When to Use

Use for Dockerfiles, Compose files, local dependencies, image size, runtime configuration, and container readiness.

## When Not to Use

Do not change deployment infrastructure blindly or bake secrets, development credentials, or environment-specific endpoints into images.

## Rules

- Use a multi-stage build and keep the final image runtime-only.
- Pin base-image choices intentionally and update them reviewably.
- Run as non-root where the application and base image support it.
- Keep configuration in environment variables or secret providers.
- Never copy source secrets, local certificates, env files, or build artifacts into images.
- Expose only required ports and configure health/readiness endpoints.
- Handle SIGTERM and stop gracefully.
- Use a proper dockerignore.
- Keep Compose networks, volumes, and dependency readiness explicit.
- Scan images and review base-image vulnerabilities.

## Verification

Build the image, run it with production-like configuration, verify health and logs, test graceful shutdown, and confirm that no secrets or unnecessary SDK files are present.