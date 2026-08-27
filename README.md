# mag1art/skills

Reusable skills for AI coding agents and development harnesses.

## Skills

| Skill | Purpose |
|---|---|
| dotnet-expert | General production C#/.NET engineering and review |
| aspnet-core-api | ASP.NET Core API design, contracts, middleware, auth, and errors |
| efcore-postgresql | EF Core and PostgreSQL queries, migrations, concurrency, and performance |
| dotnet-testing | Unit, integration, contract, and container-based testing |
| dotnet-observability | Serilog, OpenTelemetry, metrics, tracing, health, and diagnostics |
| dotnet-performance | Async pipelines, bounded concurrency, streaming, pooling, and allocation control |
| dotnet-messaging | RabbitMQ and reliable asynchronous microservice messaging |
| docker-dotnet | Containerizing and running .NET services |
| gitlab-ci-dotnet | GitLab CI pipelines for .NET services |
| dotnet-version-guide | .NET Core/.NET version history, compatibility, and upgrade guidance |
| aspnet-mvc | Classic ASP.NET MVC and ASP.NET Core MVC development |
| blazor | Blazor Web Apps, render modes, components, forms, and interop |
| gitlab-basics | GitLab and git day-to-day operations |
| pandoc | Safe Markdown, DOCX, PDF, and HTML conversion |

## Layout

Every skill is a directory containing a SKILL.md file with YAML frontmatter and operational instructions.

## Validation

The repository uses a dependency-free C# validator targeting .NET 8:

~~~bash
dotnet run --project tools/SkillValidator/SkillValidator.csproj --configuration Release
~~~

The validator checks filenames, required metadata, unique skill names, list fields, and unresolved local skill references.

## License

Apache-2.0.