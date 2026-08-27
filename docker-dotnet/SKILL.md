---
name: docker-dotnet
version: 1.1.0
description: "Use for containerizing, configuring, running, and reviewing C#/.NET services with Docker and Compose, including reproducible builds, local development, health checks, and production hardening."
author: mag1art
license: Apache-2.0
tags: [dotnet, csharp, docker, containers, compose, buildkit, deployment, health-checks, security]
triggers:
  - Dockerfile for .NET
  - docker compose .NET
  - containerize ASP.NET Core
  - Docker health check
  - .NET container
  - multi-stage Dockerfile
  - Docker development environment
metadata:
  hermes:
    tags: [dotnet, csharp, docker, containers, buildkit, deployment, security]
---

# Docker for .NET

## When to Use

Use for Dockerfiles, Compose files, local dependencies, image size, runtime configuration, container readiness, CI image builds, and production hardening of C#/.NET applications.

Use the skill for ASP.NET Core, worker services, console applications, background consumers, and multi-project solutions. Adapt the examples to the repository's project names, target framework, ports, dependencies, and deployment platform.

## When Not to Use

Do not bake secrets, local certificates, development credentials, or environment-specific endpoints into images. Do not assume that a container restart means the application is ready. Do not run database migrations independently from every application replica without an explicit migration strategy.

## Container Strategy

- Use a Dockerfile when custom OS packages, development stages, multi-project build control, private feeds, native dependencies, or a custom runtime layout are required.
- Consider dotnet publish -t:PublishContainer for a simple application when the SDK-generated image provides enough control.
- Use SDK images only for restore, build, test, and publish stages. Use aspnet for ASP.NET Core and runtime for non-web applications.
- Match the major version of SDK and runtime images to the target framework: net8.0 normally uses sdk:8.0 and aspnet:8.0.
- Choose Linux or Windows images deliberately. They are not interchangeable.
- Pin production base images by digest when supply-chain reproducibility is required, while keeping the readable tag in the reviewed change.

## Build Context and Layout

For a multi-project solution, build from the repository root:

```text
.
├── Directory.Build.props
├── Directory.Packages.props
├── NuGet.config
├── Orders.sln
├── Dockerfile
├── .dockerignore
├── compose.yaml
├── src/Orders.Api/Orders.Api.csproj
└── tests/Orders.Api.Tests/Orders.Api.Tests.csproj
```

The final argument of docker build and build.context in Compose determine what COPY can access. A Dockerfile under src/Orders.Api may still need the repository root as its build context.

The COPY lines for Directory.Build.props, Directory.Packages.props, and NuGet.config are examples. Remove any line for a file that does not exist, and add the project files for every referenced project required by restore.

## Production Dockerfile

This example is for an ASP.NET Core API in src/Orders.Api targeting .NET 8, 9, or 10. Change DOTNET_VERSION and the DLL name for the actual project.

```dockerfile
# syntax=docker/dockerfile:1.7

ARG DOTNET_VERSION=10.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS restore
WORKDIR /src

# Copy stable dependency metadata first for restore-layer reuse.
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["NuGet.config", "./"]
COPY ["src/Orders.Api/Orders.Api.csproj", "src/Orders.Api/"]

RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore "src/Orders.Api/Orders.Api.csproj" \
    --configfile NuGet.config

FROM restore AS publish
COPY . .
WORKDIR /src/src/Orders.Api

RUN dotnet publish "Orders.Api.csproj" \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS final
WORKDIR /app

# ASP.NET Core images use port 8080 by default in .NET 8+.
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# .NET 8+ images provide the non-root app user.
USER app

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Orders.Api.dll"]
```

Build and run:

```bash
docker build --pull --progress=plain \
  --build-arg DOTNET_VERSION=10.0 \
  --tag orders-api:local .

docker run --rm \
  --name orders-api \
  --publish 8080:8080 \
  --env ASPNETCORE_ENVIRONMENT=Development \
  orders-api:local
```

Rules illustrated by the example:

- SDK and runtime major versions must match the target framework.
- The final image contains the published output, not the SDK or source tree.
- EXPOSE documents a container port; it does not publish it. Use -p host:container or Compose ports.
- .NET 6 and 7 commonly use port 80 by default; .NET 8+ uses 8080. Check the actual configuration before copying an old mapping.
- UseAppHost=false is appropriate when the container starts the app through dotnet App.dll.
- The sample uses BuildKit cache mounts; use a current Docker engine and keep the cache mount outside the final image.

## .dockerignore

```gitignore
**/bin/
**/obj/
.git/
.gitignore
.vs/
.idea/
*.user
*.suo
*.pdb
*.log
.env
.env.*
!/.env.example
docker-compose*.yml
docker-compose*.yaml
```

Do not ignore source files, shared MSBuild files, NuGet.config, or project files that the Dockerfile must copy. If COPY fails, check both the build context and .dockerignore.

## ASP.NET Core Health Checks

Expose an application endpoint and separate liveness from readiness:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

app.MapGet("/", () => Results.Ok(new { service = "orders-api" }));

app.Run();
```

Keep liveness independent from PostgreSQL, RabbitMQ, Redis, or other downstream systems. Add dependency checks only to readiness, with short timeouts. A temporary dependency outage should not cause an orchestrator restart loop.

## Compose: API and PostgreSQL

Use Compose service names for container-to-container traffic. localhost inside the API container means the API container itself, not PostgreSQL.

.env.example:

```dotenv
POSTGRES_DB=orders
POSTGRES_USER=orders
POSTGRES_PASSWORD=change-me-for-local-development
API_PORT=8080
```

compose.yaml:

```yaml
services:
  api:
    build:
      context: .
      dockerfile: Dockerfile
      args:
        DOTNET_VERSION: "10.0"
    image: orders-api:${IMAGE_TAG:-local}
    ports:
      - "${API_PORT:-8080}:8080"
    environment:
      ASPNETCORE_ENVIRONMENT: ${ASPNETCORE_ENVIRONMENT:-Development}
      ConnectionStrings__Orders: Host=postgres;Port=5432;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}
    depends_on:
      postgres:
        condition: service_healthy
    init: true
    restart: unless-stopped

  postgres:
    image: postgres:17-alpine
    environment:
      POSTGRES_DB: ${POSTGRES_DB}
      POSTGRES_USER: ${POSTGRES_USER}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
    volumes:
      - orders-postgres:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U $${POSTGRES_USER} -d $${POSTGRES_DB}"]
      interval: 5s
      timeout: 5s
      retries: 10
      start_period: 10s

volumes:
  orders-postgres:
```

```bash
docker compose config
cp .env.example .env
docker compose up --build -d
docker compose ps
docker compose logs -f api
curl http://localhost:8080/health/live
docker compose down
```

depends_on with service_healthy controls startup order only. The API must still retry transient database or broker connections. docker compose down -v removes named volumes and is destructive for local database data.

## Local Development

Do not use the production runtime image for dotnet watch because it does not contain the SDK. Add a development stage:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS development
WORKDIR /src
COPY . .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

CMD ["dotnet", "watch", "--project", "src/Orders.Api/Orders.Api.csproj", "run", "--urls", "http://0.0.0.0:8080"]
```

Development Compose override:

```yaml
services:
  api:
    build:
      context: .
      target: development
    ports:
      - "8080:8080"
    volumes:
      - .:/src
      - nuget-cache:/root/.nuget/packages
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      DOTNET_USE_POLLING_FILE_WATCHER: "1"
    command:
      - dotnet
      - watch
      - --project
      - src/Orders.Api/Orders.Api.csproj
      - run
      - --urls
      - http://0.0.0.0:8080

volumes:
  nuget-cache:
```

Polling is often more reliable on Docker Desktop, WSL, and network filesystems. Do not mount host bin or obj directories into the container; they may contain binaries for the wrong OS or architecture.

## Configuration and Secrets

- Use environment variables or a mounted secret provider for configuration. Nested .NET keys use double underscores, for example ConnectionStrings__Orders.
- Keep .env out of Git and out of the image. Commit only .env.example with placeholders.
- Never put passwords in ARG, ENV, Dockerfile text, image labels, or command-line arguments.
- Use a BuildKit secret for private NuGet feeds:

```dockerfile
RUN --mount=type=secret,id=nuget_config,target=/root/.nuget/NuGet.Config \
    --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore "src/Orders.Api/Orders.Api.csproj"
```

```bash
docker build \
  --secret id=nuget_config,src=NuGet.config \
  --tag orders-api:local .
```

Do not copy local HTTPS development certificates into a production image. Terminate TLS at the ingress/reverse proxy or mount managed certificates through the deployment platform.

## Non-Root and Writable Paths

Run as the non-root app user on .NET 8+ images. If the application must write files, create a dedicated directory and grant ownership:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN mkdir /app/data && chown app:app /app/data
COPY --from=publish /app/publish .

USER app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Orders.Api.dll"]
```

Prefer object storage or a named volume for persistent data. A container writable layer is ephemeral.

## CI/CD Verification

Build and smoke-test the same image that will be deployed:

```bash
docker build --pull --tag "$IMAGE_NAME:$GIT_SHA" .
docker run --detach --name orders-api-check \
  --publish 18080:8080 \
  --env ASPNETCORE_ENVIRONMENT=Production \
  "$IMAGE_NAME:$GIT_SHA"

curl --fail --retry 10 --retry-delay 2 http://localhost:18080/health/live
docker logs orders-api-check
docker stop --time 15 orders-api-check
docker rm orders-api-check
```

Run dotnet test in an SDK/test stage or CI job. Scan the final image, generate SBOM/provenance where supported, and publish immutable commit-SHA tags instead of deploying only latest.

## Troubleshooting

### COPY failed

The file is outside the build context or excluded by .dockerignore. Build from the solution root and verify docker build -f path/to/Dockerfile ..

### Connection refused to a dependency

Use the Compose service name and container port, such as Host=postgres;Port=5432. Do not use localhost or the host-published port from inside the API container.

### Container exits immediately

Inspect docker logs, verify the DLL name in ENTRYPOINT, check the target framework/runtime image pair, and inspect the effective command and environment.

### API is not reachable from the host

Check that Kestrel listens on 0.0.0.0, the host port is published, and the host-to-container mapping matches the configured port. EXPOSE alone is insufficient.

### Permission denied after switching to non-root

Grant ownership to app, mount a writable volume, or remove the write requirement. Do not run the entire container as root as a shortcut.

### Works on one architecture only

Check docker buildx build --platform, native NuGet dependencies, RID-specific publishing, and the architecture of every base image.

## Verification Checklist

Before delivering a Docker change, verify:

- the image builds from the intended context with a clean cache;
- the final image contains no SDK, source tree, tests, secrets, or local certificates;
- SDK/runtime versions match the target framework;
- the service binds to 0.0.0.0 and documents the actual container port;
- configuration comes from the intended environment or secret provider;
- Compose dependencies have health checks where startup readiness matters;
- transient dependency failures are retried by the application;
- SIGTERM causes graceful shutdown within the configured grace period;
- the container runs as non-root unless documented otherwise;
- health, logs, shutdown, writable paths, and image scanning were tested.

## Official References

- [.NET and Docker overview](https://learn.microsoft.com/en-us/dotnet/core/docker/introduction)
- [Containerize a .NET app](https://learn.microsoft.com/en-us/dotnet/core/docker/build-container)
- [.NET container images](https://learn.microsoft.com/en-us/dotnet/core/docker/container-images)
- [Docker multi-stage builds](https://docs.docker.com/build/building/multi-stage/)
- [Compose startup order and health checks](https://docs.docker.com/compose/how-tos/startup-order/)
