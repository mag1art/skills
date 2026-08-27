---
name: gitlab-ci-dotnet
version: 1.0.0
description: "Use for GitLab CI/CD pipelines that restore, build, test, package, scan, and deploy C#/.NET services."
author: mag1art
license: Apache-2.0
tags: [gitlab, gitlab-ci, dotnet, ci-cd, testing, docker, artifacts, caching]
triggers:
  - GitLab CI .NET
  - .gitlab-ci.yml
  - dotnet pipeline
  - GitLab build test
  - Docker build GitLab
metadata:
  hermes:
    tags: [gitlab, gitlab-ci, dotnet, ci-cd, testing, docker, artifacts]
---

# GitLab CI for .NET

## When to Use

Use for GitLab CI configuration, build/test jobs, caching, artifacts, service containers, Docker builds, deployments, and pipeline troubleshooting for .NET projects.

## When Not to Use

Do not change production deployment or secret settings without understanding the environment and approval path.

## Pipeline Shape

Prefer explicit validate, build, test, package, and deploy stages. Keep restore, format/analyzer checks, build, tests, coverage, packaging, and deployment as reviewable jobs. Use rules to avoid deployments from untrusted branches.

## .NET Rules

- Pin or control the SDK with global.json and a deliberate container image.
- Cache NuGet packages with a key including lockfiles or project files.
- Use dotnet restore, dotnet build --no-restore, and dotnet test --no-build where job boundaries permit.
- Publish test and coverage artifacts with bounded retention.
- Start PostgreSQL/RabbitMQ service containers only for jobs that need them.
- Keep secrets in masked/protected CI variables, never YAML or logs.
- Use needs for the actual dependency graph.
- Make retries explicit and avoid hiding flaky tests with unlimited retries.
- Keep Docker-in-Docker or socket access restricted and intentional.

## Quality Gate

Check YAML syntax, MR and protected-branch rules, cache correctness, artifact paths, test results, dependency readiness, secret masking, and deployment approvals.
## Example: Build and Test Pipeline

```yaml
stages: [validate, build, test, image]

variables:
  NUGET_PACKAGES: "$CI_PROJECT_DIR/.nuget/packages"

cache:
  key:
    files:
      - "*.sln"
      - "**/*.csproj"
      - "Directory.Packages.props"
  paths:
    - .nuget/packages/

validate:
  stage: validate
  image: mcr.microsoft.com/dotnet/sdk:10.0
  script:
    - dotnet format --verify-no-changes

build:
  stage: build
  image: mcr.microsoft.com/dotnet/sdk:10.0
  script:
    - dotnet restore
    - dotnet build --no-restore --configuration Release

test:
  stage: test
  image: mcr.microsoft.com/dotnet/sdk:10.0
  script:
    - dotnet test --no-restore --configuration Release --logger junit
  artifacts:
    when: always
    reports:
      junit: "**/TestResults/*.xml"
```

Use an SDK image compatible with global.json. Add PostgreSQL or RabbitMQ service containers only to jobs that need them, and make deployment rules explicit for protected branches.

