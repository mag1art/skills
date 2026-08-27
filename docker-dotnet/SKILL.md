---
name: docker-dotnet
version: 1.2.0
description: "Use for containerizing and reviewing C#/.NET services with Docker and Compose, including multi-stage builds, local development, health checks, secrets, and runtime hardening."
author: mag1art
license: Apache-2.0
tags: [dotnet, csharp, docker, containers, compose, buildkit, deployment, health-checks, security]
triggers:
  - Dockerfile for .NET
  - docker compose .NET
  - containerize ASP.NET Core
  - Docker health check
  - multi-stage Dockerfile
  - Docker development environment
metadata:
  hermes:
    tags: [dotnet, csharp, docker, containers, compose, buildkit, deployment, security]
---
# Docker for .NET
## When to Use
Use for ASP.NET Core, worker services, console apps, background consumers, multi-project solutions, Dockerfiles, Compose, CI image builds, and container hardening.
## Rules
- Use SDK images only for restore/build/test/publish; use aspnet for ASP.NET Core and runtime for non-web applications.
- Match SDK/runtime major versions to the target framework.
- Use a multi-stage build; keep the final image runtime-only.
- Build from the solution root when shared projects or MSBuild files are needed.
- Keep configuration in environment variables or secret providers.
- Never copy credentials, .env files, local certificates, bin, or obj into images.
- Run as non-root where the image and application support it.
- Treat health/readiness and graceful shutdown as application concerns.
- Pin production image digests when reproducibility and supply-chain control matter.
- Do not run migrations independently from every application replica.
## Build Context and Layout
The final argument of docker build and build.context in Compose define the build context. A Dockerfile under src/Orders.Api may still require the repository root as context. Remove COPY lines for files that do not exist and add every referenced project needed by restore.
## Production Dockerfile
This example targets an ASP.NET Core API in src/Orders.Api. Change DOTNET_VERSION and the DLL name.
```dockerfile
ARG DOTNET_VERSION=10.0
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS restore
WORKDIR /src
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["NuGet.config", "./"]
COPY ["src/Orders.Api/Orders.Api.csproj", "src/Orders.Api/"]
RUN --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore src/Orders.Api/Orders.Api.csproj --configfile NuGet.config
FROM restore AS publish
COPY . .
WORKDIR /src/src/Orders.Api
RUN dotnet publish Orders.Api.csproj -c Release --no-restore \
    -o /app/publish /p:UseAppHost=false
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Orders.Api.dll"]
```
```bash
docker build --pull --tag orders-api:local .
docker run --rm --name orders-api -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development orders-api:local
```
.NET 8+ ASP.NET Core images normally listen on 8080 and include the non-root app user. .NET 6/7 commonly use port 80. EXPOSE documents a port; it does not publish it. Use -p host:container.
## .dockerignore
```gitignore
**/bin/
**/obj/
.git/
.vs/
*.pdb
*.log
.env
.env.*
!/.env.example
```
Do not ignore source, project files, shared MSBuild files, or NuGet.config when the Dockerfile copies them.
## Health Endpoint
Expose liveness and readiness separately:
```csharp
builder.Services.AddHealthChecks();
var app = builder.Build();
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");
app.Run();
```
Liveness should not depend on PostgreSQL, RabbitMQ, Redis, or another downstream service. Add dependency checks only to readiness, with short timeouts. The application must still retry transient dependency failures after startup.
## Compose with PostgreSQL
Inside Compose, use the service name and container port; localhost means the current container.
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
volumes:
  orders-postgres:
```
```bash
cp .env.example .env
docker compose config
docker compose up --build -d
docker compose logs -f api
curl http://localhost:8080/health/live
docker compose down
```
service_healthy controls startup order only. The API must still retry transient database or broker connections. docker compose down -v removes named volumes and local database data.
## Local Development
Use an SDK stage for dotnet watch, not the production runtime:
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS development
WORKDIR /src
COPY . .
EXPOSE 8080
CMD ["dotnet", "watch", "--project", "src/Orders.Api/Orders.Api.csproj", "run", "--urls", "http://0.0.0.0:8080"]
```
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
volumes:
  nuget-cache:
```
Polling is often more reliable on Docker Desktop, WSL, and network filesystems. Do not mount host bin or obj directories.
## Secrets and Writable Paths
Do not put passwords in ARG, ENV, Dockerfile text, labels, or command-line arguments. For a private NuGet feed, use a BuildKit secret:
```dockerfile
RUN --mount=type=secret,id=nuget_config,target=/root/.nuget/NuGet.Config \
    --mount=type=cache,id=nuget,target=/root/.nuget/packages \
    dotnet restore src/Orders.Api/Orders.Api.csproj
```
```bash
docker build --secret id=nuget_config,src=NuGet.config -t orders-api:local .
```
For files the app must write, create a dedicated directory and grant ownership to app. Prefer object storage or a named volume for persistent data.
## CI and Troubleshooting
```bash
docker build --pull -t "$IMAGE_NAME:$GIT_SHA" .
docker run -d --name api-check -p 18080:8080 "$IMAGE_NAME:$GIT_SHA"
curl --fail --retry 10 http://localhost:18080/health/live
docker stop --time 15 api-check
docker rm api-check
```
- COPY failures: check build context and .dockerignore.
- Connection refused: use Host=postgres;Port=5432, not localhost or the host-published port.
- Unreachable API: verify Kestrel binds to 0.0.0.0 and the port mapping matches.
- Permission denied: grant ownership to app or mount a writable volume; do not run the whole container as root.
- Architecture errors: check buildx platform, native dependencies, RIDs, and base-image architecture.
## Quality Gate
Before delivery, verify image build, runtime-only final contents, matching target framework, configuration/secrets, health semantics, dependency retries, SIGTERM shutdown, non-root execution, logs, and image scanning.
## Official References
- [Microsoft: .NET and Docker](https://learn.microsoft.com/en-us/dotnet/core/docker/introduction)
- [Microsoft: containerize a .NET app](https://learn.microsoft.com/en-us/dotnet/core/docker/build-container)
- [Microsoft: .NET container images](https://learn.microsoft.com/en-us/dotnet/core/docker/container-images)
- [Docker: multi-stage builds](https://docs.docker.com/build/building/multi-stage/)
- [Docker Compose: startup order](https://docs.docker.com/compose/how-tos/startup-order/)
